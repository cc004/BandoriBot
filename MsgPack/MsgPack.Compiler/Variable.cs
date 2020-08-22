using System.Reflection.Emit;

namespace MsgPack.Compiler
{
	public class Variable
	{
		public VariableType VarType
		{
			get;
			set;
		}

		public int Index
		{
			get;
			set;
		}

		private Variable(VariableType type, int index)
		{
			VarType = type;
			Index = index;
		}

		public static Variable CreateLocal(LocalBuilder local)
		{
			return new Variable(VariableType.Local, local.LocalIndex);
		}

		public static Variable CreateArg(int idx)
		{
			return new Variable(VariableType.Arg, idx);
		}
	}
}
