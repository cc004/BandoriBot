using System;
using System.IO;
using System.Text;

namespace MsgPack
{
	public class MsgPackReader
	{
		private Stream _strm;

		private byte[] _tmp0 = new byte[8];

		private byte[] _tmp1 = new byte[8];

		private Encoding _encoding = Encoding.UTF8;

		private Decoder _decoder = Encoding.UTF8.GetDecoder();

		private byte[] _buf = new byte[64];

		public TypePrefixes Type
		{
			get;
			private set;
		}

		public bool ValueBoolean
		{
			get;
			private set;
		}

		public uint Length
		{
			get;
			private set;
		}

		public uint ValueUnsigned
		{
			get;
			private set;
		}

		public ulong ValueUnsigned64
		{
			get;
			private set;
		}

		public int ValueSigned
		{
			get;
			private set;
		}

		public long ValueSigned64
		{
			get;
			private set;
		}

		public float ValueFloat
		{
			get;
			private set;
		}

		public double ValueDouble
		{
			get;
			private set;
		}

		public MsgPackReader(Stream strm)
		{
			_strm = strm;
		}

		public bool IsSigned()
		{
			if (Type != TypePrefixes.NegativeFixNum && Type != 0 && Type != TypePrefixes.Int8 && Type != TypePrefixes.Int16)
			{
				return Type == TypePrefixes.Int32;
			}
			return true;
		}

		public bool IsBoolean()
		{
			if (Type != TypePrefixes.True)
			{
				return Type == TypePrefixes.False;
			}
			return true;
		}

		public bool IsSigned64()
		{
			return Type == TypePrefixes.Int64;
		}

		public bool IsUnsigned()
		{
			if (Type != 0 && Type != TypePrefixes.UInt8 && Type != TypePrefixes.UInt16)
			{
				return Type == TypePrefixes.UInt32;
			}
			return true;
		}

		public bool IsUnsigned64()
		{
			return Type == TypePrefixes.UInt64;
		}

		public bool IsRaw()
		{
			if (Type != TypePrefixes.FixRaw && Type != TypePrefixes.Raw16)
			{
				return Type == TypePrefixes.Raw32;
			}
			return true;
		}

		public bool IsArray()
		{
			if (Type != TypePrefixes.FixArray && Type != TypePrefixes.Array16)
			{
				return Type == TypePrefixes.Array32;
			}
			return true;
		}

		public bool IsMap()
		{
			if (Type != TypePrefixes.FixMap && Type != TypePrefixes.Map16)
			{
				return Type == TypePrefixes.Map32;
			}
			return true;
		}

		public bool Read()
		{
			byte[] tmp = _tmp0;
			byte[] tmp2 = _tmp1;
			int num = _strm.ReadByte();
			if (num < 0)
			{
				return false;
			}
			if (num >= 0 && num <= 127)
			{
				Type = TypePrefixes.PositiveFixNum;
			}
			else if (num >= 224 && num <= 255)
			{
				Type = TypePrefixes.NegativeFixNum;
			}
			else if (num >= 160 && num <= 191)
			{
				Type = TypePrefixes.FixRaw;
			}
			else if (num >= 144 && num <= 159)
			{
				Type = TypePrefixes.FixArray;
			}
			else if (num >= 128 && num <= 143)
			{
				Type = TypePrefixes.FixMap;
			}
			else
			{
				Type = (TypePrefixes)num;
			}
			switch (Type)
			{
			case TypePrefixes.False:
				ValueBoolean = false;
				break;
			case TypePrefixes.True:
				ValueBoolean = true;
				break;
			case TypePrefixes.Float:
				_strm.Read(tmp, 0, 4);
				if (BitConverter.IsLittleEndian)
				{
					tmp2[0] = tmp[3];
					tmp2[1] = tmp[2];
					tmp2[2] = tmp[1];
					tmp2[3] = tmp[0];
					ValueFloat = BitConverter.ToSingle(tmp2, 0);
				}
				else
				{
					ValueFloat = BitConverter.ToSingle(tmp, 0);
				}
				break;
			case TypePrefixes.Double:
				_strm.Read(tmp, 0, 8);
				if (BitConverter.IsLittleEndian)
				{
					tmp2[0] = tmp[7];
					tmp2[1] = tmp[6];
					tmp2[2] = tmp[5];
					tmp2[3] = tmp[4];
					tmp2[4] = tmp[3];
					tmp2[5] = tmp[2];
					tmp2[6] = tmp[1];
					tmp2[7] = tmp[0];
					ValueDouble = BitConverter.ToDouble(tmp2, 0);
				}
				else
				{
					ValueDouble = BitConverter.ToDouble(tmp, 0);
				}
				break;
			case TypePrefixes.NegativeFixNum:
				ValueSigned = (num & 0x1F) - 32;
				break;
			case TypePrefixes.PositiveFixNum:
				ValueSigned = (num & 0x7F);
				ValueUnsigned = (uint)ValueSigned;
				break;
			case TypePrefixes.UInt8:
				num = _strm.ReadByte();
				if (num < 0)
				{
					throw new FormatException();
				}
				ValueUnsigned = (uint)num;
				break;
			case TypePrefixes.UInt16:
				if (_strm.Read(tmp, 0, 2) != 2)
				{
					throw new FormatException();
				}
				ValueUnsigned = (uint)((tmp[0] << 8) | tmp[1]);
				break;
			case TypePrefixes.UInt32:
				if (_strm.Read(tmp, 0, 4) != 4)
				{
					throw new FormatException();
				}
				ValueUnsigned = (uint)((tmp[0] << 24) | (tmp[1] << 16) | (tmp[2] << 8) | tmp[3]);
				break;
			case TypePrefixes.UInt64:
				if (_strm.Read(tmp, 0, 8) != 8)
				{
					throw new FormatException();
				}
				ValueUnsigned64 = (((ulong)tmp[0] << 56) | ((ulong)tmp[1] << 48) | ((ulong)tmp[2] << 40) | ((ulong)tmp[3] << 32) | ((ulong)tmp[4] << 24) | ((ulong)tmp[5] << 16) | ((ulong)tmp[6] << 8) | tmp[7]);
				break;
			case TypePrefixes.Int8:
				num = _strm.ReadByte();
				if (num < 0)
				{
					throw new FormatException();
				}
				ValueSigned = (sbyte)num;
				break;
			case TypePrefixes.Int16:
				if (_strm.Read(tmp, 0, 2) != 2)
				{
					throw new FormatException();
				}
				ValueSigned = (short)((tmp[0] << 8) | tmp[1]);
				break;
			case TypePrefixes.Int32:
				if (_strm.Read(tmp, 0, 4) != 4)
				{
					throw new FormatException();
				}
				ValueSigned = ((tmp[0] << 24) | (tmp[1] << 16) | (tmp[2] << 8) | tmp[3]);
				break;
			case TypePrefixes.Int64:
				if (_strm.Read(tmp, 0, 8) != 8)
				{
					throw new FormatException();
				}
				ValueSigned64 = (long)(((ulong)tmp[0] << 56) | ((ulong)tmp[1] << 48) | ((ulong)tmp[2] << 40) | ((ulong)tmp[3] << 32) | ((ulong)tmp[4] << 24) | ((ulong)tmp[5] << 16) | ((ulong)tmp[6] << 8) | tmp[7]);
				break;
			case TypePrefixes.FixRaw:
				Length = (uint)(num & 0x1F);
				break;
			case TypePrefixes.FixMap:
			case TypePrefixes.FixArray:
				Length = (uint)(num & 0xF);
				break;
				case TypePrefixes.Raw8:
					if (_strm.Read(tmp, 0, 1) != 1)
					{
						throw new FormatException();
					}
					Length = (uint)(tmp[0]);
					break;
				case TypePrefixes.Raw16:
			case TypePrefixes.Array16:
			case TypePrefixes.Map16:
				if (_strm.Read(tmp, 0, 2) != 2)
				{
					throw new FormatException();
				}
				Length = (uint)((tmp[0] << 8) | tmp[1]);
				break;
			case TypePrefixes.Raw32:
			case TypePrefixes.Array32:
			case TypePrefixes.Map32:
				if (_strm.Read(tmp, 0, 4) != 4)
				{
					throw new FormatException();
				}
				Length = (uint)((tmp[0] << 24) | (tmp[1] << 16) | (tmp[2] << 8) | tmp[3]);
				break;
			default:
				throw new FormatException(Type.ToString());
			case TypePrefixes.Nil:
				break;
			}
			return true;
		}

		public int ReadValueRaw(byte[] buf, int offset, int count)
		{
			return _strm.Read(buf, offset, count);
		}

		public string ReadRawString()
		{
			return ReadRawString(_buf);
		}

		public string ReadRawString(byte[] buf)
		{
			if (Length < buf.Length)
			{
				if (ReadValueRaw(buf, 0, (int)Length) != Length)
				{
					throw new FormatException();
				}
				return _encoding.GetString(buf, 0, (int)Length);
			}
			byte[] array = new byte[Length];
			if (ReadValueRaw(array, 0, array.Length) != array.Length)
			{
				throw new FormatException();
			}
			return _encoding.GetString(array);
		}
	}
}
