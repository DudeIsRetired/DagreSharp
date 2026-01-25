using DagreSharp.GraphLibrary;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.Order
{
	public static class InitialOrder
	{
		private class NodeRankComparer : IComparer<Node>
		{
			public int Compare(Node x, Node y)
			{
				if (x == null)
				{
					return y == null ? 0 : -1;
				}
				else if (y == null)
				{
					return 1;
				}

				if (!x.Rank.HasValue)
				{
					return y.Rank.HasValue ? -1 : 0;
				}
				else if (!y.Rank.HasValue)
				{
					return 1;
				}

				return x.Rank.Value - y.Rank.Value;
			}
		}

		/*
		* Assigns an initial order value for each node by performing a DFS search
		* starting from nodes in the first rank. Nodes are assigned an order in their
		* rank as they are first visited.
		*
		* This approach comes from Gansner, et al., "A Technique for Drawing Directed
		* Graphs."
		*
		* Returns a layering matrix with an array per layer and each layer sorted by
		* the order of its nodes.
		*/
		public static List<List<Node>> Run(Graph g)
		{
			var visited = new HashSet<string>();
			var simpleNodes = new List<Node>();
			var maxRank = int.MinValue;

			foreach (var node in g.Nodes)
			{
				if (g.GetChildren(node.Id).Count != 0)
				{
					continue;
				}

				if (node.Rank.HasValue && node.Rank.Value > maxRank)
				{
					maxRank = node.Rank.Value;
				}

				simpleNodes.Add(node);
			}

			List<List<Node>> layers = new List<List<Node>>();

			foreach (var _ in Enumerable.Range(0, maxRank + 1))
			{
				layers.Add(new List<Node>());
			}

			void DepthFirstSearch(Node node)
			{
				if (visited.Contains(node.Id))
				{
					return;
				}

				visited.Add(node.Id);

				if (node.Rank.HasValue)
				{
					layers[node.Rank.Value].Add(node);
				}

				foreach (var succ in g.GetSuccessors(node.Id))
				{
					DepthFirstSearch(succ);
				}
			}

			simpleNodes.Sort(new NodeRankComparer());

			foreach (var node in simpleNodes)
			{
				DepthFirstSearch(node);
			}

			return layers;
		}
	}
}
