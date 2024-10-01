namespace InterfaceBaseInvoke.Tests.AssemblyToProcess;

public class OverrideMethodTestCases
{
    [Fact]
    public void Property_Invoke()
    {
        var obj = new HasOverrideMethod();
        var result = obj.Base<IHasOverrideMethod>().Property;
        Assert.Equal("Property", result);
    }
    [Fact]
    public void OverrideMethod_Invoke()
    {
        var obj = new HasOverrideMethod();
        var result = obj.Base<IHasOverrideMethod>().Method(1, "a");
        Assert.Equal("Method(1, a)", result);
    }
    [Fact]
    public void OverrideMethod_InvokeTwice()
    {
        var obj = new HasOverrideMethod();
        var a = obj.Base<IHasOverrideMethod>().Method(1, "a");
        var b = obj.Base<IHasOverrideMethod>().Method(2, "b");
        var result = a + "----" + b;
        Assert.Equal("Method(1, a)----Method(2, b)", result);
    }
}
