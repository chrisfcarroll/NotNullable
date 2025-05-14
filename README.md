# NotNullable. An almost transparent way to enforce null-checking in C# maybe-null reference types

- `NotNullable<T> where T : class` 
- `EventualNotNullable<T> where T : class`

### Summary

Similar to `Nullable<T> where T : struct` provided by C# for structs, but:
  
1. They are for reference types,
2. They are slightly stricter in that there is no implicit cast back to `<typeparam name="T"/>`.
   You must read `wrapper.Value` to get the value, after first checking `HasValue`.
   An InvalidOperationException is thrown if you read the Value of an instance of which
   `HasValue` is false.

Can be used a return type of methods that cannot guarantee returning a
not-null value, and which instead want to insist that consuming code must check for null
before using the returned value.
By returning a `NotNullable<T>` instead of a possibly-null instance of <c>T</c>
you more explicitly require the consuming code to do a null check before accessing 
the value.
  
<p>Why? Intended as a more C# idomatic alternative to other fp-style Monads 
for code that wants to enforces null checking.</p>

### Consumption: use either of these idioms
```
var maybe = datasource.Get(key);
if(maybe.HasValue){ ... use maybe.Value ... }else{ ... handle not found case ...}
if(maybe.HasNoValue){ ... handle not found case ... }else{... use maybe.Value ...}
```

### Production: use one of these idioms
```
public NotNullable<T> SomeMethod<T>() => doIHaveSomethingToReturn ? value : NotNullable<T>.Default; 
public NotNullable<T> OtherMethod<T>() => new NotNullable<T>( maybeNullValue); 
public NotNullable<T> OtherMethod<T>() => NotNullable.Create( maybeNullValue); 
```

#### Remarks
Note that because you are not allowed to access the wrapped value without
first checking `HasValue` or `NoValue`, the equality
contract with the base type `<typeparamref name="T"/>` is not symmetric.

## `EventualNotNullable<T>`

This is a three-state version of `NotNullable<T>`. The additional third state is the 
default, uninitialized state of a struct (recall that a struct has a state before 
any declared constructors are run) and is marked by `IsResolved` being false.
`IsResolved` is _only_ false for a default instance. Once the constructor instance 
is constructed with a value, (whether an actual value or null), `IsResolved`
will be true, and the value of `HasValue` and `NoValue` will indicate whether the contained
value is null or not.

```
default( NotNullable<T> ) == new NotNullable<T>( null ) // true
default( EventualNotNullable<T> ) == new EventualNotNullable<T>( null ) // false
```

#### Why not `Maybe<T>`?

Because people using `Maybe<T>` probably expect it to <a href="https://en.wikipedia.org/wiki/Monad_(functional_programming)#An_example:_Maybe">
conform to monad rules</a>, such as the language defining a monadic bind operator.
