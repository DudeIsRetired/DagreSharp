using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.GraphLibrary
{
	internal static class CollectionExtensions
	{
		public static void InitOrAdd(this Dictionary<string, List<Node>> map, string key, Node value)
		{
			if (map.TryGetValue(key, out List<Node> nodeList))
			{
				nodeList.Add(value);
			}
			else
			{
				map.Add(key, new List<Node>(new[] { value }));
			}
		}

		public static void RemoveOrClear(this Dictionary<string, List<Node>> map, string key, string subKey)
		{
			if (map.TryGetValue(key, out List<Node> nodeList))
			{
				var node = nodeList.FirstOrDefault(n => n.Id == subKey);

				if (node != null)
				{
					nodeList.Remove(node);
				}

				if (map[key].Count == 0)
				{
					map.Remove(key);
				}
			}
		}

		public static void RemoveAll<T, K>(this Dictionary<string, List<T>> map, string key, Func<T, K> remove)
		{
			if (map.TryGetValue(key, out List<T> value))
			{
				var items = value.ToList();
				foreach (var item in items)
				{
					remove(item);
				}

				map.Remove(key);
			}
		}

	}
}
