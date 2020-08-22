using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace MsgPack.Compiler
{
	internal static class PackILGenerator
	{
		public static void EmitPackCode(Type type, MethodInfo mi, ILGenerator il, Func<Type, MemberInfo[]> targetMemberSelector, Func<MemberInfo, string> memberNameFormatter, Func<Type, MethodInfo> lookupPackMethod)
		{
			if (type.IsPrimitive || type.IsInterface)
			{
				throw new NotSupportedException();
			}
			Variable variable = Variable.CreateArg(0);
			Variable variable2 = Variable.CreateArg(1);
			Variable var_loop = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			if (!type.IsValueType)
			{
				Label label = il.DefineLabel();
				il.EmitLd(variable2);
				il.Emit(OpCodes.Brtrue_S, label);
				il.EmitLd(variable);
				il.Emit(OpCodes.Call, typeof(MsgPackWriter).GetMethod("WriteNil", new Type[0]));
				il.Emit(OpCodes.Ret);
				il.MarkLabel(label);
			}
			if (type.IsArray)
			{
				EmitPackArrayCode(mi, il, type, variable, variable2, var_loop, lookupPackMethod);
			}
			else
			{
				MemberInfo[] array = targetMemberSelector(type);
				il.EmitLd(variable);
				il.EmitLdc(array.Length);
				il.Emit(OpCodes.Callvirt, typeof(MsgPackWriter).GetMethod("WriteMapHeader", new Type[1]
				{
					typeof(int)
				}));
				foreach (MemberInfo memberInfo in array)
				{
					Type memberType = memberInfo.GetMemberType();
					il.EmitLd(variable);
					il.EmitLdstr(memberNameFormatter(memberInfo));
					il.EmitLd_True();
					il.Emit(OpCodes.Call, typeof(MsgPackWriter).GetMethod("Write", new Type[2]
					{
						typeof(string),
						typeof(bool)
					}));
					EmitPackMemberValueCode(memberType, il, variable, variable2, memberInfo, null, type, mi, lookupPackMethod);
				}
			}
			il.Emit(OpCodes.Ret);
		}

		private static void EmitPackArrayCode(MethodInfo mi, ILGenerator il, Type t, Variable var_writer, Variable var_obj, Variable var_loop, Func<Type, MethodInfo> lookupPackMethod)
		{
			Type elementType = t.GetElementType();
			il.EmitLd(var_writer, var_obj);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Call, typeof(MsgPackWriter).GetMethod("WriteArrayHeader", new Type[1]
			{
				typeof(int)
			}));
			Label label = il.DefineLabel();
			Label label2 = il.DefineLabel();
			il.EmitLdc(0);
			il.EmitSt(var_loop);
			il.Emit(OpCodes.Br_S, label2);
			il.MarkLabel(label);
			EmitPackMemberValueCode(elementType, il, var_writer, var_obj, null, var_loop, t, mi, lookupPackMethod);
			il.EmitLd(var_loop);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.EmitSt(var_loop);
			il.MarkLabel(label2);
			il.EmitLd(var_loop, var_obj);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Blt_S, label);
		}

		private static void EmitPackMemberValueCode(Type type, ILGenerator il, Variable var_writer, Variable var_obj, MemberInfo m, Variable elementIdx, Type currentType, MethodInfo currentMethod, Func<Type, MethodInfo> lookupPackMethod)
		{
			il.EmitLd(var_writer, var_obj);
			if (m != null)
			{
				il.EmitLdMember(m);
			}
			if (elementIdx != null)
			{
				il.EmitLd(elementIdx);
				il.Emit(OpCodes.Ldelem, type);
			}
			il.Emit(meth: type.IsPrimitive ? typeof(MsgPackWriter).GetMethod("Write", new Type[1]
			{
				type
			}) : ((!(currentType == type)) ? lookupPackMethod(type) : currentMethod), opcode: OpCodes.Call);
		}

		public static void EmitUnpackCode(Type type, MethodInfo mi, ILGenerator il, Func<Type, MemberInfo[]> targetMemberSelector, Func<MemberInfo, string> memberNameFormatter, Func<Type, MethodInfo> lookupUnpackMethod, Func<Type, IDictionary<string, int>> lookupMemberMapping, MethodInfo lookupMemberMappingMethod)
		{
			if (type.IsArray)
			{
				EmitUnpackArrayCode(type, mi, il, targetMemberSelector, memberNameFormatter, lookupUnpackMethod);
			}
			else
			{
				EmitUnpackMapCode(type, mi, il, targetMemberSelector, memberNameFormatter, lookupUnpackMethod, lookupMemberMapping, lookupMemberMappingMethod);
			}
		}

		private static void EmitUnpackMapCode(Type type, MethodInfo mi, ILGenerator il, Func<Type, MemberInfo[]> targetMemberSelector, Func<MemberInfo, string> memberNameFormatter, Func<Type, MethodInfo> lookupUnpackMethod, Func<Type, IDictionary<string, int>> lookupMemberMapping, MethodInfo lookupMemberMappingMethod)
		{
			MethodInfo method = typeof(PackILGenerator).GetMethod("UnpackFailed", BindingFlags.Static | BindingFlags.NonPublic);
			MemberInfo[] array = targetMemberSelector(type);
			IDictionary<string, int> dictionary = lookupMemberMapping(type);
			for (int i = 0; i < array.Length; i++)
			{
				dictionary.Add(memberNameFormatter(array[i]), i);
			}
			Variable variable = Variable.CreateArg(0);
			Variable v = Variable.CreateLocal(il.DeclareLocal(type));
			Variable v2 = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			Variable v3 = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			Variable v4 = Variable.CreateLocal(il.DeclareLocal(typeof(IDictionary<string, int>)));
			Variable variable2 = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			Variable v5 = Variable.CreateLocal(il.DeclareLocal(typeof(Type)));
			EmitUnpackReadAndTypeCheckCode(il, variable, typeof(MsgPackReader).GetMethod("IsMap"), method, nullCheckAndReturn: true);
			il.Emit(OpCodes.Ldtoken, type);
			il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			il.EmitSt(v5);
			il.EmitLd(v5);
			il.Emit(OpCodes.Call, lookupMemberMappingMethod);
			il.EmitSt(v4);
			il.EmitLd(v5);
			il.Emit(OpCodes.Call, typeof(FormatterServices).GetMethod("GetUninitializedObject"));
			il.Emit(OpCodes.Castclass, type);
			il.EmitSt(v);
			il.EmitLd(variable);
			il.Emit(OpCodes.Call, typeof(MsgPackReader).GetProperty("Length").GetGetMethod());
			il.EmitSt(v2);
			Label label = il.DefineLabel();
			Label label2 = il.DefineLabel();
			il.EmitLdc(0);
			il.EmitSt(v3);
			il.Emit(OpCodes.Br, label2);
			il.MarkLabel(label);
			EmitUnpackReadAndTypeCheckCode(il, variable, typeof(MsgPackReader).GetMethod("IsRaw"), method, nullCheckAndReturn: false);
			Label label3 = il.DefineLabel();
			il.EmitLd(v4);
			il.EmitLd(variable);
			il.Emit(OpCodes.Call, typeof(MsgPackReader).GetMethod("ReadRawString", new Type[0]));
			il.Emit(OpCodes.Ldloca_S, (byte)variable2.Index);
			il.Emit(OpCodes.Callvirt, typeof(IDictionary<string, int>).GetMethod("TryGetValue"));
			il.Emit(OpCodes.Brtrue, label3);
			il.Emit(OpCodes.Call, method);
			il.MarkLabel(label3);
			Label[] array2 = new Label[array.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = il.DefineLabel();
			}
			Label label4 = il.DefineLabel();
			il.EmitLd(variable2);
			il.Emit(OpCodes.Switch, array2);
			il.Emit(OpCodes.Call, method);
			for (int k = 0; k < array2.Length; k++)
			{
				il.MarkLabel(array2[k]);
				MemberInfo memberInfo = array[k];
				Type memberType = memberInfo.GetMemberType();
				MethodInfo meth = lookupUnpackMethod(memberType);
				il.EmitLd(v);
				il.EmitLd(variable);
				il.Emit(OpCodes.Call, meth);
				il.EmitStMember(memberInfo);
				il.Emit(OpCodes.Br, label4);
			}
			il.MarkLabel(label4);
			il.EmitLd(v3);
			il.EmitLdc(1);
			il.Emit(OpCodes.Add);
			il.EmitSt(v3);
			il.MarkLabel(label2);
			il.EmitLd(v3);
			il.EmitLd(v2);
			il.Emit(OpCodes.Blt, label);
			il.EmitLd(v);
			il.Emit(OpCodes.Ret);
		}

		private static void EmitUnpackArrayCode(Type arrayType, MethodInfo mi, ILGenerator il, Func<Type, MemberInfo[]> targetMemberSelector, Func<MemberInfo, string> memberNameFormatter, Func<Type, MethodInfo> lookupUnpackMethod)
		{
			Type elementType = arrayType.GetElementType();
			MethodInfo method = typeof(PackILGenerator).GetMethod("UnpackFailed", BindingFlags.Static | BindingFlags.NonPublic);
			Variable variable = Variable.CreateArg(0);
			Variable variable2 = Variable.CreateLocal(il.DeclareLocal(arrayType));
			Variable v = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			Variable variable3 = Variable.CreateLocal(il.DeclareLocal(typeof(int)));
			Variable v2 = Variable.CreateLocal(il.DeclareLocal(typeof(Type)));
			EmitUnpackReadAndTypeCheckCode(il, variable, typeof(MsgPackReader).GetMethod("IsArray"), method, nullCheckAndReturn: true);
			il.Emit(OpCodes.Ldtoken, elementType);
			il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			il.EmitSt(v2);
			il.EmitLd(variable);
			il.Emit(OpCodes.Call, typeof(MsgPackReader).GetProperty("Length").GetGetMethod());
			il.EmitSt(v);
			il.EmitLd(v2);
			il.EmitLd(v);
			il.Emit(OpCodes.Call, typeof(Array).GetMethod("CreateInstance", new Type[2]
			{
				typeof(Type),
				typeof(int)
			}));
			il.Emit(OpCodes.Castclass, arrayType);
			il.EmitSt(variable2);
			MethodInfo meth = lookupUnpackMethod(elementType);
			Label label = il.DefineLabel();
			Label label2 = il.DefineLabel();
			il.EmitLdc(0);
			il.EmitSt(variable3);
			il.Emit(OpCodes.Br, label2);
			il.MarkLabel(label);
			il.EmitLd(variable2, variable3);
			il.EmitLd(variable);
			il.Emit(OpCodes.Call, meth);
			il.Emit(OpCodes.Stelem, elementType);
			il.EmitLd(variable3);
			il.EmitLdc(1);
			il.Emit(OpCodes.Add);
			il.EmitSt(variable3);
			il.MarkLabel(label2);
			il.EmitLd(variable3);
			il.EmitLd(v);
			il.Emit(OpCodes.Blt, label);
			il.EmitLd(variable2);
			il.Emit(OpCodes.Ret);
		}

		private static void EmitUnpackReadAndTypeCheckCode(ILGenerator il, Variable msgpackReader, MethodInfo typeCheckMethod, MethodInfo failedMethod, bool nullCheckAndReturn)
		{
			Label label = il.DefineLabel();
			Label label2 = nullCheckAndReturn ? il.DefineLabel() : default(Label);
			Label label3 = il.DefineLabel();
			il.EmitLd(msgpackReader);
			il.Emit(OpCodes.Call, typeof(MsgPackReader).GetMethod("Read"));
			il.Emit(OpCodes.Brfalse_S, label);
			if (nullCheckAndReturn)
			{
				il.EmitLd(msgpackReader);
				il.Emit(OpCodes.Call, typeof(MsgPackReader).GetProperty("Type").GetGetMethod());
				il.EmitLdc(192);
				il.Emit(OpCodes.Beq_S, label2);
			}
			il.EmitLd(msgpackReader);
			il.Emit(OpCodes.Call, typeCheckMethod);
			il.Emit(OpCodes.Brtrue_S, label3);
			il.Emit(OpCodes.Br, label);
			if (nullCheckAndReturn)
			{
				il.MarkLabel(label2);
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}
			il.MarkLabel(label);
			il.Emit(OpCodes.Call, failedMethod);
			il.MarkLabel(label3);
		}

		internal static void UnpackFailed()
		{
			throw new FormatException();
		}

		private static Type GetMemberType(this MemberInfo mi)
		{
			if (mi.MemberType == MemberTypes.Field)
			{
				return ((FieldInfo)mi).FieldType;
			}
			if (mi.MemberType == MemberTypes.Property)
			{
				return ((PropertyInfo)mi).PropertyType;
			}
			throw new ArgumentException();
		}
	}
}
