using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MsgPack.Compiler
{
	public static class EmitExtensions
	{
		public static void EmitLd(this ILGenerator il, Variable v)
		{
			switch (v.VarType)
			{
			case VariableType.Arg:
				il.EmitLdarg(v);
				break;
			case VariableType.Local:
				il.EmitLdloc(v);
				break;
			default:
				throw new ArgumentException();
			}
		}

		public static void EmitLd(this ILGenerator il, params Variable[] list)
		{
			for (int i = 0; i < list.Length; i++)
			{
				il.EmitLd(list[i]);
			}
		}

		public static void EmitLdarg(this ILGenerator il, Variable v)
		{
			if (v.VarType != VariableType.Arg)
			{
				throw new ArgumentException();
			}
			switch (v.Index)
			{
			case 0:
				il.Emit(OpCodes.Ldarg_0);
				return;
			case 1:
				il.Emit(OpCodes.Ldarg_1);
				return;
			case 2:
				il.Emit(OpCodes.Ldarg_2);
				return;
			case 3:
				il.Emit(OpCodes.Ldarg_3);
				return;
			}
			if (v.Index <= 255)
			{
				il.Emit(OpCodes.Ldarg_S, (byte)v.Index);
				return;
			}
			if (v.Index <= 32767)
			{
				il.Emit(OpCodes.Ldarg, v.Index);
				return;
			}
			throw new FormatException();
		}

		public static void EmitLdloc(this ILGenerator il, Variable v)
		{
			if (v.VarType != 0)
			{
				throw new ArgumentException();
			}
			switch (v.Index)
			{
			case 0:
				il.Emit(OpCodes.Ldloc_0);
				return;
			case 1:
				il.Emit(OpCodes.Ldloc_1);
				return;
			case 2:
				il.Emit(OpCodes.Ldloc_2);
				return;
			case 3:
				il.Emit(OpCodes.Ldloc_3);
				return;
			}
			if (v.Index <= 255)
			{
				il.Emit(OpCodes.Ldloc_S, (byte)v.Index);
				return;
			}
			if (v.Index <= 32767)
			{
				il.Emit(OpCodes.Ldloc, v.Index);
				return;
			}
			throw new FormatException();
		}

		public static void EmitSt(this ILGenerator il, Variable v)
		{
			switch (v.VarType)
			{
			case VariableType.Arg:
				il.EmitStarg(v);
				break;
			case VariableType.Local:
				il.EmitStloc(v);
				break;
			default:
				throw new ArgumentException();
			}
		}

		public static void EmitStarg(this ILGenerator il, Variable v)
		{
			if (v.VarType != VariableType.Arg)
			{
				throw new ArgumentException();
			}
			if (v.Index <= 255)
			{
				il.Emit(OpCodes.Starg_S, (byte)v.Index);
				return;
			}
			if (v.Index <= 32767)
			{
				il.Emit(OpCodes.Starg, v.Index);
				return;
			}
			throw new FormatException();
		}

		public static void EmitStloc(this ILGenerator il, Variable v)
		{
			if (v.VarType != 0)
			{
				throw new ArgumentException();
			}
			switch (v.Index)
			{
			case 0:
				il.Emit(OpCodes.Stloc_0);
				return;
			case 1:
				il.Emit(OpCodes.Stloc_1);
				return;
			case 2:
				il.Emit(OpCodes.Stloc_2);
				return;
			case 3:
				il.Emit(OpCodes.Stloc_3);
				return;
			}
			if (v.Index <= 255)
			{
				il.Emit(OpCodes.Stloc_S, (byte)v.Index);
				return;
			}
			if (v.Index <= 32767)
			{
				il.Emit(OpCodes.Stloc, v.Index);
				return;
			}
			throw new FormatException();
		}

		public static void EmitLdc(this ILGenerator il, int v)
		{
			switch (v)
			{
			case 0:
				il.Emit(OpCodes.Ldc_I4_0);
				return;
			case 1:
				il.Emit(OpCodes.Ldc_I4_1);
				return;
			case 2:
				il.Emit(OpCodes.Ldc_I4_2);
				return;
			case 3:
				il.Emit(OpCodes.Ldc_I4_3);
				return;
			case 4:
				il.Emit(OpCodes.Ldc_I4_4);
				return;
			case 5:
				il.Emit(OpCodes.Ldc_I4_5);
				return;
			case 6:
				il.Emit(OpCodes.Ldc_I4_6);
				return;
			case 7:
				il.Emit(OpCodes.Ldc_I4_7);
				return;
			case 8:
				il.Emit(OpCodes.Ldc_I4_8);
				return;
			case -1:
				il.Emit(OpCodes.Ldc_I4_M1);
				return;
			}
			if (v <= 127 && v >= -128)
			{
				il.Emit(OpCodes.Ldc_I4_S, (sbyte)v);
			}
			else
			{
				il.Emit(OpCodes.Ldc_I4, v);
			}
		}

		public static void EmitLd_False(this ILGenerator il)
		{
			il.Emit(OpCodes.Ldc_I4_1);
		}

		public static void EmitLd_True(this ILGenerator il)
		{
			il.Emit(OpCodes.Ldc_I4_1);
		}

		public static void EmitLdstr(this ILGenerator il, string v)
		{
			il.Emit(OpCodes.Ldstr, v);
		}

		public static void EmitLdMember(this ILGenerator il, MemberInfo m)
		{
			if (m.MemberType == MemberTypes.Field)
			{
				il.Emit(OpCodes.Ldfld, (FieldInfo)m);
				return;
			}
			if (m.MemberType == MemberTypes.Property)
			{
				il.Emit(OpCodes.Callvirt, ((PropertyInfo)m).GetGetMethod(nonPublic: true));
				return;
			}
			throw new ArgumentException();
		}

		public static void EmitStMember(this ILGenerator il, MemberInfo m)
		{
			if (m.MemberType == MemberTypes.Field)
			{
				il.Emit(OpCodes.Stfld, (FieldInfo)m);
				return;
			}
			if (m.MemberType == MemberTypes.Property)
			{
				il.Emit(OpCodes.Callvirt, ((PropertyInfo)m).GetSetMethod(nonPublic: true));
				return;
			}
			throw new ArgumentException();
		}
	}
}
