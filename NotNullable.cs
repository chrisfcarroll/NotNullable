using System.Diagnostics.CodeAnalysis;

/// <summary>
/// <p>Similar to the <see cref="Nullable{T}"/> for structs, but (1) for reference types,
/// and (2) slightly stricter (there is no implicit cast back to <typeparam name="T"></typeparam>)
/// </p>
/// <p>Intended to be used as a return type for methods that cannot guarantee returning a
/// not-null value, and which wants to insist that consuming  code must check for null
/// before using the returned value.
/// By returning a <see cref="NotNullable{T}"/> instead of a possibly-null instance of <c>T</c>
/// you force the consuming code to do a null check before accessing the value 
/// </p>
/// <p>Why? As an alternative to using fp-style Monads for code that wants to
/// enforces null checking.</p>
/// Consumption: use either of these idioms
/// <code>
/// var tutee = GetProfileData.Get(personNumber)
/// if(tutee.HasValue){ ... use tutee.Value ... }else{ ... handle not found case ...}
/// if(tutee.NoValue){ ... handle not found case ... }else{... use tutee.Value}
/// </code>
/// Production: use one of these idioms
/// <code>
/// public NotNullable&lt;T> SomeMethod&lt;T>() => doIHaveSomethingToReturn ? value : NotNullable&lt;T>.Default; 
/// public NotNullable&lt;T> OtherMethod&lt;T>() => new NotNullable&lt;T>( maybeNullValue); 
/// public NotNullable&lt;T> OtherMethod&lt;T>() => NotNullable.Create( maybeNullValue); 
/// </code>
/// </summary>
/// <remarks>
/// Note that because you are not allowed to access the wrapped value without
/// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
/// contract with the base type <typeparamref name="T"/> is not symmetric.
/// <seealso cref="Equals(MyStudents.BL.Extensions.NotNullable{T})"/>
/// </remarks>
/// <typeparam name="T"></typeparam>
public readonly struct NotNullable<T> : IEquatable<NotNullable<T>> where T : class
{
    /// <summary>
    /// The default value of a <c>new NotNullable<T>()</c> with no value passed to the constructor.
    /// </summary>
    public static readonly NotNullable<T> Default = new();

    readonly bool hasValue;

    internal readonly T? value;

    /// <summary>
    /// If <paramref name="value"/> is null, then creates an instance for which
    /// <see cref="HasValue"/> is false and <see cref="NoValue"/> is true.
    /// If <paramref name="value"/> is not null, then creates an instance for which
    /// <see cref="HasValue"/> is true, <see cref="NoValue"/> is false, and
    /// <see cref="Value"/> is <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public NotNullable(T? value)
    {
        hasValue = value is not null;
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
    public static implicit operator NotNullable<T>(T? value) => new (value);
    
    /// <remarks>
    /// Note that because you are not allowed to access the wrapped value without
    /// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
    /// contract with the base type <typeparamref name="T"/> is not symmetric.:
    /// <code>
    /// T value = new T(...);
    /// NotNullable&lt;T> wrapper = NotNullable.Create(value);
    /// var wrapperEqualsValue = ( value.Equals(wrapper) ); /* returns false */
    /// var valueEqualsWrapper = ( wrapper.Equals(value) ); /* returns true */
    /// </code>
    /// However
    /// <code>
    /// var wrapperEqualsValue = ( value == wrapper ) ; /* returns true */
    /// var valueEqualsWrapper = ( wrapper == value ); /* returns true */
    /// </code>
    /// because in this case the <c>value</c> is first implicitly cast to
    /// a <see cref="NotNullable{T}"/> before the equality operator is applied.  
    /// </remarks>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(NotNullable<T> other)
    {
        return hasValue == other.hasValue 
               && EqualityComparer<T?>.Default.Equals(value,other.value);
    }

    /// <param name="obj"></param>
    /// <remarks>
    /// Note that because you are not allowed to access the wrapped value without
    /// first checking <see cref="HasValue"/> (or <see cref="NoValue"/>), the equality
    /// contract with the base type <typeparamref name="T"/> is not symmetric.:
    /// <code>
    /// T value = new T(...);
    /// NotNullable&lt;T> wrapper = NotNullable.Create(value);
    /// var wrapperEqualsValue = ( value.Equals(wrapper) ); /* returns false */
    /// var valueEqualsWrapper = ( wrapper.Equals(value) ); /* returns true */
    /// </code>
    /// However
    /// <code>
    /// var wrapperEqualsValue = ( value == wrapper ) ; /* returns true */
    /// var valueEqualsWrapper = ( wrapper == value ); /* returns true */
    /// </code>
    /// because in this case the <c>value</c> is first implicitly cast to
    /// a <see cref="NotNullable{T}"/> before the equality operator is applied.  
    /// </remarks>
    /// <returns>
    /// <c> if <paramref name="obj"/> is either a <see cref="NotNullable{T}"/> wrapping
    /// the same value, or else a <typeparamref name="T"/> that is equal to <see cref="Value"/>
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is NotNullable<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hasValue,hasValue,value);
    }

    public static bool operator ==(NotNullable<T> left,NotNullable<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NotNullable<T> left,NotNullable<T> right)
    {
        return !left.Equals(right);
    }
}

public static class NotNullable
{
    /// <summary>
    /// If <paramref name="value"/> is null, then creates an instance for which
    /// <see cref="HasValue"/> is false and <see cref="NoValue"/> is true.
    /// If <paramref name="value"/> is not null, then creates an instance for which
    /// <see cref="HasValue"/> is true, <see cref="NoValue"/> is false, and
    /// <see cref="Value"/> is <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    public static NotNullable<T> Create<T>(T? value)  where T : class => new(value);
}