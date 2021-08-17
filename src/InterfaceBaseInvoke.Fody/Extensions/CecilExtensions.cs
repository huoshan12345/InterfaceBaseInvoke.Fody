using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Fody;
using InterfaceBaseInvoke.Fody.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InterfaceBaseInvoke.Fody.Extensions
{
    internal static partial class CecilExtensions
    {
        public static TypeDefinition ResolveRequiredType(this TypeReference typeRef)
        {
            TypeDefinition typeDef;

            try
            {
                typeDef = typeRef.Resolve();
            }
            catch (Exception ex)
            {
                throw new WeavingException($"Could not resolve type {typeRef.FullName}: {ex.Message}");
            }

            return typeDef ?? throw new WeavingException($"Could not resolve type {typeRef.FullName}");
        }

        public static bool IsForwardedType(this ExportedType exportedType)
        {
            for (; exportedType != null; exportedType = exportedType.DeclaringType)
            {
                if (exportedType.IsForwarder)
                    return true;
            }

            return false;
        }

        private static TypeDefinition ResolveRequiredType(this ExportedType exportedType)
        {
            TypeDefinition typeDef;

            try
            {
                typeDef = exportedType.Resolve();
            }
            catch (Exception ex)
            {
                throw new WeavingException($"Could not resolve type {exportedType.FullName}: {ex.Message}");
            }

            return typeDef ?? throw new WeavingException($"Could not resolve type {exportedType.FullName}");
        }

        public static TypeReference Clone(this TypeReference typeRef)
        {
            var clone = new TypeReference(typeRef.Namespace, typeRef.Name, typeRef.Module, typeRef.Scope, typeRef.IsValueType)
            {
                DeclaringType = typeRef.DeclaringType
            };

            if (typeRef.HasGenericParameters)
            {
                foreach (var param in typeRef.GenericParameters)
                    clone.GenericParameters.Add(new GenericParameter(param.Name, clone));
            }

            return clone;
        }

        public static TypeReference CreateReference(this ExportedType exportedType, ModuleDefinition exportingModule, ModuleDefinition targetModule)
        {
            var typeDef = exportedType.ResolveRequiredType();
            var metadataScope = MapAssemblyReference(targetModule, exportingModule.Assembly.Name);

            var typeRef = new TypeReference(exportedType.Namespace, exportedType.Name, exportingModule, metadataScope, typeDef.IsValueType)
            {
                DeclaringType = exportedType.DeclaringType?.CreateReference(exportingModule, targetModule)
            };

            if (typeDef.HasGenericParameters)
            {
                foreach (var param in typeDef.GenericParameters)
                    typeRef.GenericParameters.Add(new GenericParameter(param.Name, typeRef));
            }

            return typeRef;
        }

        private static AssemblyNameReference MapAssemblyReference(ModuleDefinition module, AssemblyNameReference name)
        {
            // Try to map to an existing assembly reference by name,
            // to avoid adding additional versions of a referenced assembly
            // (netstandard v2.0 can be mapped to netstandard 2.1 for instance)

            foreach (var assemblyReference in module.AssemblyReferences)
            {
                if (assemblyReference.Name == name.Name)
                    return assemblyReference;
            }

            return name;
        }

        private static TypeReference MapToScope(this TypeReference typeRef, IMetadataScope scope, ModuleDefinition module)
        {
            if (scope.MetadataScopeType == MetadataScopeType.AssemblyNameReference)
            {
                var assemblyName = (AssemblyNameReference)scope;
                var assembly = module.AssemblyResolver.Resolve(assemblyName) ?? throw new WeavingException($"Could not resolve assembly {assemblyName.Name}");

                if (assembly.MainModule.HasExportedTypes)
                {
                    foreach (var exportedType in assembly.MainModule.ExportedTypes)
                    {
                        if (!exportedType.IsForwardedType())
                            continue;

                        if (exportedType.FullName == typeRef.FullName)
                            return exportedType.CreateReference(assembly.MainModule, module);
                    }
                }
            }

            return typeRef;
        }

        public static MethodReference Clone(this MethodReference method)
        {
            var clone = new MethodReference(method.Name, method.ReturnType, method.DeclaringType)
            {
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention
            };

            if (method.HasParameters)
            {
                foreach (var param in method.Parameters)
                    clone.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
            }

            if (method.HasGenericParameters)
            {
                foreach (var param in method.GenericParameters)
                    clone.GenericParameters.Add(new GenericParameter(param.Name, clone));
            }

            return clone;
        }

        public static MethodReference MapToScope(this MethodReference method, IMetadataScope scope, ModuleDefinition module)
        {
            var clone = new MethodReference(method.Name, method.ReturnType.MapToScope(scope, module), method.DeclaringType.MapToScope(scope, module))
            {
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention
            };

            if (method.HasParameters)
            {
                foreach (var param in method.Parameters)
                    clone.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType.MapToScope(scope, module)));
            }

            if (method.HasGenericParameters)
            {
                foreach (var param in method.GenericParameters)
                    clone.GenericParameters.Add(new GenericParameter(param.Name, clone));
            }

            return clone;
        }

        public static MethodReference MakeGeneric(this MethodReference method, TypeReference declaringType)
        {
            if (!declaringType.IsGenericInstance || method.DeclaringType.IsGenericInstance)
                return method;

            var genericDeclType = new GenericInstanceType(method.DeclaringType);

            foreach (var argument in ((GenericInstanceType)declaringType).GenericArguments)
                genericDeclType.GenericArguments.Add(argument);

            var result = method.Clone();
            result.DeclaringType = genericDeclType;
            return result;
        }

        public static Instruction[] GetArgumentPushInstructions(this Instruction instruction)
        {
            if (instruction.OpCode.FlowControl != FlowControl.Call)
                throw new InstructionWeavingException(instruction, "Expected a call instruction");

            var method = (IMethodSignature)instruction.Operand;
            var argCount = GetArgCount(instruction.OpCode, method);

            if (argCount == 0)
                return Array.Empty<Instruction>();

            var result = new Instruction[argCount];
            var currentInstruction = instruction.Previous;

            for (var paramIndex = result.Length - 1; paramIndex >= 0; --paramIndex)
                result[paramIndex] = BackwardScanPush(ref currentInstruction);

            return result;
        }

        private static Instruction BackwardScanPush(ref Instruction currentInstruction)
        {
            var startInstruction = currentInstruction;
            var stackToConsume = 1;

            while (stackToConsume > 0)
            {
                switch (currentInstruction.OpCode.FlowControl)
                {
                    case FlowControl.Branch:
                    case FlowControl.Cond_Branch:
                    case FlowControl.Return:
                    case FlowControl.Throw:
                        throw new InstructionWeavingException(startInstruction, $"Could not locate call argument due to {currentInstruction}");

                    case FlowControl.Call:
                        if (currentInstruction.OpCode == OpCodes.Jmp)
                            throw new InstructionWeavingException(startInstruction, $"Could not locate call argument due to {currentInstruction}");
                        break;
                }

                var popCount = GetPopCount(currentInstruction);
                var pushCount = GetPushCount(currentInstruction);

                stackToConsume -= pushCount;

                if (stackToConsume == 0)
                {
                    var result = currentInstruction;
                    currentInstruction = currentInstruction.Previous;
                    return result;
                }

                if (stackToConsume < 0)
                    throw new InstructionWeavingException(startInstruction, $"Could not locate call argument due to {currentInstruction} which pops an unexpected number of items from the stack");

                stackToConsume += popCount;
                currentInstruction = currentInstruction.Previous;
            }

            throw new InstructionWeavingException(startInstruction, "Could not locate call argument, reached beginning of method");
        }

        private static int GetArgCount(OpCode opCode, IMethodSignature method)
        {
            var argCount = method.Parameters.Count;

            if (method.HasThis && !method.ExplicitThis && opCode.Code != Code.Newobj)
                ++argCount;

            if (opCode.Code == Code.Calli)
                ++argCount;

            return argCount;
        }

        public static int GetPopCount(this Instruction instruction)
        {
            if (instruction.OpCode.FlowControl == FlowControl.Call)
                return GetArgCount(instruction.OpCode, (IMethodSignature)instruction.Operand);

            if (instruction.OpCode == OpCodes.Dup)
                return 0;

            switch (instruction.OpCode.StackBehaviourPop)
            {
                case StackBehaviour.Pop0:
                    return 0;

                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Pop1:
                    return 1;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return 2;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    return 3;

                case StackBehaviour.PopAll:
                    throw new InstructionWeavingException(instruction, "Unexpected stack-clearing instruction encountered");

                default:
                    throw new InstructionWeavingException(instruction, $"Unexpected stack pop behavior: {instruction.OpCode.StackBehaviourPop}");
            }
        }

        public static int GetPushCount(this Instruction instruction)
        {
            if (instruction.OpCode.FlowControl == FlowControl.Call)
            {
                var method = (IMethodSignature)instruction.Operand;
                return method.ReturnType.MetadataType != MetadataType.Void || instruction.OpCode.Code == Code.Newobj ? 1 : 0;
            }

            if (instruction.OpCode == OpCodes.Dup)
                return 1;

            switch (instruction.OpCode.StackBehaviourPush)
            {
                case StackBehaviour.Push0:
                    return 0;

                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    return 1;

                case StackBehaviour.Push1_push1:
                    return 2;

                default:
                    throw new InstructionWeavingException(instruction, $"Unexpected stack push behavior: {instruction.OpCode.StackBehaviourPush}");
            }
        }

        public static MethodCallingConvention ToMethodCallingConvention(this CallingConvention callingConvention)
        {
            return callingConvention switch
            {
                CallingConvention.Cdecl => MethodCallingConvention.C,
                CallingConvention.StdCall => MethodCallingConvention.StdCall,
                CallingConvention.Winapi => MethodCallingConvention.StdCall,
                CallingConvention.FastCall => MethodCallingConvention.FastCall,
                CallingConvention.ThisCall => MethodCallingConvention.ThisCall,
                _ => throw new WeavingException("Invalid calling convention")
            };
        }

        public static IEnumerable<Instruction> GetInstructions(this ExceptionHandler handler)
        {
            if (handler.TryStart != null)
                yield return handler.TryStart;

            if (handler.TryEnd != null)
                yield return handler.TryEnd;

            if (handler.FilterStart != null)
                yield return handler.FilterStart;

            if (handler.HandlerStart != null)
                yield return handler.HandlerStart;

            if (handler.HandlerEnd != null)
                yield return handler.HandlerEnd;
        }

        public static SequencePoint? GetInputSequencePoint(this Instruction? instruction, MethodDefinition method)
        {
            if (instruction == null)
                return null;

            var sequencePoints = method.DebugInformation.HasSequencePoints
                ? method.DebugInformation.SequencePoints
                : Enumerable.Empty<SequencePoint>();

            return sequencePoints.LastOrDefault(sp => sp.Offset <= instruction.Offset);
        }

        private static readonly Func<TypeReference, TypeReference, bool> _typeRefEqualFunc = CreateTypeRefEqualFunc();

        private static Func<TypeReference, TypeReference, bool> CreateTypeRefEqualFunc()
        {
            var assembly = typeof(TypeReference).Assembly;
            var method = TypeHelper.GetMethod(assembly, "Mono.Cecil.TypeReferenceEqualityComparer", "AreEqual", true, true);
            var paras = new[]
            {
                Expression.Parameter(typeof(TypeReference)),
                Expression.Parameter(typeof(TypeReference)),
            };
            var args = paras.Append(Expression.Constant(0).Convert(assembly.GetType("Mono.Cecil.TypeComparisonMode")));
            var call = Expression.Call(method, args);
            return call.Lambda<Func<TypeReference, TypeReference, bool>>(paras).Compile();
        }

        public static bool IsEqualTo(this TypeReference a, TypeReference b)
        {
            return _typeRefEqualFunc(a, b);
        }

        public static bool IsAssignableTo(this TypeDefinition derivedTypeDef, TypeReference baseTypeRef)
        {
            if (derivedTypeDef.IsEqualTo(baseTypeRef))
                return true;

            return derivedTypeDef.Interfaces.Any(m => m.InterfaceType.IsEqualTo(baseTypeRef));
        }

        public static MethodDefinition GetInterfaceDefaultMethod(this TypeDefinition typeDef, MethodReference methodRef)
        {
            var methods = typeDef.Methods.Where(m => m.Overrides.Any(x => x.FullName == methodRef.FullName)).ToList();
            return methods.Count switch
            {
                0 => throw new MissingMethodException(methodRef.Name),
                > 1 => throw new AmbiguousMatchException($"Found more than one method in type {typeDef.Name} by name " + methodRef.Name),
                _ => methods[0]
            };
        }
    }
}
