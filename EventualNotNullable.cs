using System.Diagnostics.CodeAnalysis;

/// <summary>
/// <p><see cref="EventualNotNullable{T}"/> is a three-state version of <see cref="NotNullable{T}"/>.
/// The third state is for when a value might not yet be resolved either way, for instance
/// in multi-threaded async code, and is indicated when <see cref="IsResolved"/> is <c>false</c>.
/// </p>
/// <p>Similar to the <see cref="Nullable{T}"/> for structs, but (1) for reference types,
/// and (2) slightly stricter in that there is no implicit cast back to <typeparamref name="T" />)
/// and (3) as a promise to be resolved.</p>
/// <p>Intended to be used as a return type for async methods that cannot guarantee returning a
/// not-null value when they do return, and which wants to insist that consuming code must check
/// for null before using the returned value.
/// By returning an <see cref="EventualNotNullable{T}"/> instead of a possibly-null instance of
/// <typeparamref name="T"/>, you force the consuming code to do a null check before accessing
/// the value.</p>
/// <p>Why? As a more familiar idiom than other fp-style Monads for code that wants to
/// enforces 3-state null checking.</p>
/// <p><b>Consumption:</b></p>
/// Use this idiom:
/// <code>
/// // Thread 1:
/// var user = await GetUserDate(userId);
/// ...
/// // Thread 2:
/// if(user.IsNotResolved){ ... }
/// else if(user.HasNoValue){ ... }
/// else { ... can reference user.Value here ... }
/// // or
/// if(user.HasValue){ ... can reference user.Value here ... }
/// else if(user.IsResolved) { ... the user is definitely unknown ...}
/// else{ ... we are still waiting for the awaited call to complete ...}
/// </code>
/// <p><b>Production:</b></p>
/// Use one of these idioms
/// <code>
/// public EventualNotNullable&lt;T> SomeMethod&lt;T>() => doIHaveSomethingToReturn ? value : null;
/// public EventualNotNullable&lt;T> OtherMethod&lt;T>() => new EventualNotNullable&lt;T>( maybeNullValue); 
/// public EventualNotNullable&lt;T> OtherMethod&lt;T>() => EventualNotNullable.Create( maybeNullValue); 
/// </code>
/// Note that you will not usually <i>produce</i> an unresolved instance. The unresolved instance
/// is used on the consumer side when the consumer is still waiting for the producer.
/// </summary>
/// <remarks>
/// Note that because you are not allowed to access the wrapped value without
/// first checking <see cref="HasValue"/> (or <see cref="HasNoValue"/>), the equality
/// contract with the base type <typeparamref name="T"/> is not symmetric.
/// <seealso cref="Equals(EventualNotNullable{T})"/>
/// </remarks>
/// <typeparam name="T"></typeparam>
public readonly struct EventualNotNullable<T> : IEquatable<EventualNotNullable<T>> where T : class
{
    /// <summary>
    /// The default value of a <c>new EventualNotNullable&lt;T>()</c>, which has bypassed the
    /// declared constructor (recall that a struct can do that), for which
    /// <list type="bullet">
    /// <item><see cref="IsResolved"/> is true.</item>
    /// <item><see cref="HasValue"/> = false,</item>
    /// <item><see cref="HasNoValue"/> is true,</item>
    /// <item>calling <see cref="Value"/> will thrown an <see cref="InvalidOperationException"/></item>
    /// </list>
    /// 
    /// <p>In the following code, <c>fieldA</c> will be initialized to the value <see cref="Default"/>.</p>
    /// <code>
    /// class Example
    /// {
    ///   EventualNotNullablle&lt;string> fieldA;
    /// }
    /// </code>
    /// </summary>
    public static readonly EventualNotNullable<T> Default = new();

    /// <summary>
    /// The “HasNoValue” instance.
    /// The value of a <c>new EventualNotNullable&lt;T>( null )</c>, for which
    /// <list type="bullet">
    /// <item><see cref="IsResolved"/> is true.</item>
    /// <item><see cref="HasValue"/> = false,</item>
    /// <item><see cref="HasNoValue"/> is true,</item>
    /// <item>calling <see cref="Value"/> will thrown an <see cref="InvalidOperationException"/></item>
    /// </list>
    /// </summary>
    /// <remarks>
    ///  <c>HasNoValue == new(null)</c> is true.
    /// </remarks>
    public static readonly EventualNotNullable<T> NoValue = new(null);
    
    readonly bool hasValue;
    
    /// <summary>
    /// True if this instance is not <see cref="Default"/>.
    /// That is, if this instance contains either a null or a non-null value.
    /// <p>False if this instance was constructed with the no-parameter constructor, for instance as
    /// a result of field initialization.</p>
    /// <p>In this code, <c>fieldA</c> will be initialized to <see cref="Default"/> and IsResolved will be false.</p>
    /// <code>
    /// class Example
    /// {
    ///   EventualNotNullablle&lt;string> fieldA;
    /// }
    /// </code>
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <seealso cref="Default"/>
    /// <returns>True if and only if this instance is not <see cref="Default"/>.</returns>
    public readonly bool IsResolved;

    /// <summary>
    /// True if <see cref="IsResolved"/> is false. When this is true, this instance is
    /// equal to <see cref="Default"/>, and <see cref="HasNoValue"/> is also true.
    /// </summary>
    public bool IsNotResolved => !IsResolved;

    internal readonly T? value;

    /// <summary>
    /// If <paramref name="value"/> is null, then creates an instance for which
    /// <see cref="HasValue"/> is false and <see cref="HasNoValue"/> is true.
    /// If <paramref name="value"/> is not null, then creates an instance for which
    /// <see cref="HasValue"/> is true, <see cref="HasNoValue"/> is false, and
    /// <see cref="Value"/> is <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public EventualNotNullable(T? value)
    {
        hasValue = value is not null;
        IsResolved = true;
        this.value = value!;
    }

    /// <returns>
    /// True if this instance is neither <see cref="Default"/> or <see cref="NoValue"/>.
    /// That is, if this instance contains a non-null value.
    /// </returns>
    /// <remarks>
    /// Always implies that <see cref="HasNoValue"/> is false and that <see cref="IsResolved"/> is true.
    /// </remarks>
    /// <seealso cref="HasValue"/>
    [MemberNotNullWhen(true, nameof(value), nameof(Value))]
    public readonly bool HasValue => hasValue;
    
    
    /// <returns>
    /// True if this instance is either <see cref="Default"/> or <see cref="NoValue"/>.
    /// That is, if this instance contains no value. 
    /// </returns>
    /// <remarks>
    /// Always equal to <c>!HasValue</c>
    /// </remarks>
    /// <seealso cref="HasValue"/>
    [MemberNotNullWhen(false, nameof(value), nameof(Value))]
    public readonly bool HasNoValue => !hasValue;

    /// <summary>
    /// The Value being wrapped.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <b>Note:</b> Reading the Value property of an instance wrapping null will throw an Exception.
    /// </exception>
    public readonly T Value => value ?? throw new InvalidOperationException("A NotNullable value was accessed when it has a null value.");
    
    /// <returns><c>null</c> if <see cref="Value"/> is <c>null</c>,
    /// otherwise returns <c>Value.ToString()</c>.
    /// </returns>
    public override string? ToString() => hasValue ? value!.ToString() : null;

    /// <returns><c>new (value)</c></returns>
    public static implicit operator EventualNotNullable<T>(T? value) => new (value);
    
    /// <remarks>
    /// Note that because you are not allowed to access the wrapped value without
    /// first checking <see cref="HasValue"/> (or <see cref="HasNoValue"/>), the equality
    /// contract with the base type <typeparamref name="T"/> is not symmetric.:
    /// <code>
    /// T value = new T(...);
    /// EventualNotNullable&lt;T> wrapper = EventualNotNullable.Create(value);
    /// var wrapperEqualsValue = ( value.Equals(wrapper) ); /* returns false */
    /// var valueEqualsWrapper = ( wrapper.Equals(value) ); /* returns true */
    /// </code>
    /// However
    /// <code>
    /// var wrapperEqualsValue = ( value == wrapper ) ; /* returns true */
    /// var valueEqualsWrapper = ( wrapper == value ); /* returns true */
    /// </code>
    /// because in this case the <c>value</c> is first implicitly cast to
    /// a <see cref="EventualNotNullable{T}"/> before the equality operator is applied.  
    /// </remarks>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(EventualNotNullable<T> other)
    {
        return IsResolved == other.IsResolved 
               && hasValue == other.hasValue 
               && EqualityComparer<T?>.Default.Equals(value,other.value);
    }

    /// <param name="obj"></param>
    /// <remarks>
    /// Note that because you are not allowed to access the wrapped value without
    /// first checking <see cref="HasValue"/> (or <see cref="HasNoValue"/>), the equality
    /// contract with the base type <typeparamref name="T"/> is not symmetric.:
    /// <code>
    /// T value = new T(...);
    /// EventualNotNullable&lt;T> wrapper = EventualNotNullable.Create(value);
    /// var wrapperEqualsValue = ( value.Equals(wrapper) ); /* returns false */
    /// var valueEqualsWrapper = ( wrapper.Equals(value) ); /* returns true */
    /// </code>
    /// However
    /// <code>
    /// var wrapperEqualsValue = ( value == wrapper ) ; /* returns true */
    /// var valueEqualsWrapper = ( wrapper == value ); /* returns true */
    /// </code>
    /// because in this case the <c>value</c> is first implicitly cast to
    /// a <see cref="EventualNotNullable{T}"/> before the equality operator is applied.  
    /// </remarks>
    /// <returns>
    /// <c> if <paramref name="obj"/> is either a <see cref="EventualNotNullable{T}"/> wrapping
    /// the same value, or else a <typeparamref name="T"/> that is equal to <see cref="Value"/>
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is EventualNotNullable<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hasValue,hasValue,value);
    }

    public static bool operator ==(EventualNotNullable<T> left,EventualNotNullable<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EventualNotNullable<T> left,EventualNotNullable<T> right)
    {
        return !left.Equals(right);
    }
}

public static class EventualNotNullable
{
    /// <summary>
    /// If <paramref name="value"/> is null, then creates an instance for which
    /// <see cref="HasValue"/> is false and <see cref="NoValue"/> is true.
    /// If <paramref name="value"/> is not null, then creates an instance for which
    /// <see cref="HasValue"/> is true, <see cref="NoValue"/> is false, and
    /// <see cref="Value"/> is <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public static EventualNotNullable<T> Create<T>(T? value)  where T : class => new(value);
}
