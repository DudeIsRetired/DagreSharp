using DagreSharp.GraphLibrary;
using System;
using System.Linq;

namespace DagreSharp
{
	public static class Normalize
	{
		/*
		* Breaks any long edges in the graph into short segments that span 1 layer
		* each. This operation is undoable with the denormalize function.
		*
		* Pre-conditions:
		*
		*    1. The input graph is a DAG.
		*    2. Each node in the graph has a "rank" property.
		*
		* Post-condition:
		*
		*    1. All edges in the graph have a length of 1.
		*    2. Dummy nodes are added where edges have been split into segments.
		*    3. The graph is augmented with a "dummyChains" attribute which contains
		*       the first dummy in each chain of dummy nodes produced.
		*/
		public static void Run(Graph g)
		{
			g.Options.DummyChains.Clear();
			var edges = g.Edges.ToArray();

			foreach (var edge in edges)
			{
				NormalizeEdge(g, edge);
			}
		}

		private static void NormalizeEdge(Graph g, Edge edge)
		{
			var v = edge.From;
			var vRank = g.GetNode(v).Rank;
			var w = edge.To;
			var wRank = g.GetNode(w).Rank;
			var name = edge.Name;
			var labelRank = edge.LabelRank;

			if (wRank == vRank + 1)
			{
				return;
			}

			g.RemoveEdge(edge);

			vRank++;
			for (var i = 0; vRank < wRank; ++i, ++vRank)
			{
				edge.Points.Clear();

				var dummy = Util.AddDummyNode(g, DummyType.Edge, "_d", n =>
				{
					n.Width = 0;
					n.Height = 0;
					n.Rank = vRank;
					n.DummyEdge = edge;

					if (vRank == labelRank)
					{
						n.Width = edge.Width;
						n.Height = edge.Height;
						n.LabelPosition = edge.LabelPosition;
						n.DummyType = DummyType.EdgeLabel;
					}
				});

				g.SetEdge(v, dummy.Id, name, x => { x.Weight = edge.Weight; });

				if (i == 0)
				{
					g.Options.DummyChains.Add(dummy);
				}

				v = dummy.Id;
			}

			g.SetEdge(v, w, name, x => { x.Weight = edge.Weight; });
		}

		public static void Undo(Graph g)
		{
			foreach (var node in g.Options.DummyChains)
			{
				if (node.DummyEdge == null)
				{
					throw new InvalidOperationException("DummyEdge is null");
				}

				var origLabel = node.DummyEdge;
				g.SetEdge(origLabel);
				var nd = node;

				while (nd.DummyType != DummyType.None)
				{
					var w = g.GetSuccessors(nd.Id).First();
					g.RemoveNode(nd.Id);
					origLabel.Points.Add(new Point(nd.X, nd.Y));

					if (nd.DummyType == DummyType.EdgeLabel)
					{
						origLabel.X = nd.X;
						origLabel.Y = nd.Y;
						origLabel.Width = nd.Width;
						origLabel.Height = nd.Height;
					}

					var v = w;
					nd = v;
				}
			}
		}
	}
}
