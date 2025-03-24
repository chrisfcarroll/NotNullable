# NotNullable. An almost transparent way to enforce null-checking in C# might-be-null reference types

- `NotNullable<T> where T : class` 
- `EventualNotNullable<T> where T : class`

### Summary

Similar to `Nullable<T> where T : struct` provided by C# for structs, but:
  
1. They are for reference types,
2. They are slightly stricter. There is no implicit cast back to `<typeparam name="T"/>`,
   you have to read `wrapper.Value` to get the value. An InvalidOperationException
   is thrown if you read the Value of null.

Intended to be used as a return type for methods that cannot guarantee returning a
not-null value, but which instead want to insist that consuming  code must check for null
before using the returned value.
By returning a `NotNullable<T>` instead of a possibly-null instance of <c>T</c>
you force the consuming code to do a null check before accessing the value.
  
<p>Why? As a hopefully simpler and more C#-obvious alternative to using fp-style Monads 
for code that wants to enforces null checking.</p>

### Consumption: use either of these idioms
```
var maybe = datasource.Get(key);
if(maybe.HasValue){ ... use maybe.Value ... }else{ ... handle not found case ...}
if(maybe.NoValue){ ... handle not found case ... }else{... use maybe.Value ...}
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

This is a three-state version of `NotNullable<T>`. The additional third state is marked
by `IsResolved` being false, and is only true of a default, uninitialized instance. Once an
instance is constructed with a value, (whether or not the value is null), `IsResolved`
will be true, and the value of HasValue and NoValue will indicate whether the contained
value is null or not.

```
default( NotNullable<T> ) == new NotNullable<T>( null ) // true
default( EventualNotNullable<T> ) == new EventualNotNullable<T>( null ) // false
```
