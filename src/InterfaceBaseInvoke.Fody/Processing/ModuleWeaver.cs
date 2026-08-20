namespace InterfaceBaseInvoke.Fody.Processing;

public class ModuleWeaver : BaseModuleWeaver
{
    private readonly IWeaverLogger _log;

    public ModuleWeaver()
    {
        _log = new WeaverLogger(this);
    }

    public override void Execute()
    {
        using var context = new ModuleWeavingContext(ModuleDefinition, WeaverAnchors.AssemblyName, ProjectDirectoryPath);

        var processed = false;
        foreach (var type in ModuleDefinition.GetTypes())
        {
            foreach (var method in type.Methods)
            {
                if (method.HasBody == false)
                    continue;

                _log.Debug($"Processing: {method.FullName}");

                try
                {
                    processed = new MethodWeaver(context, method, _log).Process() || processed;
                }
                catch (WeavingException ex)
                {
                    var terminate = AddError(ex.ToString(), ex.SequencePoint);
                    if (terminate)
                        return;
                }
            }
        }

        if (processed == false)
        {
            _log.Warning("No type is processed.", null);
        }
    }

    public override IEnumerable<string> GetAssembliesForScanning() => [];

    protected virtual bool AddError(string message, SequencePoint? sequencePoint)
    {
        _log.Error(message, sequencePoint);
        return true;
    }
}
