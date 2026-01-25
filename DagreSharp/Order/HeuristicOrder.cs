using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static DagreSharp.Order.LayerGraph;

namespace DagreSharp.Order
{
	public static class HeuristicOrder
	{
		/*
		* Applies heuristics to minimize edge crossings in the graph and sets the best
		* order solution as an order attribute on each node.
		*
		* Pre-conditions:
		*
		*    1. Graph must be DAG
		*    2. Graph nodes must be objects with a "rank" attribute
		*    3. Graph edges must have the "weight" attribute
		*
		* Post-conditions:
		*
		*    1. Graph nodes will have an "order" attribute based on the results of the
		*       algorithm.
		*/
		public static void Run(Graph g)
		{
			var maxRank = Util.MaxRank(g);
			var downLayerGraphs = BuildLayerGraphs(g, Util.Range(1, maxRank + 1), (v, u) => g.GetInEdges(v, u));
			var upLayerGraphs = BuildLayerGraphs(g, Util.Range(maxRank - 1, -1, -1), (v, u) => g.GetOutEdges(v, u));

			var layering = InitialOrder.Run(g);
			AssignOrder(layering);

			var bestCC = int.MaxValue;
			var best = new List<List<Node>>();
			var i = 0;

			for (var lastBest = 0; lastBest < 4; lastBest++)
			{
				SweepLayerGraphs(i % 2 == 0 ? downLayerGraphs : upLayerGraphs, i % 4 >= 2);

				layering = Util.BuildLayerMatrix(g);
				var cc = CrossCounter.CrossCount(g, layering);

				if (cc < bestCC)
				{
					lastBest = 0;
					best = layering;
					bestCC = cc;
				}
				
				i++;
			}

			AssignOrder(best);
		}

		private static List<Graph> BuildLayerGraphs(Graph g, ICollection<int> ranks, RelationShipFunc relationshipFunc)
		{
			var nodesWithRank = new Dictionary<int, List<Node>>();

			void AddNodeToRank(Node node)
			{
				if (nodesWithRank.TryGetValue(node.Rank.Value, out var nodes))
				{
					nodes.Add(node);
				}
				else
				{
					nodesWithRank.Add(node.Rank.Value, new List<Node>(new[] { node }));
				}
			}


			foreach (var node in g.Nodes)
			{
				if (node.Rank.HasValue)
				{
					AddNodeToRank(node);
				}

				if (node.MinRank.HasValue && node.MaxRank.HasValue)
				{
					for (int i = node.MinRank.Value; i <= node.MaxRank.Value; i++)
					{
						if (node.Rank.HasValue && i != node.Rank.Value)
						{
							AddNodeToRank(node);
						}
					}
				}
			}

			return ranks.Select(r => LayerGraph.Build(
				g,
				r,
				relationshipFunc,
				nodesWithRank.TryGetValue(r, out var nodes) ? nodes : null)).ToList();
		}

		private static void AssignOrder(List<List<Node>> layering)
		{
			foreach (var layer in layering)
			{
				for (var i = 0; i < layer.Count; i++)
				{
					var v = layer[i];
					v.Order = i;
				}
			}
		}

		private static void SweepLayerGraphs(List<Graph> layerGraphs, bool biasRight)
		{
			var cg = new Graph();
			foreach (var lg in layerGraphs)
			{
				var root = lg.Options.Root ?? throw new InvalidOperationException("root is null");
				var sorted = SubGraph.Sort(lg, root, cg, biasRight);

				for (int i = 0; i < sorted.Vs.Count; i++)
				{
					var item = sorted.Vs[i];
					var node = lg.GetNode(item);
					node.Order = i;
				}

				AddSubgraphConstraints(lg, cg, sorted.Vs);
			}
		}

		public static void AddSubgraphConstraints(Graph g, Graph cg, List<string> vs)
		{
			var prev = new Dictionary<string, string>();
			string rootPrev = null;

			foreach (var v in vs)
			{
				var child = g.FindParent(v);
				string parent = null;
				string prevChild = null;

				while (child != null)
				{
					parent = g.FindParent(child);
					if (parent != null)
					{
						prevChild = prev.TryGetValue(parent, out var value) ? value : null;
						prev[parent] = child;
					}
					else
					{
						prevChild = rootPrev;
						rootPrev = child;
					}
					if (prevChild != null && prevChild != child)
					{
						cg.SetEdge(prevChild, child);
						break;
					}
					child = parent;
				}
			}
		}

	}
}
