using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace MsgPack
{
	public class BoxingPacker
	{
		private static Type KeyValuePairDefinitionType;

		static BoxingPacker()
		{
			KeyValuePairDefinitionType = typeof(KeyValuePair<object, object>).GetGenericTypeDefinition();
		}

		public void Pack(Stream strm, object o)
		{
			MsgPackWriter writer = new MsgPackWriter(strm);
			Pack(writer, o);
		}

		public byte[] Pack(object o)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Pack(memoryStream, o);
				return memoryStream.ToArray();
			}
		}

		private void Pack(MsgPackWriter writer, object o)
		{
			if (o == null)
			{
				writer.WriteNil();
				return;
			}

			if (o is string s)
			{
				writer.Write(Encoding.UTF8.GetBytes((string)o));
				return;
			}

			Type type = o.GetType();
			if (type.IsPrimitive)
			{
				if (type.Equals(typeof(int)))
				{
					writer.Write((int)o);
					return;
				}
				if (type.Equals(typeof(uint)))
				{
					writer.Write((uint)o);
					return;
				}
				if (type.Equals(typeof(float)))
				{
					writer.Write((float)o);
					return;
				}
				if (type.Equals(typeof(double)))
				{
					writer.Write((double)o);
					return;
				}
				if (type.Equals(typeof(long)))
				{
					writer.Write((long)o);
					return;
				}
				if (type.Equals(typeof(ulong)))
				{
					writer.Write((ulong)o);
					return;
				}
				if (type.Equals(typeof(bool)))
				{
					writer.Write((bool)o);
					return;
				}
				if (type.Equals(typeof(byte)))
				{
					writer.Write((byte)o);
					return;
				}
				if (type.Equals(typeof(sbyte)))
				{
					writer.Write((sbyte)o);
					return;
				}
				if (type.Equals(typeof(short)))
				{
					writer.Write((short)o);
					return;
				}
				if (type.Equals(typeof(ushort)))
				{
					writer.Write((ushort)o);
					return;
				}
				throw new NotSupportedException();
			}

			IDictionary dictionary = o as IDictionary;

			if (dictionary != null)
			{
				writer.WriteMapHeader(dictionary.Count);
				foreach (DictionaryEntry item in dictionary)
				{
					Pack(writer, item.Key);
					Pack(writer, item.Value);
				}
			}
			else
			{
				if (!type.IsArray)
				{
					return;
				}
				Array array = (Array)o;
				Type elementType = type.GetElementType();
				if (elementType.IsGenericType && elementType.GetGenericTypeDefinition().Equals(KeyValuePairDefinitionType))
				{
					PropertyInfo property = elementType.GetProperty("Key");
					PropertyInfo property2 = elementType.GetProperty("Value");
					writer.WriteMapHeader(array.Length);
					for (int i = 0; i < array.Length; i++)
					{
						object value = array.GetValue(i);
						Pack(writer, property.GetValue(value, null));
						Pack(writer, property2.GetValue(value, null));
					}
				}
				else
				{
					writer.WriteArrayHeader(array.Length);
					for (int j = 0; j < array.Length; j++)
					{
						Pack(writer, array.GetValue(j));
					}
				}
			}
		}

		public object Unpack(Stream strm)
		{
			MsgPackReader reader = new MsgPackReader(strm);
			var obj = Unpack(reader);
			return obj;
		}

		public object Unpack(byte[] buf, int offset, int size)
		{
			using (MemoryStream strm = new MemoryStream(buf, offset, size))
			{
				return Unpack(strm);
			}
		}

		public object Unpack(byte[] buf)
		{
			return Unpack(buf, 0, buf.Length);
		}

		private object Unpack(MsgPackReader reader)
		{
			if (!reader.Read())
			{
				throw new FormatException();
			}
			switch (reader.Type)
			{
			case TypePrefixes.PositiveFixNum:
			case TypePrefixes.Int8:
			case TypePrefixes.Int16:
			case TypePrefixes.Int32:
			case TypePrefixes.NegativeFixNum:
				return reader.ValueSigned;
			case TypePrefixes.Int64:
				return reader.ValueSigned64;
			case TypePrefixes.UInt8:
			case TypePrefixes.UInt16:
			case TypePrefixes.UInt32:
				return reader.ValueUnsigned;
			case TypePrefixes.UInt64:
				return reader.ValueUnsigned64;
			case TypePrefixes.True:
				return true;
			case TypePrefixes.False:
				return false;
			case TypePrefixes.Float:
				return reader.ValueFloat;
			case TypePrefixes.Double:
				return reader.ValueDouble;
			case TypePrefixes.Nil:
				return null;
			case TypePrefixes.FixRaw:
				case TypePrefixes.Raw8:
			case TypePrefixes.Raw16:
			case TypePrefixes.Raw32:
			{
				byte[] array2 = new byte[reader.Length];
				reader.ReadValueRaw(array2, 0, array2.Length);
				return array2;
			}
			case TypePrefixes.FixArray:
			case TypePrefixes.Array16:
			case TypePrefixes.Array32:
			{
				object[] array = new object[reader.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = Unpack(reader);
				}
				return array;
			}
			case TypePrefixes.FixMap:
			case TypePrefixes.Map16:
			case TypePrefixes.Map32:
			{
				IDictionary<object, object> dictionary = new Dictionary<object, object>((int)reader.Length);
				int length = (int)reader.Length;
				for (int i = 0; i < length; i++)
				{
					object key = Unpack(reader) ?? "null";
					object value = Unpack(reader);

							if (key is byte[] ba) key = Encoding.UTF8.GetString(ba);
					if (value is byte[] ba2) value = Encoding.UTF8.GetString(ba2);

					dictionary.Add(key, value);
				}
				return dictionary;
			}
			default:
				throw new FormatException();
			}
		}
	}
}
