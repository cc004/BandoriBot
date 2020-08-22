using System;
using System.Collections.Generic;

namespace MsgPack
{
	public static class ReflectionCache
	{
		private static Dictionary<Type, ReflectionCacheEntry> _cache;

		static ReflectionCache()
		{
			_cache = new Dictionary<Type, ReflectionCacheEntry>();
		}

		public static ReflectionCacheEntry Lookup(Type type)
		{
			ReflectionCacheEntry value;
			lock (_cache)
			{
				if (_cache.TryGetValue(type, out value))
				{
					return value;
				}
			}
			value = new ReflectionCacheEntry(type);
			lock (_cache)
			{
				_cache[type] = value;
				return value;
			}
		}

		public static void RemoveCache(Type type)
		{
			lock (_cache)
			{
				_cache.Remove(type);
			}
		}

		public static void Clear()
		{
			lock (_cache)
			{
				_cache.Clear();
			}
		}
	}
}
