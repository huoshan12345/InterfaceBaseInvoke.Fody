namespace InterfaceBaseInvoke.Tests.AssemblyToProcess;

public class ReOverrideMethodTestCases
{
    [Fact]
    public void Property_InvokeTwice()
    {
        var obj = new HasReOverrideMethod();
        var result = obj.Base<IHasOverrideMethod>().Property + "----" + obj.Base<IHasReOverrideMethod>().Property;
        Assert.Equal("Property----IHasReOverrideMethod.Property", result);
    }

    [Fact]
    public void ReOverrideMethod_InvokeTwice()
    {
        var obj = new HasReOverrideMethod();
        var a = obj.Base<IHasOverrideMethod>().Method(1, "a");
        var b = obj.Base<IHasReOverrideMethod>().Method(2, "b");
        var result = a + "----" + b;
        Assert.Equal("Method(1, a)----IHasReOverrideMethod.Method(2, b)", result);
    }
}
