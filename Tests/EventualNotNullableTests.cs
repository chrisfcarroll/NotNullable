using TestBase;
using Assert = TestBase.Assert;
// ReSharper disable SuspiciousTypeConversion.Global

namespace MyStudents.BL.Tests.ExtensionTests;

[TestClass]
public class EventualEventualNotNullableTests
{
    [TestMethod]
    public void HasValueAndNoValueAndIsResolvedAreCorrectForNulls()
    {
        var uut = new EventualNotNullable<string>(null);
        uut.HasValue.ShouldBeFalse();
        uut.NoValue.ShouldBeTrue();
        uut.IsResolved.ShouldBeTrue();
    }
    
    [TestMethod]
    public void HasValueAndNoValueAndIsResolvedAreCorrectForValues()
    {
        var uut = new EventualNotNullable<string>("");
        uut.HasValue.ShouldBeTrue();
        uut.NoValue.ShouldBeFalse();
        uut.IsResolved.ShouldBeTrue();
    }
    
    [TestMethod]
    public void HasValueAndNoValueAndIsResolvedAreCorrectForDefaultValue()
    {
        var uut = default(EventualNotNullable<string>);
        uut.HasValue.ShouldBeFalse();
        uut.NoValue.ShouldBeTrue();
        uut.IsResolved.ShouldBeFalse();
        
        EventualNotNullable<string>.Default.HasValue.ShouldBeFalse();
        EventualNotNullable<string>.Default.NoValue.ShouldBeTrue();
    }
    
    [TestMethod]
    public void DefaultValueInstanceIsDefaultValue()
    {
        (EventualNotNullable<string>.Default == default).ShouldBeTrue();
        (default == EventualNotNullable<string>.Default).ShouldBeTrue();
        (EventualNotNullable<string>.Default.Equals(default)).ShouldBeTrue();
        (default(EventualNotNullable<string>).Equals(EventualNotNullable<string>.Default)).ShouldBeTrue();

        EventualNotNullable<string>.Default.ShouldBe(default(EventualNotNullable<string>));
        default(EventualNotNullable<string>).ShouldBe(EventualNotNullable<string>.Default);
    }
    
    [TestMethod]
    public void NullValueInstanceIsNotDefaultValue()
    {
        var nullValue= new EventualNotNullable<string>(null);
        
        (nullValue == default).ShouldBeFalse();
        (default == nullValue).ShouldBeFalse();
        
        nullValue.ShouldNotBe(default(EventualNotNullable<string>));
        default(EventualNotNullable<string>).ShouldNotBe(nullValue);
        
        nullValue.HasValue.ShouldBeFalse();
        nullValue.NoValue.ShouldBeTrue();
        
        (EventualNotNullable<string>.Default == default).ShouldBeTrue();
        EventualNotNullable<string>.Default.ShouldBe(default(EventualNotNullable<string>));
        EventualNotNullable<string>.Default.HasValue.ShouldBeFalse();
        EventualNotNullable<string>.Default.NoValue.ShouldBeTrue();
        
        (default == EventualNotNullable<string>.Default).ShouldBeTrue();
        default(EventualNotNullable<string>).ShouldBe(EventualNotNullable<string>.Default);
    }
    
    [TestMethod]
    [DataRow("")]
    [DataRow("Something")]
    public void EventualNotNullableEqualsWorksOneWayOnNotNullValues(string baseValue)
    {
        var nn = EventualNotNullable.Create(baseValue);
        
        //
        ( nn.Equals( baseValue ) ).ShouldBeTrue();
        ( nn == baseValue ).ShouldBeTrue();
        
        ( baseValue.Equals(nn ) ).ShouldBeFalse();
        ( baseValue == nn ).ShouldBeTrue();
        
        ( baseValue.Equals( nn.Value ) ).ShouldBeTrue();
        ( baseValue == nn.Value ).ShouldBeTrue();
        Object.Equals(baseValue, nn.Value ).ShouldBeTrue();
        
        ( nn.Value.Equals( baseValue ) ).ShouldBeTrue();
        ( nn.Value == baseValue ).ShouldBeTrue();
        Object.Equals(nn.Value, baseValue ).ShouldBeTrue();

        // other cases
        (baseValue == nn.ToString() ).ShouldBeTrue();
        nn.ShouldNotEqual(default);
        (nn.Equals(default)).ShouldBeFalse();
    }
    
    [TestMethod]
    public void EventualNotNullableEqualsWorksOneWayOnNulls()
    {
        string? value = null;
        var nn = EventualNotNullable.Create(value);
        
        //
        ( nn.Equals( value ) ).ShouldBeTrue();
        ( nn == value ).ShouldBeTrue();
        
        //
        (value == nn.ToString() ).ShouldBeTrue();
    }
    
    [TestMethod]
    public void EventualNotNullableEqualityOperatorWorksBothWaysWhetherNullOrNull()
    {
        string? value = "value";
        var nnv = EventualNotNullable.Create(value);
        var nnn = EventualNotNullable.Create(null as string);
        
        //
        ( nnv == value ).ShouldBeTrue();
        ( value == nnv ).ShouldBeTrue();
        //
        ( nnn == value ).ShouldBeFalse();
        ( value == nnn ).ShouldBeFalse();
        //
        ( nnn == null ).ShouldBeTrue();
        ( nnn == default ).ShouldBeFalse();
    }
    
    [TestMethod]
    public void EventualNotNullableEqualityWorksOnDefault()
    {
        var defaultValue = new EventualNotNullable<string>();
        (defaultValue.Equals(default)).ShouldBeTrue();
        (defaultValue == default).ShouldBeTrue();
        (defaultValue != default).ShouldBeFalse();
        defaultValue.Equals(EventualNotNullable<string>.Default).ShouldBeTrue();
    }
    
    
    [TestMethod]
    public void ReferencingANotNullValueWhenNullThrows()
    {
        var uut = new EventualNotNullable<string>(null);
        Assert.Throws<InvalidOperationException>( () => _ = uut.Value);
    }
}
