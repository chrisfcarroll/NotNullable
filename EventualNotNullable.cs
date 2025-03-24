using System.Diagnostics.CodeAnalysis;

/// <summary>
/// <p><see cref="EventualNotNullable{T}"/> is a three-state version if <see cref="NotNullable{T}"/>. The
/// third state, <see cref="IsResolved"/>, can be used when a value might not yet be resolved either way,
/// for instance when waiting for an async call.
/// </p>
/// <p>Similar to the <see cref="Nullable{T}"/> for structs, but (1) for reference types,
/// and (2) slightly stricter (there is no implicit cast back to <typeparam name="T"></typeparam>)
/// and (3) as a promise to be resolved.</p>
/// <p>Intended to be used as a return type for async methods that cannot guarantee returning a
/// not-null value when they do return, and which wants to insist that consuming code must check for null
/// before using the returned value.
/// By returning a <see cref="EventualNotNullable{T}"/> instead of a possibly-null instance of <c>T</c>
/// you force the consuming code to do a null check before accessing the value 
/// </p>
/// <p>Why? As an alternative to using fp-style Monads for code that wants to
/// enforces null checking.</p>
/// Consumption: use either of these idioms
/// <code>
/// var tutee = await GetProfileData.Get(personNumber)
/// if(tutee.HasValue){ ... use tutee.Value ... }else{ ... handle not found case ...}
/// if(tutee.NoValue){ ... handle not found case ... }else{... use tutee.Value}
/// </code>
/// Production: use one of these idioms
/// <code>
/// public EventualNotNullable&lt;T> SomeMethod&lt;T>() => doIHaveSomethingToReturn ? value : EventualNotNullable&lt;T>.Default; 
/// public EventualNotNullable&lt;T> OtherMethod&lt;T>() => new EventualNotNullable&lt;T>( maybeNullValue); 
/// public EventualNotNullable&lt;T> OtherMethod&lt;T>() => EventualNotNullable.Create( maybeNullValue); 
/// </code>
/// </summary>
/// <remarks>
/// Note that because you are not allowed to access the wrapped value without
/// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
/// contract with the base type <typeparamref name="T"/> is not symmetric.
/// <seealso cref="Equals(EventualNotNullable{T})"/>
/// </remarks>
/// <typeparam name="T"></typeparam>
public readonly struct EventualNotNullable<T> : IEquatable<EventualNotNullable<T>> where T : class
{
    /// <summary>
    /// The default value of a <c>new EventualNotNullable<T>()</c>, which has <see cref="IsResolved"/> 
    // set to false.
    /// </summary>
    public static readonly EventualNotNullable<T> Default = new();

    readonly bool hasValue;
    
    public readonly bool IsResolved;

    internal readonly T? value;

    /// <summary>
    /// If <paramref name="value"/> is null, then creates an instance for which
    /// <see cref="HasValue"/> is false and <see cref="NoValue"/> is true.
    /// If <paramref name="value"/> is not null, then creates an instance for which
    /// <see cref="HasValue"/> is true, <see cref="NoValue"/> is false, and
    /// <see cref="Value"/> is <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public EventualNotNullable(T? value)
    {
        hasValue = value is not null;
        IsResolved = true;
        this.value = value!;
    }

    [MemberNotNullWhen(true, nameof(value), nameof(Value))]
    public readonly bool HasValue => hasValue;
    
    [MemberNotNullWhen(false, nameof(value), nameof(Value))]
    public readonly bool NoValue => !hasValue;

    /// <summary>
    /// The Value being wrapped.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <b>Note:</b> Reading the Value property of an instance wrapping null will throw an Exception.
    /// </exception>
    public readonly T Value => value ?? throw new InvalidOperationException("The nullable value has not been set.");
    
    /// <returns><c>null</c> if <see cref="Value"/> is <c>null</c>,
    /// otherwise returns <c>Value.ToString()</c>.
    /// </returns>
    public override string? ToString() => hasValue ? value!.ToString() : null;

    /// <returns><c>new (value)</c></returns>
    public static implicit operator EventualNotNullable<T>(T? value) => new (value);
    
    /// <remarks>
    /// Note that because you are not allowed to access the wrapped value without
    /// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
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
    /// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
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