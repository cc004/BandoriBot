using MsgPack.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace MsgPack
{
	public class CompiledPacker
	{
		public abstract class PackerBase
		{
			private Dictionary<Type, Delegate> _packers = new Dictionary<Type, Delegate>();

			private Dictionary<Type, Delegate> _unpackers = new Dictionary<Type, Delegate>();

			protected Dictionary<Type, MethodInfo> _packMethods = new Dictionary<Type, MethodInfo>();

			protected Dictionary<Type, MethodInfo> _unpackMethods = new Dictionary<Type, MethodInfo>();

			protected PackerBase()
			{
				DefaultPackMethods.Register(_packMethods, _unpackMethods);
			}

			public Action<MsgPackWriter, T> CreatePacker<T>()
			{
				Delegate value;
				lock (_packers)
				{
					if (!_packers.TryGetValue(typeof(T), out value))
					{
						value = CreatePacker_Internal<T>();
						_packers.Add(typeof(T), value);
					}
				}
				return (Action<MsgPackWriter, T>)value;
			}

			public Func<MsgPackReader, T> CreateUnpacker<T>()
			{
				Delegate value;
				lock (_unpackers)
				{
					if (!_unpackers.TryGetValue(typeof(T), out value))
					{
						value = CreateUnpacker_Internal<T>();
						_unpackers.Add(typeof(T), value);
					}
				}
				return (Func<MsgPackReader, T>)value;
			}

			protected abstract Action<MsgPackWriter, T> CreatePacker_Internal<T>();

			protected abstract Func<MsgPackReader, T> CreateUnpacker_Internal<T>();
		}

		public sealed class DynamicMethodPacker : PackerBase
		{
			protected static MethodInfo LookupMemberMappingMethod;

			private static Dictionary<Type, IDictionary<string, int>> UnpackMemberMappings;

			private static int _dynamicMethodIdx;

			static DynamicMethodPacker()
			{
				_dynamicMethodIdx = 0;
				UnpackMemberMappings = new Dictionary<Type, IDictionary<string, int>>();
				LookupMemberMappingMethod = typeof(DynamicMethodPacker).GetMethod("LookupMemberMapping", BindingFlags.Static | BindingFlags.NonPublic);
			}

			protected override Action<MsgPackWriter, T> CreatePacker_Internal<T>()
			{
				DynamicMethod dynamicMethod = CreatePacker(typeof(T), CreatePackDynamicMethod(typeof(T)));
				return (Action<MsgPackWriter, T>)dynamicMethod.CreateDelegate(typeof(Action<MsgPackWriter, T>));
			}

			protected override Func<MsgPackReader, T> CreateUnpacker_Internal<T>()
			{
				DynamicMethod dynamicMethod = CreateUnpacker(typeof(T), CreateUnpackDynamicMethod(typeof(T)));
				return (Func<MsgPackReader, T>)dynamicMethod.CreateDelegate(typeof(Func<MsgPackReader, T>));
			}

			private DynamicMethod CreatePacker(Type t, DynamicMethod dm)
			{
				ILGenerator iLGenerator = dm.GetILGenerator();
				_packMethods.Add(t, dm);
				PackILGenerator.EmitPackCode(t, dm, iLGenerator, LookupMembers, FormatMemberName, LookupPackMethod);
				return dm;
			}

			private DynamicMethod CreateUnpacker(Type t, DynamicMethod dm)
			{
				ILGenerator iLGenerator = dm.GetILGenerator();
				_unpackMethods.Add(t, dm);
				PackILGenerator.EmitUnpackCode(t, dm, iLGenerator, LookupMembers, FormatMemberName, LookupUnpackMethod, LookupMemberMapping, LookupMemberMappingMethod);
				return dm;
			}

			private static DynamicMethod CreatePackDynamicMethod(Type t)
			{
				return CreateDynamicMethod(typeof(void), new Type[2]
				{
					typeof(MsgPackWriter),
					t
				});
			}

			private static DynamicMethod CreateUnpackDynamicMethod(Type t)
			{
				return CreateDynamicMethod(t, new Type[1]
				{
					typeof(MsgPackReader)
				});
			}

			private static MemberInfo[] LookupMembers(Type t)
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				List<MemberInfo> list = new List<MemberInfo>();
				list.AddRange(t.GetFields(bindingAttr));
				return list.ToArray();
			}

			private MethodInfo LookupPackMethod(Type t)
			{
				if (_packMethods.TryGetValue(t, out MethodInfo value))
				{
					return value;
				}
				DynamicMethod dm = CreatePackDynamicMethod(t);
				return CreatePacker(t, dm);
			}

			private MethodInfo LookupUnpackMethod(Type t)
			{
				if (_unpackMethods.TryGetValue(t, out MethodInfo value))
				{
					return value;
				}
				DynamicMethod dm = CreateUnpackDynamicMethod(t);
				return CreateUnpacker(t, dm);
			}

			private static string FormatMemberName(MemberInfo m)
			{
				if (m.MemberType != MemberTypes.Field)
				{
					return m.Name;
				}
				string text = m.Name;
				int num;
				if (text[0] == '<' && (num = text.IndexOf('>')) > 1)
				{
					text = text.Substring(1, num - 1);
				}
				return text;
			}

			private static DynamicMethod CreateDynamicMethod(Type returnType, Type[] parameterTypes)
			{
				string name = "_" + Interlocked.Increment(ref _dynamicMethodIdx).ToString();
				return new DynamicMethod(name, returnType, parameterTypes, restrictedSkipVisibility: true);
			}

			internal static IDictionary<string, int> LookupMemberMapping(Type t)
			{
				lock (UnpackMemberMappings)
				{
					if (UnpackMemberMappings.TryGetValue(t, out IDictionary<string, int> value))
					{
						return value;
					}
					value = new Dictionary<string, int>();
					UnpackMemberMappings.Add(t, value);
					return value;
				}
			}
		}

		public sealed class MethodBuilderPacker : PackerBase
		{
			public const string AssemblyName = "MessagePackInternalAssembly";

			private static AssemblyName DynamicAsmName;

			private static AssemblyBuilder DynamicAsmBuilder;

			private static ModuleBuilder DynamicModuleBuilder;

			protected static MethodInfo LookupMemberMappingMethod;

			private static Dictionary<Type, IDictionary<string, int>> UnpackMemberMappings;

			static MethodBuilderPacker()
			{
				UnpackMemberMappings = new Dictionary<Type, IDictionary<string, int>>();
				LookupMemberMappingMethod = typeof(MethodBuilderPacker).GetMethod("LookupMemberMapping", BindingFlags.Static | BindingFlags.NonPublic);
				DynamicAsmName = new AssemblyName("MessagePackInternalAssembly");
				DynamicAsmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(DynamicAsmName, AssemblyBuilderAccess.Run);
				DynamicModuleBuilder = DynamicAsmBuilder.DefineDynamicModule(DynamicAsmName.Name);
			}

			protected override Action<MsgPackWriter, T> CreatePacker_Internal<T>()
			{
				CreatePackMethodBuilder(typeof(T), out TypeBuilder tb, out MethodBuilder mb);
				_packMethods.Add(typeof(T), mb);
				CreatePacker(typeof(T), mb);
				MethodInfo method = ToCallableMethodInfo(typeof(T), tb, isPacker: true);
				return (Action<MsgPackWriter, T>)Delegate.CreateDelegate(typeof(Action<MsgPackWriter, T>), method);
			}

			protected override Func<MsgPackReader, T> CreateUnpacker_Internal<T>()
			{
				CreateUnpackMethodBuilder(typeof(T), out TypeBuilder tb, out MethodBuilder mb);
				_unpackMethods.Add(typeof(T), mb);
				CreateUnpacker(typeof(T), mb);
				MethodInfo method = ToCallableMethodInfo(typeof(T), tb, isPacker: false);
				return (Func<MsgPackReader, T>)Delegate.CreateDelegate(typeof(Func<MsgPackReader, T>), method);
			}

			private void CreatePacker(Type t, MethodBuilder mb)
			{
				ILGenerator iLGenerator = mb.GetILGenerator();
				PackILGenerator.EmitPackCode(t, mb, iLGenerator, LookupMembers, FormatMemberName, LookupPackMethod);
			}

			private void CreateUnpacker(Type t, MethodBuilder mb)
			{
				ILGenerator iLGenerator = mb.GetILGenerator();
				PackILGenerator.EmitUnpackCode(t, mb, iLGenerator, LookupMembers, FormatMemberName, LookupUnpackMethod, LookupMemberMapping, LookupMemberMappingMethod);
			}

			private MethodInfo ToCallableMethodInfo(Type t, TypeBuilder tb, bool isPacker)
			{
				Type type = tb.CreateType();
				MethodInfo method = type.GetMethod(isPacker ? "Pack" : "Unpack", BindingFlags.Static | BindingFlags.Public);
				if (isPacker)
				{
					_packMethods[t] = method;
				}
				else
				{
					_unpackMethods[t] = method;
				}
				return method;
			}

			private MethodInfo LookupPackMethod(Type t)
			{
				if (_packMethods.TryGetValue(t, out MethodInfo value))
				{
					return value;
				}
				CreatePackMethodBuilder(t, out TypeBuilder tb, out MethodBuilder mb);
				_packMethods.Add(t, mb);
				CreatePacker(t, mb);
				return ToCallableMethodInfo(t, tb, isPacker: true);
			}

			private MethodInfo LookupUnpackMethod(Type t)
			{
				if (_unpackMethods.TryGetValue(t, out MethodInfo value))
				{
					return value;
				}
				CreateUnpackMethodBuilder(t, out TypeBuilder tb, out MethodBuilder mb);
				_unpackMethods.Add(t, mb);
				CreateUnpacker(t, mb);
				return ToCallableMethodInfo(t, tb, isPacker: false);
			}

			private static string FormatMemberName(MemberInfo m)
			{
				return m.Name;
			}

			private static MemberInfo[] LookupMembers(Type t)
			{
				BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
				List<MemberInfo> list = new List<MemberInfo>();
				list.AddRange(t.GetFields(bindingAttr));
				return list.ToArray();
			}

			private static void CreatePackMethodBuilder(Type t, out TypeBuilder tb, out MethodBuilder mb)
			{
				tb = DynamicModuleBuilder.DefineType(t.Name + "PackerType", TypeAttributes.Public);
				mb = tb.DefineMethod("Pack", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, typeof(void), new Type[2]
				{
					typeof(MsgPackWriter),
					t
				});
			}

			private static void CreateUnpackMethodBuilder(Type t, out TypeBuilder tb, out MethodBuilder mb)
			{
				tb = DynamicModuleBuilder.DefineType(t.Name + "UnpackerType", TypeAttributes.Public);
				mb = tb.DefineMethod("Unpack", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, t, new Type[1]
				{
					typeof(MsgPackReader)
				});
			}

			internal static IDictionary<string, int> LookupMemberMapping(Type t)
			{
				lock (UnpackMemberMappings)
				{
					if (UnpackMemberMappings.TryGetValue(t, out IDictionary<string, int> value))
					{
						return value;
					}
					value = new Dictionary<string, int>();
					UnpackMemberMappings.Add(t, value);
					return value;
				}
			}
		}

		internal static class DefaultPackMethods
		{
			public static void Register(Dictionary<Type, MethodInfo> packMethods, Dictionary<Type, MethodInfo> unpackMethods)
			{
				RegisterPackMethods(packMethods);
				RegisterUnpackMethods(unpackMethods);
			}

			private static void RegisterPackMethods(Dictionary<Type, MethodInfo> packMethods)
			{
				Type typeFromHandle = typeof(DefaultPackMethods);
				MethodInfo[] methods = typeFromHandle.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
				string text = "Pack";
				for (int i = 0; i < methods.Length; i++)
				{
					if (text.Equals(methods[i].Name))
					{
						ParameterInfo[] parameters = methods[i].GetParameters();
						if (parameters.Length == 2 && !(parameters[0].ParameterType != typeof(MsgPackWriter)))
						{
							packMethods.Add(parameters[1].ParameterType, methods[i]);
						}
					}
				}
			}

			internal static void Pack(MsgPackWriter writer, string x)
			{
				if (x == null)
				{
					writer.WriteNil();
				}
				else
				{
					writer.Write(x, highProbAscii: false);
				}
			}

			private static void RegisterUnpackMethods(Dictionary<Type, MethodInfo> unpackMethods)
			{
				BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.NonPublic;
				Type typeFromHandle = typeof(DefaultPackMethods);
				MethodInfo method = typeFromHandle.GetMethod("Unpack_Signed", bindingAttr);
				unpackMethods.Add(typeof(sbyte), method);
				unpackMethods.Add(typeof(short), method);
				unpackMethods.Add(typeof(int), method);
				method = typeFromHandle.GetMethod("Unpack_Signed64", bindingAttr);
				unpackMethods.Add(typeof(long), method);
				method = typeFromHandle.GetMethod("Unpack_Unsigned", bindingAttr);
				unpackMethods.Add(typeof(byte), method);
				unpackMethods.Add(typeof(ushort), method);
				unpackMethods.Add(typeof(char), method);
				unpackMethods.Add(typeof(uint), method);
				method = typeFromHandle.GetMethod("Unpack_Unsigned64", bindingAttr);
				unpackMethods.Add(typeof(ulong), method);
				method = typeFromHandle.GetMethod("Unpack_Boolean", bindingAttr);
				unpackMethods.Add(typeof(bool), method);
				method = typeFromHandle.GetMethod("Unpack_Float", bindingAttr);
				unpackMethods.Add(typeof(float), method);
				method = typeFromHandle.GetMethod("Unpack_Double", bindingAttr);
				unpackMethods.Add(typeof(double), method);
				method = typeFromHandle.GetMethod("Unpack_String", bindingAttr);
				unpackMethods.Add(typeof(string), method);
			}

			internal static int Unpack_Signed(MsgPackReader reader)
			{
				if (!reader.Read() || !reader.IsSigned())
				{
					UnpackFailed();
				}
				return reader.ValueSigned;
			}

			internal static long Unpack_Signed64(MsgPackReader reader)
			{
				if (!reader.Read())
				{
					UnpackFailed();
				}
				if (reader.IsSigned())
				{
					return reader.ValueSigned;
				}
				if (reader.IsSigned64())
				{
					return reader.ValueSigned64;
				}
				UnpackFailed();
				return 0L;
			}

			internal static uint Unpack_Unsigned(MsgPackReader reader)
			{
				if (!reader.Read() || !reader.IsUnsigned())
				{
					UnpackFailed();
				}
				return reader.ValueUnsigned;
			}

			internal static ulong Unpack_Unsigned64(MsgPackReader reader)
			{
				if (!reader.Read())
				{
					UnpackFailed();
				}
				if (reader.IsUnsigned())
				{
					return reader.ValueUnsigned;
				}
				if (reader.IsUnsigned64())
				{
					return reader.ValueUnsigned64;
				}
				UnpackFailed();
				return 0uL;
			}

			internal static bool Unpack_Boolean(MsgPackReader reader)
			{
				if (!reader.Read() || !reader.IsBoolean())
				{
					UnpackFailed();
				}
				return reader.ValueBoolean;
			}

			internal static float Unpack_Float(MsgPackReader reader)
			{
				if (!reader.Read() || reader.Type != TypePrefixes.Float)
				{
					UnpackFailed();
				}
				return reader.ValueFloat;
			}

			internal static double Unpack_Double(MsgPackReader reader)
			{
				if (!reader.Read() || reader.Type != TypePrefixes.Double)
				{
					UnpackFailed();
				}
				return reader.ValueDouble;
			}

			internal static string Unpack_String(MsgPackReader reader)
			{
				if (!reader.Read() || !reader.IsRaw())
				{
					UnpackFailed();
				}
				return reader.ReadRawString();
			}

			internal static void UnpackFailed()
			{
				throw new FormatException();
			}
		}

		private static PackerBase _publicFieldPacker;

		private static PackerBase _allFieldPacker;

		private PackerBase _packer;

		static CompiledPacker()
		{
			_publicFieldPacker = new MethodBuilderPacker();
			_allFieldPacker = new DynamicMethodPacker();
		}

		public CompiledPacker()
			: this(packPrivateField: false)
		{
		}

		public CompiledPacker(bool packPrivateField)
		{
			_packer = (packPrivateField ? _allFieldPacker : _publicFieldPacker);
		}

		public void Prepare<T>()
		{
			_packer.CreatePacker<T>();
			_packer.CreateUnpacker<T>();
		}

		public byte[] Pack<T>(T o)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Pack(memoryStream, o);
				return memoryStream.ToArray();
			}
		}

		public void Pack<T>(Stream strm, T o)
		{
			_packer.CreatePacker<T>()(new MsgPackWriter(strm), o);
		}

		public T Unpack<T>(byte[] buf)
		{
			return Unpack<T>(buf, 0, buf.Length);
		}

		public T Unpack<T>(byte[] buf, int offset, int size)
		{
			using (MemoryStream strm = new MemoryStream(buf, offset, size))
			{
				return Unpack<T>(strm);
			}
		}

		public T Unpack<T>(Stream strm)
		{
			return _packer.CreateUnpacker<T>()(new MsgPackReader(strm));
		}

		public byte[] Pack(object o)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Pack(memoryStream, o);
				return memoryStream.ToArray();
			}
		}

		public void Pack(Stream strm, object o)
		{
			throw new NotImplementedException();
		}

		public object Unpack(Type t, byte[] buf)
		{
			return Unpack(t, buf, 0, buf.Length);
		}

		public object Unpack(Type t, byte[] buf, int offset, int size)
		{
			using (MemoryStream strm = new MemoryStream(buf, offset, size))
			{
				return Unpack(t, strm);
			}
		}

		public object Unpack(Type t, Stream strm)
		{
			throw new NotImplementedException();
		}
	}
}
