using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;

namespace DagreSharp
{
	public class GreedyFAS
	{
		private struct FasGraphState
		{
			public Graph Graph { get; set; }

			public List<Queue<Node>> Buckets { get; set; }

			public int ZeroIndex { get; set; }
		}

		private static readonly Func<Edge, int> DefaultWeightFunc = e => 1;

		public static List<Edge> Run(Graph g, Func<Edge, int> weightFn = null)
		{
			if (g.Nodes.Count <= 1)
			{
				return new List<Edge>();
			}

			if (weightFn == null)
			{
				weightFn = DefaultWeightFunc;
			}
			
			var state = BuildState(g, weightFn);
			var result = DoGreedyFAS(state.Graph, state.Buckets, state.ZeroIndex);

			// Expand multi-edges
			return result;
		}

		private static List<Edge> DoGreedyFAS(Graph g, List<Queue<Node>> buckets, int zeroIdx)
		{
			var result = new List<Edge>();
			var sources = buckets[buckets.Count - 1];
			var sinks = buckets[0];

			while (g.Nodes.Count > 0)
			{
				Node entry = sinks.Count == 0 ? null : sinks.Dequeue();
				while (entry != null)
				{
					RemoveNode(g, buckets, zeroIdx, entry);
					entry = sinks.Count == 0 ? null : sinks.Dequeue();
				}

				entry = sources.Count == 0 ? null : sources.Dequeue();
				while (entry != null)
				{
					RemoveNode(g, buckets, zeroIdx, entry);
					entry = sources.Count == 0 ? null : sources.Dequeue();
				}

				if (g.Nodes.Count > 0)
				{
					for (var i = buckets.Count - 2; i > 0; --i)
					{
						entry = buckets[i].Count == 0 ? null : buckets[i].Dequeue();
						if (entry != null)
						{
							result.AddRange(RemoveNode(g, buckets, zeroIdx, entry, true));
							break;
						}
					}
				}
			}

			return result;
		}

		private static List<Edge> RemoveNode(Graph g, List<Queue<Node>> buckets, int zeroIdx, Node entry, bool collectPredecessors = false)
		{
			var result = new List<Edge>();

			foreach (var edge in g.GetInEdges(entry.Id))
			{
				var weight = edge.Weight;
				var uEntry = g.GetNodeInternal(edge.From);

				if (collectPredecessors)
				{
					//result.Add(new Edge(edge.From, edge.To) { Name = edge.Name });
					result.Add(((Edge)edge).Copy());
				}

				uEntry.Out -= weight;
				AssignBucket(buckets, zeroIdx, uEntry);
			}

			foreach (var edge in g.GetOutEdges(entry.Id))
			{
				var weight = edge.Weight;
				var w = edge.To;
				var wEntry = g.GetNodeInternal(w);
				wEntry.In -= weight;
				AssignBucket(buckets, zeroIdx, wEntry);
			}

			g.RemoveNode(entry.Id);

			return result;
		}

		private static FasGraphState BuildState(Graph g, Func<Edge, int> weightFn)
		{
			var fasGraph = new Graph(g.IsDirected, g.IsMultigraph, g.IsCompound);
			var maxIn = 0;
			var maxOut = 0;

			foreach (var node in g.GetNodes())
			{
				fasGraph.SetNode(node, n =>
				{
					n.In = 0;
					n.Out = 0;
				});
			}

			// Aggregate weights on nodes, but also sum the weights across multi-edges
			// into a single edge for the fasGraph.
			foreach (var edge in g.GetEdges())
			{
				var prevWeight = 0;
				var weight = weightFn(edge);
				var edgeWeight = prevWeight + weight;
				fasGraph.SetEdge(edge.From, edge.To, edge.Name, e => { e.Weight = edgeWeight; });
				maxOut = Math.Max(maxOut, fasGraph.GetNodeInternal(edge.From).Out += weight);
				maxIn = Math.Max(maxIn, fasGraph.GetNodeInternal(edge.To).In += weight);
			}

			var limit = maxOut + maxIn + 3;
			var zeroIdx = maxIn + 1;
			var buckets = new List<Queue<Node>>();

			for (var i = 0; i < limit; i++)
			{
				buckets.Add(new Queue<Node>());
			}
			
			foreach (var node in fasGraph.GetNodes())
			{
				AssignBucket(buckets, zeroIdx, node);
			}

			return new FasGraphState { Graph = fasGraph, Buckets = buckets, ZeroIndex = zeroIdx };
		}

		private static void AssignBucket(List<Queue<Node>> buckets, int zeroIdx, Node entry)
		{
			if (entry.Out == 0)
			{
				buckets[0].Enqueue(entry);
			}
			else if (entry.In == 0)
			{
				buckets[buckets.Count - 1].Enqueue(entry);
			}
			else
			{
				buckets[entry.Out - entry.In + zeroIdx].Enqueue(entry);
			}
		}
	}
}
