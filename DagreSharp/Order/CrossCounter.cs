using DagreSharp.GraphLibrary;
using System.Collections.Generic;

namespace DagreSharp.Order
{
	public static class CrossCounter
	{
		/*
		* A function that takes a layering (an array of layers, each with an array of
		* ordererd nodes) and a graph and returns a weighted crossing count.
		*
		* Pre-conditions:
		*
		*    1. Input graph must be simple (not a multigraph), directed, and include
		*       only simple edges.
		*    2. Edges in the input graph must have assigned weights.
		*
		* Post-conditions:
		*
		*    1. The graph and layering matrix are left unchanged.
		*
		* This algorithm is derived from Barth, et al., "Bilayer Cross Counting."
		*/
		public static int CrossCount(Graph g, List<List<Node>> layering)
		{
			var cc = 0;
			for (var i = 1; i < layering.Count; ++i)
			{
				cc += TwoLayerCrossCount(g, layering[i - 1], layering[i]);
			}
			return cc;
		}

		private struct LayerEntry
		{
			public int Pos { get; set; }

			public int Weight { get; set; }
		}

		private static int TwoLayerCrossCount(Graph g, List<Node> northLayer, List<Node> southLayer)
		{
			// Sort all of the edges between the north and south layers by their position
			// in the north layer and then the south. Map these edges to the position of
			// their head in the south layer.
			var southPos = new Dictionary<string, int>();

			for (int i = 0; i < southLayer.Count; i++)
			{
				var s = southLayer[i];
				southPos.Add(s.Id, i);
			}

			var southEntries = new List<LayerEntry>();

			foreach (var entry in northLayer)
			{
				var edges = new List<LayerEntry>();
				
				foreach (var edge in g.GetOutEdges(entry.Id))
				{
					edges.Add(new LayerEntry { Pos = southPos[edge.To], Weight = edge.Weight });
				}

				edges.Sort(Comparer<LayerEntry>.Create((a, b) => a.Pos - b.Pos));
				southEntries.AddRange(edges);
			}

			// Build the accumulator tree
			var firstIndex = 1;
			while (firstIndex < southLayer.Count)
			{
				firstIndex <<= 1;
			}

			var treeSize = 2 * firstIndex - 1;
			firstIndex -= 1;
			var tree = new int[treeSize];

			for (int i = 0; i < treeSize; i++)
			{
				tree[i] = 0;
			}

			// Calculate the weighted crossings
			var cc = 0;
			foreach (var entry in southEntries)
			{
				var index = entry.Pos + firstIndex;
				tree[index] += entry.Weight;
				var weightSum = 0;
				while (index > 0)
				{
					if (index % 2 != 0)
					{
						weightSum += tree[index + 1];
					}
					index = (index - 1) >> 1;
					tree[index] += entry.Weight;
				}
				cc += entry.Weight * weightSum;
			};

			return cc;
		}
	}
}
