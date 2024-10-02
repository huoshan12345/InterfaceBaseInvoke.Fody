namespace InterfaceBaseInvoke.Tests.AssemblyToProcess;

public struct HasOverrideMethodStruct : IHasOverrideMethod
{
    public string Property => throw new InvalidOperationException();
    public string Method(int x, string y) => throw new InvalidOperationException();
}

public class StructTestCases
{
    [Fact]
    public void Property_Invoke()
    {
        var obj = new HasOverrideMethodStruct();
        var result = obj.Base<IHasOverrideMethod>().Property;
        Assert.Equal("Property", result);
    }

    [Fact]
    public void OverrideMethod_Invoke()
    {
        var obj = new HasOverrideMethodStruct();
        var result = obj.Base<IHasOverrideMethod>().Method(1, "a");
        Assert.Equal("Method(1, a)", result);
    }

    [Fact]
    public void OverrideMethod_InvokeTwice()
    {
        var obj = new HasOverrideMethodStruct();
        var a = obj.Base<IHasOverrideMethod>().Method(1, "a");
        var b = obj.Base<IHasOverrideMethod>().Method(2, "b");
        var result = a + "----" + b;
        Assert.Equal("Method(1, a)----Method(2, b)", result);
    }
}
