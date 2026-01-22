using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;

namespace DagreSharp
{
	public static class ParentDummyChains
	{
		private struct LowLim
		{
			public int Low { get; set; }

			public int Lim { get; set; }
		}

		public static void Run(Graph g)
		{
			var postorderNums = PostOrder(g);

			foreach (var node in g.OptionsInternal.DummyChains)
			{
				if (node.DummyEdge == null)
				{
					throw new InvalidOperationException("DummyEdge is null");
				}

				var edgeObj = node.DummyEdge;
				var pathData = FindPath(g, postorderNums, edgeObj.From, edgeObj.To);
				var path = pathData.Path;
				var lca = pathData.Lca;
				var pathIdx = 0;
				var pathV = path[pathIdx];
				var ascending = true;
				var nd = node;

				while (nd.Id != edgeObj.To)
				{
					if (ascending)
					{
						while (pathV != lca && !string.IsNullOrEmpty(pathV) && g.GetNodeInternal(pathV).MaxRank < nd.Rank)
						{
							pathIdx++;
							pathV = path[pathIdx];
						}

						if (pathV == lca)
						{
							ascending = false;
						}
					}

					if (!ascending)
					{
						while (pathIdx < path.Count - 1 && g.GetNodeInternal(path[pathIdx + 1]).MinRank <= nd.Rank)
						{
							pathIdx++;
						}

						pathV = path[pathIdx];
					}

					g.SetParent(nd.Id, pathV);
					nd = g.GetSuccessorsInternal(nd.Id)[0];
				}
			}
		}

		// Find a path from v to w through the lowest common ancestor (LCA). Return the
		// full path and the LCA.
		private static PathLca FindPath(Graph g, Dictionary<string, LowLim> postorderNums, string v, string w)
		{
			var vPath = new List<string>();
			var wPath = new List<string>();
			var low = Math.Min(postorderNums[v].Low, postorderNums[w].Low);
			var lim = Math.Max(postorderNums[v].Lim, postorderNums[w].Lim);

			// Traverse up from v to find the LCA
			var parent = v;
			do
			{
				parent = g.FindParent(parent);
				vPath.Add(parent);
			} while (!string.IsNullOrEmpty(parent) && (postorderNums[parent].Low > low || lim > postorderNums[parent].Lim));
			var lca = parent;

			// Traverse from w to LCA
			parent = g.FindParent(w);

			while (parent != lca)
			{
				wPath.Add(parent);
				parent = g.FindParent(parent);
			}

			wPath.Reverse();
			vPath.AddRange(wPath);

			return new PathLca { Path = vPath, Lca = lca };
		}

		private struct PathLca
		{
			public List<string> Path { get; set; }

			public string Lca { get; set; }
		}

		private static Dictionary<string, LowLim> PostOrder(Graph g)
		{
			var result = new Dictionary<string, LowLim>();
			var lim = 0;

			void dfs(string v)
			{
				var low = lim;

				foreach (var child in g.GetChildren(v))
				{
					dfs(child.Id);
				}

				var lowLim = new LowLim { Low = low, Lim = lim++ };
				if (!result.TryGetValue(v, out LowLim value))
				{
					result.Add(v, lowLim);
				}
				else
				{
					value = lowLim;
				}
			}

			var children = g.GetChildren();
			foreach (var child in children)
			{
				dfs(child.Id);
			}

			return result;
		}
	}
}
