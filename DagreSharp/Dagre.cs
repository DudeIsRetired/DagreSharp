using DagreSharp.GraphLibrary;
using DagreSharp.Order;
using DagreSharp.Rank;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp
{
	public class Dagre
	{
		public static bool IsDebug { get => Util.IsDebug; set => Util.IsDebug = value; }

		private readonly Graph _graph;

		public GraphOptions Options { get => _graph.Options; }

		public IReadOnlyCollection<Node> Nodes => _graph.Nodes;

		public IReadOnlyCollection<Edge> Edges => _graph.Edges;

		public Dagre(Graph graph)
		{
			_graph = graph ?? throw new ArgumentNullException(nameof(graph));
		}

		public Node SetNode(string id, Action<Node> configure = null)
		{
			return _graph.SetNode(id, configure);
		}

		public Edge SetEdge(string from, string to, string name = null, Action<Edge> configure = null)
		{
			return _graph.SetEdge(from, to, name, configure);
		}

		public Action<Node> ConfigureDefaultNode
		{
			get => _graph.ConfigureDefaultNode;
			set => _graph.ConfigureDefaultNode = value;
		}

		public Action<Edge> ConfigureDefaultEdge
		{
			get => _graph.ConfigureDefaultEdge;
			set => _graph.ConfigureDefaultEdge = value;
		}

		public void Layout()
		{
			Util.Time("layout", () =>
			{
				var layoutGraph = Util.Time("  buildLayoutGraph", () => BuildLayoutGraph());
				Util.Time("  runLayout", () => RunLayout(layoutGraph));
				Util.Time("  updateInputGraph", () => UpdateInputGraph(layoutGraph));
			});
		}

		private static void RunLayout(Graph g)
		{
			Util.Time("    makeSpaceForEdgeLabels", () => MakeSpaceForEdgeLabels(g));
			Util.Time("    removeSelfEdges", () => RemoveSelfEdges(g));
			Util.Time("    acyclic", () => Acyclic.Run(g));
			Util.Time("    nestingGraph.run", () => NestingGraph.Run(g));
			Util.Time("    rank", () => Ranker.Rank(Util.AsNonCompoundGraph(g)));
			Util.Time("    injectEdgeLabelProxies", () => InjectEdgeLabelProxies(g));
			Util.Time("    removeEmptyRanks", () => Util.RemoveEmptyRanks(g));
			Util.Time("    nestingGraph.cleanup", () => NestingGraph.Cleanup(g));
			Util.Time("    normalizeRanks", () => Util.NormalizeRanks(g));
			Util.Time("    assignRankMinMax", () => AssignRankMinMax(g));
			Util.Time("    removeEdgeLabelProxies", () => RemoveEdgeLabelProxies(g));
			Util.Time("    normalize.run", () => Normalize.Run(g));
			Util.Time("    parentDummyChains", () => ParentDummyChains.Run(g));
			Util.Time("    addBorderSegments", () => BorderSegments.Add(g));
			Util.Time("    order", () => HeuristicOrder.Run(g));
			Util.Time("    insertSelfEdges", () => InsertSelfEdges(g));
			Util.Time("    adjustCoordinateSystem", () => CoordinateSystem.Adjust(g));
			Util.Time("    position", () => Position(g));
			Util.Time("    positionSelfEdges", () => PositionSelfEdges(g));
			Util.Time("    removeBorderNodes", () => RemoveBorderNodes(g));
			Util.Time("    normalize.undo", () => Normalize.Undo(g));
			Util.Time("    fixupEdgeLabelCoords", () => FixupEdgeLabelCoords(g));
			Util.Time("    undoCoordinateSystem", () => CoordinateSystem.Undo(g));
			Util.Time("    translateGraph", () => TranslateGraph(g));
			Util.Time("    assignNodeIntersects", () => AssignNodeIntersects(g));
			Util.Time("    reversePoints", () => ReversePointsForReversedEdges(g));
			Util.Time("    acyclic.undo", () => Acyclic.Undo(g));
		}

		/*
		* Constructs a new graph from the input graph, which can be used for layout.
		* This process copies only whitelisted attributes from the input graph to the
		* layout graph. Thus this function serves as a good place to determine what
		* attributes can influence layout.
		*/
		private Graph BuildLayoutGraph()
		{
			var g = new Graph(true, true, true);
			g.OptionsInternal.CopyFrom(_graph.OptionsInternal);

			foreach (var node in _graph.GetNodes())
			{
				g.SetNode(node);
				g.SetParent(node.Id, _graph.FindParent(node.Id));

			}

			foreach (var e in _graph.GetEdges())
			{
				var newEdge = e.Copy();
				g.SetEdge(newEdge);
			}

			return g;
		}

		/*
		* This idea comes from the Gansner paper: to account for edge labels in our
		* layout we split each rank in half by doubling minlen and halving ranksep.
		* Then we can place labels at these mid-points between nodes.
		*
		* We also add some minimal padding to the width to push the label for the edge
		* away from the edge itself a bit.
		*/
		private static void MakeSpaceForEdgeLabels(Graph g)
		{
			var graph = g.Options;
			graph.RankSeparation /= 2;

			foreach (var edge in g.Edges)
			{
				edge.MinLength *= 2;

				if (edge.LabelPosition != LabelPosition.Center)
				{
					if (graph.RankDirection == RankDirection.TopBottom || graph.RankDirection == RankDirection.BottomTop)
					{
						edge.Width += edge.LabelOffset;
					}
					else
					{
						edge.Height += edge.LabelOffset;
					}
				}
			}
		}

		private static void RemoveSelfEdges(Graph g)
		{
			foreach (var e in g.GetEdges())
			{
				if (e.From == e.To)
				{
					var node = g.GetNodeInternal(e.From);
					node.SelfEdges.Clear();
					node.SelfEdges.Add(new SelfEdge(e));
					g.RemoveEdge(e);
				}
			}
		}

		/*
		 * Creates temporary dummy nodes that capture the rank in which each edge's
		 * label is going to, if it has one of non-zero width and height. We do this
		 * so that we can safely remove empty ranks while preserving balance for the
		 * label's position.
		 */
		private static void InjectEdgeLabelProxies(Graph g)
		{
			foreach (var edge in g.GetEdges())
			{
				if (edge.Width != 0 && edge.Height != 0)
				{
					var v = g.GetNodeInternal(edge.From);
					var w = g.GetNodeInternal(edge.To);

					Util.AddDummyNode(g, DummyType.EdgeProxy, "_ep", n =>
					{
						n.Rank = (w.Rank - v.Rank) / 2 + v.Rank;
						n.DummyEdge = edge;
					});
				}
			}
		}

		private static void AssignRankMinMax(Graph g)
		{
			var maxRank = 0;

			foreach (var node in g.GetNodes())
			{
				if (!string.IsNullOrEmpty(node.BorderTop) && !string.IsNullOrEmpty(node.BorderBottom))
				{
					node.MinRank = g.GetNodeInternal(node.BorderTop).Rank;
					node.MaxRank = g.GetNodeInternal(node.BorderBottom).Rank;

					if (node.MaxRank.HasValue)
					{
						maxRank = Math.Max(maxRank, node.MaxRank.Value);
					}
				}
			}

			g.OptionsInternal.MaxRank = maxRank;
		}

		private static void RemoveEdgeLabelProxies(Graph g)
		{
			foreach (var node in g.GetNodes())
			{
				if (node.DummyType == DummyType.EdgeProxy)
				{
					if (node.DummyEdge == null)
					{
						throw new InvalidOperationException("Edge-proxy specified, but not found");
					}

					if (node.Rank.HasValue)
					{
						node.DummyEdge.LabelRank = node.Rank.Value;
					}

					g.RemoveNode(node.Id);
				}
			}
		}

		private static void InsertSelfEdges(Graph g)
		{
			var layers = Util.BuildLayerMatrix(g);

			foreach (var layer in layers)
			{
				var orderShift = 0;

				for (int i = 0; i < layer.Count; i++)
				{
					var v = layer[i];
					var node = g.GetNodeInternal(v);
					node.Order = i + orderShift;

					foreach (var selfEdge in node.SelfEdges)
					{
						Util.AddDummyNode(g, DummyType.SelfEdge, "_se", n =>
						{
							n.Width = selfEdge.Edge.Width;
							n.Height = selfEdge.Edge.Height;
							n.Rank = node.Rank;
							n.Order = i + (++orderShift);
							n.DummyEdge = selfEdge.Edge;
							n.Name = selfEdge.Edge.From;
						});
					}
					node.SelfEdges.Clear();
				}
			}
		}

		public static void Position(Graph g)
		{
			g = Util.AsNonCompoundGraph(g);

			PositionY(g);
			var posX = BrandesKopf.PositionX(g);

			foreach (var kvp in posX)
			{
				g.GetNode(kvp.Key).X = kvp.Value;
			}
		}

		private static void PositionY(Graph g)
		{
			var layering = Util.BuildLayerMatrix(g);
			var rankSep = g.Options.RankSeparation;
			var prevY = 0.0;

			foreach (var layer in layering)
			{
				var maxHeight = layer.Select(l => g.GetNode(l).Height).Max();

				foreach (var l in layer)
				{
					g.GetNode(l).Y = (double)prevY + maxHeight / 2;
				}

				prevY += maxHeight + rankSep;
			}
		}

		private static void PositionSelfEdges(Graph g)
		{
			foreach (var node in g.GetNodes())
			{
				if (node.DummyType == DummyType.SelfEdge)
				{
					if (node.DummyEdge == null)
					{
						throw new InvalidOperationException("SelfEdge not present");
					}

					var selfNode = g.GetNode(node.DummyEdge.From);
					var x = selfNode.X + selfNode.Width / 2;
					var y = selfNode.Y;
					var dx = node.X - x;
					var dy = selfNode.Height / 2;
					g.SetEdge(node.DummyEdge);
					g.RemoveNode(node.Id);
					node.DummyEdge.Points.AddRange(new[] {
						new Point(x + 2 * dx / 3, y - dy),
						new Point(x + 5 * dx / 6, y - dy),
						new Point(x + dx, y),
						new Point(x + 5 * dx / 6, y + dy),
						new Point(x + 2 * dx / 3, y + dy)
						});
					node.DummyEdge.X = node.X;
					node.DummyEdge.Y = node.Y;
				}
			}
		}

		private static void RemoveBorderNodes(Graph g)
		{
			foreach (var node in g.GetNodes())
			{
				if (g.GetChildren(node.Id).Count > 0)
				{
					if (string.IsNullOrEmpty(node.BorderTop))
					{
						throw new InvalidOperationException("Node.BorderTop is null");
					}

					if (string.IsNullOrEmpty(node.BorderBottom))
					{
						throw new InvalidOperationException("Node.BorderBottom is null");
					}

					var t = g.GetNode(node.BorderTop);
					var b = g.GetNode(node.BorderBottom);
					//var l = g.GetNode(node.BorderLeft[node.BorderLeft.Count - 1]);
					var l = g.GetNode(node.BorderLeft.Values.Last());
					//var r = g.GetNode(node.BorderRight[node.BorderRight.Count - 1]);
					var r = g.GetNode(node.BorderRight.Values.Last());

					node.Width = Math.Abs(r.X - l.Y);
					node.Height = Math.Abs(b.Y - t.Y);
					node.X = l.X + node.Width / 2;
					node.Y = t.Y + node.Height / 2;
				}
			}

			foreach (var node in g.GetNodes())
			{
				if (node.DummyType == DummyType.Border)
				{
					g.RemoveNode(node.Id);
				}
			}
		}

		private static void FixupEdgeLabelCoords(Graph g)
		{
			foreach (var edge in g.Edges)
			{
				if (edge.X.HasValue)
				{
					if (edge.LabelPosition == LabelPosition.Left || edge.LabelPosition == LabelPosition.Right)
					{
						edge.Width -= edge.LabelOffset;
					}
					switch (edge.LabelPosition)
					{
						case LabelPosition.Left:
							edge.X -= edge.Width / 2 + edge.LabelOffset;
							break;
						case LabelPosition.Right:
							edge.X += edge.Width / 2 + edge.LabelOffset;
							break;
						default:
							break;
					}
				}
			}
		}

		private static void TranslateGraph(Graph g)
		{
			var minX = double.MaxValue;
			var maxX = 0.0;
			var minY = double.MaxValue;
			var maxY = 0.0;
			var options = g.OptionsInternal;
			var marginX = options.MarginX;
			var marginY = options.MarginY;

			foreach (var node in g.Nodes)
			{
				var x = node.X;
				var y = node.Y;
				var w = node.Width;
				var h = node.Height;
				minX = Math.Min(minX, x - w / 2);
				maxX = Math.Max(maxX, x + w / 2);
				minY = Math.Min(minY, y - h / 2);
				maxY = Math.Max(maxY, y + h / 2);
			}

			foreach (var edge in g.Edges)
			{
				if (edge.X.HasValue && edge.Y.HasValue)
				{
					var x = edge.X;
					var y = edge.Y;
					var w = edge.Width;
					var h = edge.Height;
					minX = Math.Min(minX, x.Value - w / 2);
					maxX = Math.Max(maxX, x.Value + w / 2);
					minY = Math.Min(minY, y.Value - h / 2);
					maxY = Math.Max(maxY, y.Value + h / 2);
				}
			}

			minX -= marginX;
			minY -= marginY;

			foreach (var node in g.Nodes)
			{
				node.X -= minX;
				node.Y -= minY;
			}

			foreach (var edge in g.GetEdges())
			{
				var points = new List<Point>();
				foreach (var point in edge.Points)
				{
					points.Add(new Point(point.X - minX, point.Y - minY));
				}
				
				edge.Points.Clear();
				edge.Points.AddRange(points);

				if (edge.X.HasValue)
				{
					edge.X -= minX;
				}

				if (edge.Y.HasValue)
				{
					edge.Y -= minY;
				}
			}

			options.Width = maxX - minX + marginX;
			options.Height = maxY - minY + marginY;
		}

		private static void AssignNodeIntersects(Graph g)
		{
			foreach (var edge in g.GetEdges())
			{
				var nodeV = g.GetNode(edge.From);
				var nodeW = g.GetNode(edge.To);
				Point p1, p2;

				if (edge.Points.Count == 0)
				{
					p1 = new Point(nodeW.X, nodeW.Y);
					p2 = new Point(nodeV.X, nodeV.Y);
				}
				else
				{
					p1 = edge.Points[0];
					p2 = edge.Points[edge.Points.Count - 1];
				}

				edge.Points.Insert(0, Util.IntersectRect(nodeV, p1));
				edge.Points.Add(Util.IntersectRect(nodeW, p2));
			}
		}

		private static void ReversePointsForReversedEdges(Graph g)
		{
			foreach (var edge in g.GetEdges())
			{
				if (edge.IsReversed)
				{
					edge.Points.Reverse();
				}
			}
		}

		/*
		* Copies final layout information from the layout graph back to the input
		* graph. This process only copies whitelisted attributes from the layout graph
		* to the input graph, so it serves as a good place to determine what
		* attributes can influence layout.
		*/
		private void UpdateInputGraph(Graph layoutGraph)
		{
			foreach (var inputNode in _graph.GetNodes())
			{
				var layoutLabel = layoutGraph.GetNodeInternal(inputNode.Id);

				if (inputNode != null)
				{
					inputNode.X = layoutLabel.X;
					inputNode.Y = layoutLabel.Y;
					inputNode.Rank = layoutLabel.Rank;

					if (layoutGraph.GetChildren(inputNode.Id).Count > 0)
					{
						inputNode.Width = layoutLabel.Width;
						inputNode.Height = layoutLabel.Height;
					}
				}
			}

			foreach (var inputEdge in _graph.GetEdges())
			{
				var layoutLabel = layoutGraph.GetEdge(inputEdge);

				inputEdge.Points.Clear();
				inputEdge.Points.AddRange(layoutLabel.Points);

				if (layoutLabel.X.HasValue)
				{
					inputEdge.X = layoutLabel.X;
					inputEdge.Y = layoutLabel.Y;
				}
			}

			_graph.OptionsInternal.Width = layoutGraph.OptionsInternal.Width;
			_graph.OptionsInternal.Height = layoutGraph.OptionsInternal.Height;
		}
	}
}

