using DagreSharp.GraphLibrary;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.Rank
{
	public static class Ranker
	{
		/*
		* Assigns a rank to each node in the input graph that respects the "minlen"
		* constraint specified on edges between nodes.
		*
		* This basic structure is derived from Gansner, et al., "A Technique for
		* Drawing Directed Graphs."
		*
		* Pre-conditions:
		*
		*    1. Graph must be a connected DAG
		*    2. Graph nodes must be objects
		*    3. Graph edges must have "weight" and "minlen" attributes
		*
		* Post-conditions:
		*
		*    1. Graph nodes will have a "rank" attribute based on the results of the
		*       algorithm. Ranks can start at any index (including negative), we'll
		*       fix them up later.
		*/
		public static void Rank(Graph g)
		{
			switch (g.Options.Ranker)
			{
				case GraphRank.NetworkSimplex:
					NetworkSimplex.Run(g);
					break;
				case GraphRank.TightTree:
					TightTreeRanker(g);
					break;
				case GraphRank.LongestPath:
					LongestPath(g);
					break;
				default:
					NetworkSimplex.Run(g);
					break;
			}
		}

		// A fast and simple ranker, but results are far from optimal.
		private static void TightTreeRanker(Graph g)
		{
			LongestPath(g);
			FeasibleTree.Run(g);
		}

		/*
		* Initializes ranks for the input graph using the longest path algorithm. This
		* algorithm scales well and is fast in practice, it yields rather poor
		* solutions. Nodes are pushed to the lowest layer possible, leaving the bottom
		* ranks wide and leaving edges longer than necessary. However, due to its
		* speed, this algorithm is good for getting an initial ranking that can be fed
		* into other algorithms.
		*
		* This algorithm does not normalize layers because it will be used by other
		* algorithms in most cases. If using this algorithm directly, be sure to
		* run normalize at the end.
		*
		* Pre-conditions:
		*
		*    1. Input graph is a DAG.
		*    2. Input graph node labels can be assigned properties.
		*
		* Post-conditions:
		*
		*    1. Each node will be assign an (unnormalized) "rank" property.
		*/
		public static void LongestPath(Graph g)
		{
			var visited = new HashSet<string>();

			int? DepthFirstSearch(Node node)
			{
				if (visited.Contains(node.Id))
				{
					return node.Rank;
				}

				visited.Add(node.Id);
				var rankValues = g.GetOutEdges(node.Id).Select(v =>
				{
					var to = g.GetNodeInternal(v.To);
					return DepthFirstSearch(to) - v.MinLength;
				}).ToList();
				var rank = rankValues.Count > 0 ? rankValues.Min() : 0;
				node.Rank = rank;

				return rank;
			}

			foreach (var v in g.GetSourcesInternal())
			{
				DepthFirstSearch(v);
			}
		}

		/*
		 * Returns the amount of slack for the given edge. The slack is defined as the
		 * difference between the length of the edge and its minimum length.
		 */
		public static int Slack(Graph g, Edge e)
		{
			var ewNode = g.GetNodeInternal(e.To);
			var evNode = g.GetNodeInternal(e.From);
			var ewRank = ewNode.Rank ?? 0;
			var evRank = evNode.Rank ?? 0;

			return ewRank - evRank - e.MinLength;
		}
	}
}
