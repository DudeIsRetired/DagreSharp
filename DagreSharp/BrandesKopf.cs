using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp
{
	public class BrandesKopf
	{
		public struct VerticalAlignment
		{
			public Dictionary<string, string> Root { get; set; }

			public Dictionary<string, string> Align { get; set; }
		}

		public static Dictionary<string, double> PositionX(Graph g)
		{
			var layering = Util.BuildLayerMatrix(g);

			var conflicts = FindConflicts(g, layering);
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>();
			var adjustedLayering = new List<List<string>>();
			Func<string, ICollection<Node>> neighborFn;

			foreach (GraphAlignment alignment in Enum.GetValues(typeof(GraphAlignment)))
			{
				if (alignment == GraphAlignment.None)
				{
					continue;
				}

				adjustedLayering = layering;
				neighborFn = g.GetPredecessors;

				if (alignment == GraphAlignment.DownLeft || alignment == GraphAlignment.DownRight)
				{
					adjustedLayering = layering.Select(x => x.ToList()).Reverse().ToList();
					neighborFn = g.GetSuccessors;
				}

				if (alignment == GraphAlignment.UpRight || alignment == GraphAlignment.DownRight)
				{
					adjustedLayering = adjustedLayering.Select(x => x.ToList()).ToList();
					foreach (var item in adjustedLayering)
					{
						item.Reverse();
					}
				}

				var align = VerticalAlign(adjustedLayering, conflicts, neighborFn);
				var horiz = alignment == GraphAlignment.UpRight || alignment == GraphAlignment.DownRight;
				var xs = HorizontalCompaction(g, adjustedLayering, align.Root, align.Align, horiz);

				if (alignment == GraphAlignment.UpRight || alignment == GraphAlignment.DownRight)
				{
					foreach (var x in xs)
					{
						xs[x.Key] = -x.Value;
					}
				}

				xss[alignment] = xs;
			}

			var smallestWidth = FindSmallestWidthAlignment(g, xss);
			AlignCoordinates(xss, smallestWidth);
			return Balance(xss, g.Options.Aligment);
		}

		private static Dictionary<string, Dictionary<string, bool>> FindConflicts(Graph g, List<List<string>> layering)
		{
			var type1Conflicts = FindType1Conflicts(g, layering);
			var type2Conflicts = FindType2Conflicts(g, layering);

			foreach (var t2 in type2Conflicts)
			{
				if (!type1Conflicts.TryGetValue(t2.Key, out Dictionary<string, bool> t1))
				{
					type1Conflicts.Add(t2.Key, t2.Value);
				}
				else
				{
					foreach (var item in t2.Value)
					{
						if (t1.ContainsKey(item.Key))
						{
							t1[item.Key] = item.Value;
						}
						else
						{
							t1.Add(item.Key, item.Value);
						}
					}
				}
			}

			var conflicts = type1Conflicts;
			return conflicts;
		}

		/*
		* Marks all edges in the graph with a type-1 conflict with the "type1Conflict"
		* property. A type-1 conflict is one where a non-inner segment crosses an
		* inner segment. An inner segment is an edge with both incident nodes marked
		* with the "dummy" property.
		*
		* This algorithm scans layer by layer, starting with the second, for type-1
		* conflicts between the current layer and the previous layer. For each layer
		* it scans the nodes from left to right until it reaches one that is incident
		* on an inner segment. It then scans predecessors to determine if they have
		* edges that cross that inner segment. At the end a final scan is done for all
		* nodes on the current rank to see if they cross the last visited inner
		* segment.
		*
		* This algorithm (safely) assumes that a dummy node will only be incident on a
		* single node in the layers being scanned.
		*/
		public static Dictionary<string, Dictionary<string, bool>> FindType1Conflicts(Graph g, List<List<string>> layering)
		{
			var conflicts = new Dictionary<string, Dictionary<string, bool>>();

			List<string> visitLayer(List<string> previousLayer, List<string> layer)
			{
				// last visited node in the previous layer that is incident on an inner segment.
				var k0 = 0;
				// Tracks the last node in this layer scanned for crossings with a type-1 segment.
				var scanPos = 0;
				var prevLayerLength = previousLayer.Count;
				var lastNode = layer[layer.Count - 1];

				for (int i = 0; i < layer.Count; i++)
				{
					var v = layer[i];
					var w = FindOtherInnerSegmentNode(g, v);
					var k1 = w != null ? w.Order : prevLayerLength;

					if (w != null || v == lastNode)
					{
						foreach (var scanNode in layer.GetRange(scanPos, i + 1 - scanPos))
						{
							foreach (var pred in g.GetPredecessors(scanNode))
							{
								var uPos = pred.Order;

								if ((uPos < k0 || k1 < uPos) &&
									!(pred.DummyType != DummyType.None && g.GetNode(scanNode).DummyType != DummyType.None))
								{
									AddConflict(conflicts, pred.Id, scanNode);
								}
							}
						}
						scanPos = i + 1;
						k0 = k1;
					}
				}

				return layer;
			}

			var prevLayer = layering[0];

			for (int i = 1; i < layering.Count; i++)
			{
				prevLayer = visitLayer(prevLayer, layering[i]);
			}

			return conflicts;
		}

		private static Node FindOtherInnerSegmentNode(Graph g, string v)
		{
			if (g.GetNode(v).DummyType != DummyType.None)
			{
				return g.GetPredecessors(v).FirstOrDefault(u => u.DummyType != DummyType.None);
			}

			return null;
		}

		public static void AddConflict(Dictionary<string, Dictionary<string, bool>> conflicts, string v, string w)
		{
			if (string.Compare(v, w, StringComparison.OrdinalIgnoreCase) > 0)
			{
				(w, v) = (v, w);
			}

			if (!conflicts.TryGetValue(v, out Dictionary<string, bool> conflictsV))
			{
				conflictsV = new Dictionary<string, bool>();
				conflicts.Add(v, conflictsV);
			}

			if (!conflictsV.ContainsKey(w))
			{
				conflictsV.Add(w, true);
			}
			else conflictsV[w] = true;
		}

		public static Dictionary<string, Dictionary<string, bool>> FindType2Conflicts(Graph g, List<List<string>> layering)
		{
			var conflicts = new Dictionary<string, Dictionary<string, bool>>();

			void Scan(List<string> south, int southPos, int southEnd, int prevNorthBorder, int nextNorthBorder)
			{
				var range = Util.Range(southPos, southEnd);
				foreach (var i in range)
				{
					var v = south[i];

					if (g.GetNode(v).DummyType != DummyType.None)
					{
						foreach (var uNode in g.GetPredecessors(v))
						{
							if (uNode.DummyType != DummyType.None && (uNode.Order < prevNorthBorder || uNode.Order > nextNorthBorder))
							{
								AddConflict(conflicts, uNode.Id, v);
							}
						}
					}
				}
			}

			object visitLayer(List<string> north, List<string> south)
			{
				var prevNorthPos = -1;
				var nextNorthPos = 0;
				var southPos = 0;

				for (int southLookahead = 0; southLookahead < south.Count; southLookahead++)
				{
					var v = south[southLookahead];
					if (g.GetNode(v).DummyType == DummyType.Border)
					{
						var predecessors = g.GetPredecessors(v);
						if (predecessors.Count > 0)
						{
							nextNorthPos = predecessors.First().Order;
							Scan(south, southPos, southLookahead, prevNorthPos, nextNorthPos);
							southPos = southLookahead;
							prevNorthPos = nextNorthPos;
						}
					}

					Scan(south, southPos, south.Count, nextNorthPos, north.Count);
				}

				return south;
			}

			if (layering.Count > 1)
			{
				var northList = layering[0];
				for (int i = 1; i < layering.Count; i++)
				{
					var layer = layering[i];
					visitLayer(northList, layer);
					northList = layer;
				}
			}

			return conflicts;
		}

		/*
		* Try to align nodes into vertical "blocks" where possible. This algorithm
		* attempts to align a node with one of its median neighbors. If the edge
		* connecting a neighbor is a type-1 conflict then we ignore that possibility.
		* If a previous node has already formed a block with a node after the node
		* we're trying to form a block with, we also ignore that possibility - our
		* blocks would be split in that scenario.
		*/
		public static VerticalAlignment VerticalAlign(List<List<string>> layering, Dictionary<string, Dictionary<string, bool>> conflicts, Func<string, ICollection<Node>> neighborFn)
		{
			var root = new Dictionary<string, string>();
			var align = new Dictionary<string, string>();
			var pos = new Dictionary<string, int>();

			// We cache the position here based on the layering because the graph and
			// layering may be out of sync. The layering matrix is manipulated to
			// generate different extreme alignments.
			foreach (var layer in layering)
			{
				for (var order = 0; order < layer.Count; order++)
				{
					var v = layer[order];
					root[v] = v;
					align[v] = v;
					pos[v] = order;
				}
			}

			foreach (var layer in layering)
			{
				var prevIdx = -1;
				foreach (var v in layer)
				{
					var ws = neighborFn(v).ToList();
					if (ws.Count > 0)
					{
						ws.Sort(Comparer<Node>.Create((a, b) => pos[a.Id] - pos[b.Id]));
						double mp = (double)(ws.Count - 1) / 2;
						var il = Math.Ceiling(mp);
						for (int i = (int)Math.Floor(mp); i <= il; ++i)
						{
							var w = ws[i];
							if (align[v] == v && prevIdx < pos[w.Id] && !HasConflict(conflicts, v, w.Id))
							{
								align[w.Id] = v;
								align[v] = root[v] = root[w.Id];
								prevIdx = pos[w.Id];
							}
						}
					}
				}
			}

			return new VerticalAlignment { Root = root, Align = align };
		}

		public static bool HasConflict(Dictionary<string, Dictionary<string, bool>> conflicts, string v, string w)
		{
			if (string.Compare(v, w, StringComparison.OrdinalIgnoreCase) > 0)
			{
				(w, v) = (v, w);
			}

			return conflicts.ContainsKey(v) && conflicts[v].ContainsKey(w);
		}

		public static Dictionary<string, double> HorizontalCompaction(Graph g, List<List<string>> layering, Dictionary<string, string> root, Dictionary<string, string> align, bool reverseSep = false)
		{
			// This portion of the algorithm differs from BK due to a number of problems.
			// Instead of their algorithm we construct a new block graph and do two
			// sweeps. The first sweep places blocks with the smallest possible
			// coordinates. The second sweep removes unused space by moving blocks to the
			// greatest coordinates without violating separation.
			var xs = new Dictionary<string, double>();
			var blockG = BuildBlockGraph(g, layering, root, reverseSep);
			var borderType = reverseSep ? "borderLeft" : "borderRight";

			void Iterate(Action<Node> setXsFunc, Func<string, ICollection<Node>> nextNodesFunc)
			{
				var stack = new Stack<Node>(blockG.Nodes);
				var elem = stack.Pop();
				var visited = new HashSet<string>();

				while (elem != null)
				{
					if (visited.Contains(elem.Id))
					{
						setXsFunc(elem);
					}
					else
					{
						stack.Push(elem);
						visited.Add(elem.Id);

						foreach (var item in nextNodesFunc(elem.Id))
						{
							stack.Push(item);
						}
					}

					elem = stack.Count > 0 ? stack.Pop() : null;
				}
			}

			// First pass, assign smallest coordinates
			void pass1(Node node)
			{
				var acc = double.MinValue;

				foreach (var item in blockG.GetInEdges(node.Id))
				{
					var value = xs[item.From] + item.NodeX;

					if (value > acc)
					{
						acc = value;
					}
				}

				if (acc != double.MinValue)
				{
					xs[node.Id] = acc;
				}
				else
				{
					xs[node.Id] = 0;
				}
			}

			// Second pass, assign greatest coordinates
			void pass2(Node node)
			{
				var min = double.MaxValue;

				foreach (var item in blockG.GetOutEdges(node.Id))
				{
					var value = xs[item.To] - item.NodeX;

					if (value < min)
					{
						min = value;
					}
				}

				if (min != double.MaxValue && node.BorderType != borderType)
				{
					xs[node.Id] = min;
				}
			}

			Iterate(pass1, blockG.GetPredecessors);
			Iterate(pass2, blockG.GetSuccessors);

			// Assign x coordinates to all nodes
			foreach (var v in align.Keys)
			{
				xs[v] = xs[root[v]];
			}

			return xs;
		}

		private static Graph BuildBlockGraph(Graph g, List<List<string>> layering, Dictionary<string, string> root, bool reverseSep)
		{
			var blockGraph = new Graph();
			var graphLabel = g.Options;
			var sepFn = Sep(graphLabel.NodeSeparation, graphLabel.EdgeSeparation, reverseSep);

			foreach (var layer in layering)
			{
				string u = null;

				foreach (var v in layer)
				{
					var vRoot = root[v];
					blockGraph.SetNode(new Node(vRoot));

					if (!string.IsNullOrEmpty(u))
					{
						var uRoot = root[u];
						var prevMaxEdge = blockGraph.FindEdge(uRoot, vRoot);
						var prevMax = prevMaxEdge == null ? 0 : prevMaxEdge.NodeX;
						var value = Math.Max(sepFn(g, v, u), prevMax);

						blockGraph.SetEdge(uRoot, vRoot, null, e => { e.NodeX = value; });
					}

					u = v;
				}
			}

			return blockGraph;
		}

		private static Func<Graph, string, string, double> Sep(int nodeSep, int edgeSep, bool reverseSep)
		{
			return (g, v, w) => {
				var vNode = g.GetNode(v);
				var wNode = g.GetNode(w);
				var sum = 0.0;
				var delta = 0.0;

				sum += vNode.Width / 2;
				if (vNode.LabelPosition != LabelPosition.None)
				{
					switch (vNode.LabelPosition)
					{
						case LabelPosition.Left:
							delta = -vNode.Width / 2;
							break;
						case LabelPosition.Right:
							delta = vNode.Width / 2;
							break;
						default:
							break;
					}
				}

				if (delta != 0)
				{
					sum += reverseSep ? delta : -delta;
				}
				delta = 0;

				sum += (double)(vNode.DummyType != DummyType.None ? edgeSep : nodeSep) / 2;
				sum += (double)(wNode.DummyType != DummyType.None ? edgeSep : nodeSep) / 2;

				sum += wNode.Width / 2;
				if (wNode.LabelPosition != LabelPosition.None)
				{
					switch (wNode.LabelPosition)
					{
						case LabelPosition.Left:
							delta = wNode.Width / 2;
							break;
						case LabelPosition.Right:
							delta = -wNode.Width / 2;
							break;
						default:
							break;
					}
				}
				if (delta != 0)
				{
					sum += reverseSep ? delta : -delta;
				}
				delta = 0;

				return sum;
			};
		}

		private struct SmallestWidthAlignment
		{
			public int Min { get; set; }

			public List<int> Xs { get; set; }
		}

		/*
		* Returns the alignment that has the smallest width of the given alignments.
		*/
		public static Dictionary<string, double> FindSmallestWidthAlignment(Graph g, Dictionary<GraphAlignment, Dictionary<string, double>> xss)
		{
			var currentMin = double.MaxValue;
			var currentXs = new Dictionary<string, double>();

			foreach (var xs in xss)
			{
				var max = double.MinValue;
				var min = double.MaxValue;
				var x = 0;

				foreach (var item in xs.Value)
				{
					var halfWidth = GetWidth(g, item.Key) / 2;
					max = Math.Max(item.Value + halfWidth, max);
					min = Math.Min(item.Value - halfWidth, min);
					x++;
				}

				var newMin = max - min;
				if (newMin < currentMin)
				{
					currentMin = newMin;
					currentXs = xs.Value;
				}
			}

			return currentXs;
		}

		private static double GetWidth(Graph g, string v)
		{
			return g.GetNode(v).Width;
		}

		/*
		* Align the coordinates of each of the layout alignments such that
		* left-biased alignments have their minimum coordinate at the same point as
		* the minimum coordinate of the smallest width alignment and right-biased
		* alignments have their maximum coordinate at the same point as the maximum
		* coordinate of the smallest width alignment.
		*/
		public static void AlignCoordinates(Dictionary<GraphAlignment, Dictionary<string, double>> xss, Dictionary<string, double> alignTo)
		{
			var alignToVals = alignTo.Values;
			var alignToMin = alignToVals.Count > 0 ? alignToVals.Min() : int.MinValue;
			var alignToMax = alignToVals.Count > 0 ? alignToVals.Max() : int.MaxValue;

			foreach (GraphAlignment alignment in Enum.GetValues(typeof(GraphAlignment)))
			{
				if (alignment == GraphAlignment.None)
				{
					continue;
				}

				var xs = xss[alignment];

				if (xs == alignTo)
				{
					continue;
				}

				var xsVals = xs.Values;
				var delta = alignToMin - xsVals.Min();

				if (alignment == GraphAlignment.UpRight || alignment == GraphAlignment.DownRight)
				{
					delta = alignToMax - xsVals.Max();
				}

				if (delta != 0)
				{
					xss[alignment] = Util.MapValues(xs, x => x + delta);
				}
			}
		}

		public static Dictionary<string, double> Balance(Dictionary<GraphAlignment, Dictionary<string, double>> xss, GraphAlignment align = GraphAlignment.None)
		{
			if (!xss.TryGetValue(GraphAlignment.UpLeft, out Dictionary<string, double> target))
			{
				throw new InvalidOperationException("Cannot find ul in xss!");
			}

			var result = new Dictionary<string, double>();

			foreach (var kvp in target)
			{
				if (align == GraphAlignment.None)
				{
					var xs = xss.Values.Select(x => x[kvp.Key]).ToList();
					xs.Sort();
					result[kvp.Key] = (xs[1] + xs[2]) / 2;
				}
				else
				{
					result[kvp.Key] = xss[align][kvp.Key];
				}
			}

			return result;
		}
	}
}
