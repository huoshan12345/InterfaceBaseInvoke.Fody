﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Fody;
using InterfaceBaseInvoke.Fody.Models;
using InterfaceBaseInvoke.Fody.Support;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

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

                case StackBehaviour.Varpop:
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

                case StackBehaviour.Varpush:
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

        public static GraphNode<int> BuildGraph(this IList<Instruction> instructions)
        {
            // Failed to execute weaver {weaver.Config.AssemblyPath} due to a failure to load ValueTuple.
            // This is a known issue with in dotnet(https://github.com/dotnet/runtime/issues/27533).
            // The recommended work around is to avoid using ValueTuple inside a weaver.
            var dic = instructions.Select((m, i) => Tuple.Create(m, i)).ToDictionary(x => x.Item1, x => x.Item2);
            var root = new GraphNode<int>(0);
            var queue = new Queue<GraphNode<int>>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                var index = node.Value;
                var instruction = instructions[index];
                switch (instruction.OpCode.FlowControl)
                {
                    case FlowControl.Cond_Branch:
                    {
                        AddBranchOperand();
                        AddNext();
                        break;
                    }
                    case FlowControl.Branch:
                    {
                        AddBranchOperand();
                        break;
                    }
                    case FlowControl.Return:
                    case FlowControl.Throw:
                        break;

                    case FlowControl.Break:
                    case FlowControl.Meta:
                    case FlowControl.Next:
                    case FlowControl.Phi:
                    case FlowControl.Call when instruction.OpCode != OpCodes.Jmp:
                    default:
                    {
                        AddNext();
                        break;
                    }
                }

                void AddBranchOperand()
                {
                    var ins = (Instruction)instruction!.Operand;
                    var i = dic![ins];
                    var child = GraphNode.Create(i);
                    node!.Children.Add(child);
                    queue!.Enqueue(child);
                }

                void AddNext()
                {
                    var nextIndex = index + 1;
                    if (nextIndex >= instructions.Count)
                        return;

                    var child = GraphNode.Create(nextIndex);
                    node!.Children.Add(child);
                    queue!.Enqueue(child);
                }

            }

            return root;
        }

        public static Instruction[] GetArgumentPushInstructions(this Instruction instruction, IList<Instruction> instructions, GraphNode<int> graph)
        {
            if (instruction.OpCode.FlowControl != FlowControl.Call)
                throw new InstructionWeavingException(instruction, "Expected a call instruction");

            var method = (IMethodSignature)instruction.Operand;
            var argCount = GetArgCount(instruction.OpCode, method);

            if (argCount == 0)
                return Array.Empty<Instruction>();

            var stack = new Stack<Instruction>();
            Dfs(instruction, instructions, graph, ref stack);

            if (stack.Count < argCount)
            {
                throw new InstructionWeavingException(instruction, $"The stack count {stack.Count} is less than the expected argument count {argCount} for {instruction}");
            }

            return stack.Take(argCount).Reverse().ToArray();
        }

        private static bool Dfs(in Instruction instruction, in IList<Instruction> instructions, GraphNode<int> node, ref Stack<Instruction> stack)
        {
            var index = node.Value;
            if (instruction == instructions[index])
            {
                return true;
            }

            var cur = instructions[index];
            var popCount = GetPopCount(cur);
            var pushCount = GetPushCount(cur);

            if (stack.Count < popCount)
            {
                throw new InstructionWeavingException(cur, $"Could not pop {popCount} values from stack whose count is {stack.Count} due to {cur}");
            }

            if (pushCount > 2)
            {
                throw new InstructionWeavingException(cur, $"Unknown instruction {cur} that pops {popCount} values from stack");
            }

            for (var i = 0; i < popCount; i++)
            {
                stack.Pop();
            }

            for (var i = 0; i < pushCount; i++)
            {
                stack.Push(cur);
            }

            if (node.Children.Count == 1)
            {
                return Dfs(instruction, instructions, node.Children[0], ref stack);
            }
            else if (node.Children.Count > 1)
            {
                foreach (var child in node.Children)
                {
                    var perservedStack = stack.Clone();
                    if (!Dfs(instruction, instructions, child, ref perservedStack))
                        continue;
                    stack = perservedStack;
                    return true;
                }
            }

            return false;
        }
    }
}
