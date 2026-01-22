using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.GraphLibrary
{
	public class Algorithm
	{
		private delegate void OrderFunc(Node node, Func<string, IList<Node>> navigation, HashSet<string> visited, List<string> acc);

		private class TarjanVisit
		{
			public bool OnStack { get; set; }
			public int LowLink { get; set; }
			public int Index { get; set; }
		}

		/// <summary>
		/// A helper that preforms a pre- or post-order traversal on the input graph
		/// and returns the nodes in the order they were visited.If the graph is
		/// undirected then this algorithm will navigate using neighbors. If the graph
		/// is directed then this algorithm will navigate using successors.
		/// If the order is not "post", it will be treated as "pre".
		/// </summary>
		public static List<string> Dfs(Graph g, ICollection<Node> vs, string order)
		{
			Func<string, List<Node>> navigation = null;

			if (g.IsDirected)
			{
				navigation = v => g.GetSuccessors(v);
			}
			else
			{
				navigation = v => g.GetNeighbors(v);
			}

			OrderFunc orderFunc = null;
			if (string.Compare(order, "post", StringComparison.OrdinalIgnoreCase) == 0)
			{
				orderFunc = PostOrderDfs;
			}
			else
			{
				orderFunc = PreOrderDfs;
			}

			var acc = new List<string>();
			var visited = new HashSet<string>();

			foreach (var node in vs)
			{
				orderFunc(node, navigation, visited, acc);
			}

			return acc;
		}

		private static void PostOrderDfs(Node node, Func<string, IList<Node>> navigation, HashSet<string> visited, List<string> acc)
		{
			var stack = new Stack<(string Id, bool IsDone)>();
			stack.Push((node.Id, false));

			while (stack.Count > 0)
			{
				var (Id, IsDone) = stack.Pop();

				if (IsDone)
				{
					acc.Add(Id);
				}
				else
				{
					if (!visited.Contains(Id))
					{
						stack.Push((Id, true));
						visited.Add(Id);
						ForEachRight(navigation(Id), (w, c, l) => stack.Push((w.Id, false)));
					}
				}
			}
		}

		private static void PreOrderDfs(Node node, Func<string, IList<Node>> navigation, HashSet<string> visited, List<string> acc)
		{
			var stack = new Stack<string>(new[] { node.Id });

			while (stack.Count > 0)
			{
				var curr = stack.Pop();
				if (!visited.Contains(curr))
				{
					acc.Add(curr);
					visited.Add(curr);
					ForEachRight(navigation(curr), (w, c, l) => stack.Push(w.Id));
				}
			}
		}

		private static IList<Node> ForEachRight(IList<Node> array, Action<Node, int, ICollection<Node>> iteratee)
		{
			var length = array.Count - 1;
			while (length >= 0)
			{
				iteratee(array[length], length, array);
				length--;
			}

			return array;
		}

		public static List<string> PreOrder(Graph g, ICollection<Node> vs)
		{
			return Dfs(g, vs, "pre");
		}

		public static List<string> PostOrder(Graph g, ICollection<Node> vs)
		{
			return Dfs(g, vs, "post");
		}

		public static List<List<string>> Tarjan(Graph g)
		{
			var index = 0;
			var stack = new Stack<string>();
			var visited = new Dictionary<string, TarjanVisit>();
			var results = new List<List<string>>();

			void dfs(string v)
			{
				var entry = new TarjanVisit()
				{
					OnStack = true,
					LowLink = index,
					Index = index++
				};
				visited.Add(v, entry);
				stack.Push(v);

				foreach (var w in g.GetSuccessors(v))
				{
					if (!visited.ContainsKey(w.Id))
					{
						dfs(w.Id);
						entry.LowLink = Math.Min(entry.LowLink, visited[w.Id].LowLink);
					}
					else if (visited[w.Id].OnStack)
					{
						entry.LowLink = Math.Min(entry.LowLink, visited[w.Id].Index);
					}
				}

				if (entry.LowLink == entry.Index)
				{
					var cmpt = new List<string>();
					string w;
					do
					{
						w = stack.Pop();
						visited[w].OnStack = false;
						cmpt.Add(w);
					} while (v != w);
					results.Add(cmpt);
				}
			}

			foreach (var v in g.Nodes)
			{
				if (!visited.ContainsKey(v.Id))
				{
					dfs(v.Id);
				}
			}

			return results;
		}

		public static List<List<string>> FindCycles(Graph g)
		{
			return Tarjan(g).Where(x => x.Count > 1 || (x.Count == 1 && g.HasEdge(x[0], x[0]))).ToList();
		}

		public static List<List<string>> Components(Graph g)
		{
			var visited = new Dictionary<string, bool>();
			var cmpts = new List<List<string>>();
			List<string> cmpt;

			void dfs(string v)
			{
				if (visited.ContainsKey(v))
				{
					return;
				}

				visited.Add(v, true);
				cmpt.Add(v);

				foreach (var node in g.GetSuccessors(v))
				{
					dfs(node.Id);
				}

				foreach (var node in g.GetPredecessors(v))
				{
					dfs(node.Id);
				}
			}

			foreach (var node in g.Nodes)
			{
				cmpt = new List<string>();
				dfs(node.Id);

				if (cmpt.Count > 0)
				{
					cmpts.Add(cmpt);
				}
			}

			return cmpts;
		}
	}
}
