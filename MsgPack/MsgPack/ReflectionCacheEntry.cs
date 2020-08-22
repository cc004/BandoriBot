using System;
using System.Collections.Generic;
using System.Reflection;

namespace MsgPack
{
	public class ReflectionCacheEntry
	{
		private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField;

		public IDictionary<string, FieldInfo> FieldMap
		{
			get;
			private set;
		}

		public ReflectionCacheEntry(Type t)
		{
			FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField);
			IDictionary<string, FieldInfo> dictionary = new Dictionary<string, FieldInfo>(fields.Length);
			foreach (FieldInfo fieldInfo in fields)
			{
				string text = fieldInfo.Name;
				int num;
				if (text[0] == '<' && (num = text.IndexOf('>')) > 1)
				{
					text = text.Substring(1, num - 1);
				}
				dictionary[text] = fieldInfo;
			}
			FieldMap = dictionary;
		}
	}
}
