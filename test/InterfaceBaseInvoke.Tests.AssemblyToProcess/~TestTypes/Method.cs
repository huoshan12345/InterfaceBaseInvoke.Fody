namespace InterfaceBaseInvoke.Tests.AssemblyToProcess;

public interface IHasDefaultMethod
{
    string Property => $"{nameof(Property)}";
    string Method(int x, string y) => $"{nameof(Method)}.{x}.{y}";
}

public class HasDefaultMethod : IHasDefaultMethod
{
    public string Property => throw new InvalidOperationException();
    public virtual string Method(int x, string y) => throw new InvalidOperationException();
}

public class DerivedHasDefaultMethod : HasDefaultMethod
{
    public override string Method(int x, string y) => throw new InvalidOperationException();
}

public interface IHasEmptyMethod
{
    string Property { get; }
    string Method(int x, string y);
}

public interface IHasOverrideMethod : IHasEmptyMethod
{
    string IHasEmptyMethod.Property => $"{nameof(Property)}";
    string IHasEmptyMethod.Method(int x, string y) => $"{nameof(Method)}({x}, {y})";
}

public class HasOverrideMethod : IHasOverrideMethod
{
    public string Property => throw new InvalidOperationException();
    public string Method(int x, string y) => throw new InvalidOperationException();
}

public interface IHasReOverrideMethod : IHasOverrideMethod
{
    string IHasEmptyMethod.Property => $"{nameof(IHasReOverrideMethod)}.{nameof(Property)}";
    string IHasEmptyMethod.Method(int x, string y) => $"{nameof(IHasReOverrideMethod)}.{nameof(Method)}({x}, {y})";
}

public class HasReOverrideMethod : IHasReOverrideMethod
{
    public string Property => throw new InvalidOperationException();
    public string Method(int x, string y) => throw new InvalidOperationException();
}
