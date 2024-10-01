﻿using System.Reflection;

namespace InterfaceBaseInvoke.Fody.Processing;

internal sealed class MethodWeaver
{
    private readonly ModuleDefinition _module;
    private readonly MethodDefinition _method;
    private readonly MethodWeaverLogger _log;
    private readonly WeaverILProcessor _il;
    private readonly References _references;
    private Collection<Instruction> Instructions => _method.Body.Instructions;
    private TypeReferences Types => _references.Types;
    private MethodReferences Methods => _references.Methods;

    public MethodWeaver(ModuleDefinition module, MethodDefinition method, IWeaverLogger log)
    {
        _module = module;
        _method = method;
        _il = new WeaverILProcessor(method);
        _log = new MethodWeaverLogger(log, _method);
        _references = new References(module);
    }

    public bool Process()
    {
        try
        {
            return ProcessImpl();
        }
        catch (InstructionWeavingException ex)
        {
            throw new WeavingException(_log.QualifyMessage(ex.Message, ex.Instruction))
            {
                SequencePoint = ex.Instruction.GetInputSequencePoint(_method)
            };
        }
        catch (WeavingException ex)
        {
            throw new WeavingException(_log.QualifyMessage(ex.Message))
            {
                SequencePoint = ex.SequencePoint
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error occured while processing method {_method.FullName}: {ex.Message}", ex);
        }
    }

    private static bool IsAnchorMethodCall(Instruction instruction)
    {
        return instruction.OpCode == OpCodes.Call
               && instruction.Operand is GenericInstanceMethod
               {
                   Name: WeaverAnchors.MethodName,
                   DeclaringType:
                   {
                       Name: WeaverAnchors.TypeName,
                       Scope.Name: WeaverAnchors.AssemblyName
                   }
               };
    }

    private bool ProcessImpl()
    {
        var processed = false;
        var instruction = Instructions.FirstOrDefault();
        Instruction? nextInstruction;

        for (; instruction != null; instruction = nextInstruction)
        {
            nextInstruction = instruction.Next;

            if (IsAnchorMethodCall(instruction) == false)
                continue;

            try
            {
                nextInstruction = ProcessAnchorMethod(instruction);
                processed = true;
            }
            catch (InstructionWeavingException)
            {
                throw;
            }
            catch (WeavingException ex)
            {
                throw new InstructionWeavingException(instruction, _log.QualifyMessage(ex.Message, instruction));
            }
            catch (Exception ex)
            {
                throw new InstructionWeavingException(instruction, $"Unexpected error occured while processing method {_method.FullName} at instruction {instruction}: {ex}");
            }
        }
        return processed;
    }

    private Instruction? ProcessAnchorMethod(Instruction instruction)
    {
        var anchorMethod = (GenericInstanceMethod)instruction.Operand;
        var interfaceTypeRef = anchorMethod.GenericArguments.First();
        var interfaceTypeDef = interfaceTypeRef.Resolve();
        if (!interfaceTypeDef.IsInterface)
            throw new InstructionWeavingException(instruction, "The method Base<T> requires that T is an interface type, but got " + interfaceTypeDef.FullName);

        var next = instruction.Next;
        if (next == null)
            throw NoInterfaceMethodInvocationException();

        // var x = obj.Base<T>();
        if (IsStloc(next))
            throw NoInterfaceMethodInvocationException();

        // obj.Base<T>().SomeMethod();
        if (IsCallAndPop(next) && !IsInterfaceMethodCandidate(next, interfaceTypeRef, interfaceTypeDef))
        {
            if (IsAnchorMethodCall(next))
                throw new InvalidOperationException("The method Base<T> cannot be invoked followed by another Base<T>");
            else
                throw NoInterfaceMethodInvocationException();
        }

        for (var p = next; p != null; p = p.Next)
        {
            if (!IsInterfaceMethodCandidate(p, interfaceTypeRef, interfaceTypeDef))
                continue;

            // We build a graph for the Instructions here to validate the method body as we emit new instructions.
            // The graph should be built every time here because Instructions may be updated.
            //var graph = Instructions.BuildGraph();
            //var args = p.GetArgumentPushInstructions(Instructions, graph);
            //var arg = args.First();

            //if (arg != instruction)
            //    continue;

            p = EmitBaseInvokeInstructions(instruction, interfaceTypeRef, interfaceTypeDef, p);
            return p.Next;
        }

        throw NoInterfaceMethodInvocationException();
    }

    private Instruction EmitBaseInvokeInstructions(Instruction anchor, TypeReference typeRef, TypeDefinition interfaceTypeDef, Instruction invokeInstruction)
    {
        _il.Remove(anchor);

        var methodRef = (MethodReference)invokeInstruction.Operand;

        if (typeRef.IsEqualTo(methodRef.DeclaringType))
        {
            var methodDef = methodRef.Resolve();
            EnsureNonAbstract(methodDef);

            invokeInstruction.OpCode = OpCodes.Call;
            return invokeInstruction;
        }

        var interfaceDefaultMethod = interfaceTypeDef.GetInterfaceDefaultMethod(methodRef);
        EnsureNonAbstract(interfaceDefaultMethod);

        var interfaceDefaultMethodRef = (MethodReference)interfaceDefaultMethod;
        if (methodRef is GenericInstanceMethod { HasGenericArguments: true } genericInstanceMethod)
        {
            interfaceDefaultMethodRef = interfaceDefaultMethod.MakeGenericMethod(genericInstanceMethod.GenericArguments);
        }

        // we have to use Calli instead of Call to avoid MethodAccessException
        // we cannot use Ldftn to get the method pointer because of MethodAccessException
        var handle = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.RuntimeMethodHandle));
        var ptr = _il.Locals.AddLocalVar(new LocalVarBuilder(Types.IntPtr));
        var callSite = new StandAloneMethodSigBuilder(CallingConventions.HasThis, interfaceDefaultMethodRef).Build();

        var to64 = _il.Create(OpCodes.Call, Methods.ToInt64);
        var to32 = _il.Create(OpCodes.Call, Methods.ToInt32);
        var calli = _il.Create(OpCodes.Calli, callSite);

        var instructions = new List<Instruction>
        {
            _il.Create(OpCodes.Ldtoken, interfaceDefaultMethodRef),
            _il.Create(OpCodes.Stloc, handle),
            _il.Create(OpCodes.Ldloca, handle),
            _il.Create(OpCodes.Call, Methods.FunctionPointer),
            _il.Create(OpCodes.Stloc, ptr),
            _il.Create(OpCodes.Ldloca, ptr),
            _il.Create(OpCodes.Call, Methods.Is64BitProcess),
            _il.Create(OpCodes.Brfalse, to32),
            to64,
            _il.Create(OpCodes.Br, calli),
            to32,
            calli,
            Instruction.Create(OpCodes.Nop)
        };

        var cur = _il.IL.InsertAfter(invokeInstruction, instructions);
        _il.Remove(invokeInstruction);
        return cur;
    }

    private static bool IsInterfaceMethodCandidate(Instruction instruction, TypeReference typeRef, TypeDefinition typeDef)
    {
        if (instruction.OpCode != OpCodes.Callvirt
            || instruction.Operand is not MethodReference methodRef)
            return false;

        var baseTypeRef = methodRef.DeclaringType;
        if (typeRef.IsEqualTo(baseTypeRef))
            return true;

        return typeDef.Interfaces.Any(m => m.InterfaceType.IsEqualTo(baseTypeRef));
    }

    private static bool IsCallAndPop(Instruction instruction)
    {
        return instruction.OpCode.FlowControl == FlowControl.Call && instruction.GetPopCount() > 0;
    }

    private static bool IsStloc(Instruction instruction)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Stloc:
            case Code.Stloc_S:
            case Code.Stloc_0:
            case Code.Stloc_1:
            case Code.Stloc_2:
            case Code.Stloc_3:
                return true;
            default:
                return false;
        }
    }

    private static Exception NoInterfaceMethodInvocationException()
    {
        return new InvalidOperationException("The method Base<T> requires that an interface methods to be base-invoked with its return value fluently");
    }

    private static void EnsureNonAbstract(MethodDefinition method)
    {
        if (method.IsAbstract)
            throw new InvalidOperationException($"The abstract interface method {method.FullName} cannot be invoked");
    }
}
