using TestBase;
using Assert = TestBase.Assert;

[TestClass]
public class NotNullableTests
{
    [TestMethod]
    public void HasValueAndNoValueAreCorrectForNulls()
    {
        var uut = new NotNullable<string>(null);
        uut.HasValue.ShouldBeFalse();
        uut.NoValue.ShouldBeTrue();
    }
    
    [TestMethod]
    public void HasValueAndNoValueAreCorrectForValues()
    {
        var uut = new NotNullable<string>("");
        uut.HasValue.ShouldBeTrue();
        uut.NoValue.ShouldBeFalse();
    }
    
    [TestMethod]
    public void HasValueAndNoValueAreCorrectForDefaultValue()
    {
        var uut = default(NotNullable<string>);
        uut.HasValue.ShouldBeFalse();
        uut.NoValue.ShouldBeTrue();
        
        NotNullable<string>.Default.HasValue.ShouldBeFalse();
        NotNullable<string>.Default.NoValue.ShouldBeTrue();
    }
    
    [TestMethod]
    public void DefaultValueInstanceIsDefaultValue()
    {
        (NotNullable<string>.Default == default).ShouldBeTrue();
        (default == NotNullable<string>.Default).ShouldBeTrue();

        NotNullable<string>.Default.ShouldBe(default(NotNullable<string>));
        default(NotNullable<string>).ShouldBe(NotNullable<string>.Default);
    }
    
    [TestMethod]
    public void NullValueInstanceIsDefaultValue()
    {
        var nullValue= new NotNullable<string>(null);
        
        (nullValue == default).ShouldBeTrue();
        (default == nullValue).ShouldBeTrue();
        
        nullValue.ShouldBe(default(NotNullable<string>));
        default(NotNullable<string>).ShouldBe(nullValue);
        
        nullValue.HasValue.ShouldBeFalse();
        nullValue.NoValue.ShouldBeTrue();
        
        (NotNullable<string>.Default == default).ShouldBeTrue();
        NotNullable<string>.Default.ShouldBe(default(NotNullable<string>));
        NotNullable<string>.Default.HasValue.ShouldBeFalse();
        NotNullable<string>.Default.NoValue.ShouldBeTrue();
        
        (default == NotNullable<string>.Default).ShouldBeTrue();
        default(NotNullable<string>).ShouldBe(NotNullable<string>.Default);
    }
    
    [TestMethod]
    [DataRow("")]
    [DataRow("Something")]
    public void NotNullableEqualsWorksOneWayOnNotNullValues(string baseValue)
    {
        var nn = NotNullable.Create(baseValue);
        
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
    public void NotNullableEqualsWorksOneWayOnNulls()
    {
        string? value = null;
        var nn = NotNullable.Create(value);
        
        //
        ( nn.Equals( value ) ).ShouldBeTrue();
        ( nn == value ).ShouldBeTrue();
        
        //
        (value == nn.ToString() ).ShouldBeTrue();
    }
    
    [TestMethod]
    public void NotNullableEqualityOperatorWorksBothWaysWhetherNullOrNull()
    {
        string? value = "value";
        var nnv = NotNullable.Create(value);
        var nnn = NotNullable.Create(null as string);
        
        //
        ( nnv == value ).ShouldBeTrue();
        ( value == nnv ).ShouldBeTrue();
        //
        ( nnn == value ).ShouldBeFalse();
        ( value == nnn ).ShouldBeFalse();
        //
        ( nnn == null ).ShouldBeTrue();
        ( nnn == default ).ShouldBeTrue();
    }
    
    [TestMethod]
    public void NotNullableEqualityWorksOnDefault()
    {
        var defaultValue = new NotNullable<string>();
        (defaultValue.Equals(default)).ShouldBeTrue();
        (defaultValue == default).ShouldBeTrue();
        (defaultValue != default).ShouldBeFalse();
        defaultValue.Equals(NotNullable<string>.Default).ShouldBeTrue();
    }
    
    
    [TestMethod]
    public void ReferencingANotNullValueWhenNullThrows()
    {
        var uut = new NotNullable<string>(null);
        Assert.Throws<InvalidOperationException>( () => _ = uut.Value);
    }
}
