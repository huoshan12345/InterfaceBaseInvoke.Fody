namespace InterfaceBaseInvoke.Tests.Support;

/// <summary>
/// Used to collect all errors.
/// </summary>
internal class GuardedWeaver : ModuleWeaver
{
    protected override bool AddError(string message, SequencePoint? sequencePoint)
    {
        base.AddError(message, sequencePoint);
        return false;
    }
}
