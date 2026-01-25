using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.Rank
{
	public class NetworkSimplex
	{
		/*
		* The network simplex algorithm assigns ranks to each node in the input graph
		* and iteratively improves the ranking to reduce the length of edges.
		*
		* Preconditions:
		*
		*    1. The input graph must be a DAG.
		*    2. All nodes in the graph must have an object value.
		*    3. All edges in the graph must have "minlen" and "weight" attributes.
		*
		* Postconditions:
		*
		*    1. All nodes in the graph will have an assigned "rank" attribute that has
		*       been optimized by the network simplex algorithm. Ranks start at 0.
		*
		*
		* A rough sketch of the algorithm is as follows:
		*
		*    1. Assign initial ranks to each node. We use the longest path algorithm,
		*       which assigns ranks to the lowest position possible. In general this
		*       leads to very wide bottom ranks and unnecessarily long edges.
		*    2. Construct a feasible tight tree. A tight tree is one such that all
		*       edges in the tree have no slack (difference between length of edge
		*       and minlen for the edge). This by itself greatly improves the assigned
		*       rankings by shorting edges.
		*    3. Iteratively find edges that have negative cut values. Generally a
		*       negative cut value indicates that the edge could be removed and a new
		*       tree edge could be added to produce a more compact graph.
		*
		* Much of the algorithms here are derived from Gansner, et al., "A Technique
		* for Drawing Directed Graphs." The structure of the file roughly follows the
		* structure of the overall algorithm.
		*/
		public static void Run(Graph g)
		{
			g = Util.Simplify(g);
			Ranker.LongestPath(g);
			var t = FeasibleTree.Run(g);
			InitLowLimValues(t);
			InitCutValues(t, g);

			var leaveEdge = LeaveEdge(t);

			while (leaveEdge != null)
			{
				var enterEdge = EnterEdge(t, g, leaveEdge);
				ExchangeEdges(t, g, leaveEdge, enterEdge);
				leaveEdge = LeaveEdge(t);
			}
		}

		public static void InitLowLimValues(Graph tree, Node root = null)
		{
			if (root == null)
			{
				root = tree.Nodes.First();
			}
			
			DfsAssignLowLim(tree, new HashSet<string>(), 1, root);
		}

		private static int DfsAssignLowLim(Graph tree, HashSet<string> visited, int nextLim, Node node, Node parent = null)
		{
			var low = nextLim;
			var targetNode = tree.GetNode(node.Id);
			visited.Add(targetNode.Id);

			foreach (var w in tree.GetNeighbors(targetNode.Id))
			{
				if (!visited.Contains(w.Id))
				{
					nextLim = DfsAssignLowLim(tree, visited, nextLim, w, targetNode);
				}
			}

			targetNode.Low = low;
			targetNode.Lim = nextLim++;
			targetNode.Parent = parent;

			return nextLim;
		}

		/*
		* Initializes cut values for all edges in the tree.
		*/
		public static void InitCutValues(Graph t, Graph g)
		{
			var vs = Algorithm.PostOrder(t, t.Nodes.ToList());
			vs = vs.GetRange(0, vs.Count - 1);

			foreach (var v in vs)
			{
				AssignCutValue(t, g, v);
			}
		}

		private static void AssignCutValue(Graph t, Graph g, string child)
		{
			var childLab = t.GetNode(child);
			var parent = childLab.Parent;
			
			if (parent != null)
			{
				t.GetEdge(child, parent.Id).CutValue = CalcCutValue(t, g, child);
			}
		}

		/*
		* Given the tight tree, its graph, and a child in the graph calculate and
		* return the cut value for the edge between the child and its parent.
		*/
		public static int CalcCutValue(Graph t, Graph g, string child)
		{
			var childLab = t.GetNode(child);
			var parent = childLab.Parent ?? throw new InvalidOperationException("Child has no parent!");

			// True if the child is on the tail end of the edge in the directed graph
			var childIsTail = true;
			// The graph's view of the tree edge we're inspecting
			var graphEdge = g.FindEdge(child, parent.Id);

			if (graphEdge == null)
			{
				childIsTail = false;
				graphEdge = g.GetEdge(parent.Id, child);
			}

			// The accumulated cut value for the edge between this node and its parent
			int cutValue = graphEdge.Weight;
			foreach (var edge in g.GetAllEdges(child))
			{
				var isOutEdge = edge.From == child;
				var other = isOutEdge ? edge.To : edge.From;

				if (other != parent.Id)
				{
					var pointsToHead = isOutEdge == childIsTail;
					var otherWeight = edge.Weight;

					cutValue += pointsToHead ? otherWeight : -otherWeight;
					if (IsTreeEdge(t, child, other))
					{
						var otherCutValue = t.GetEdge(child, other).CutValue;
						cutValue += pointsToHead ? -otherCutValue : otherCutValue;
					}
				}
			}

			return cutValue;
		}

		/*
		* Returns true if the edge is in the tree.
		*/
		private static bool IsTreeEdge(Graph tree, string u, string v)
		{
			return tree.HasEdge(u, v);
		}

		public static Edge LeaveEdge(Graph tree)
		{
			return tree.Edges.FirstOrDefault(e => e.CutValue < 0);
		}

		public static Edge EnterEdge(Graph t, Graph g, Edge edge)
		{
			var v = edge.From;
			var w = edge.To;

			// For the rest of this function we assume that v is the tail and w is the
			// head, so if we don't have this edge in the graph we should flip it to
			// match the correct orientation.
			if (!g.HasEdge(v, w))
			{
				v = edge.To;
				w = edge.From;
			}

			var vLabel = t.GetNode(v);
			var wLabel = t.GetNode(w);
			var tailLabel = vLabel;
			var flip = false;

			// If the root is in the tail of the edge then we need to flip the logic that
			// checks for the head and tail nodes in the candidates function below.
			if (vLabel.Lim > wLabel.Lim)
			{
				tailLabel = wLabel;
				flip = true;
			}

			var candidates = g.Edges.Where(e => flip == IsDescendant(t.GetNode(e.From), tailLabel) && flip != IsDescendant(t.GetNode(e.To), tailLabel));
			Edge enterEdge = null;

			foreach (var candidate in candidates)
			{
				if (enterEdge == null)
				{
					enterEdge = candidate;
					continue;
				}

				if (Ranker.Slack(g, candidate) < Ranker.Slack(g, enterEdge))
				{
					enterEdge = candidate;
				}
			}

			return enterEdge;
		}

		/*
		* Returns true if the specified node is descendant of the root node per the
		* assigned low and lim attributes in the tree.
		*/
		private static bool IsDescendant(Node node, Node rootNode)
		{
			return rootNode.Low <= node.Lim && node.Lim <= rootNode.Lim;
		}

		public static void ExchangeEdges(Graph t, Graph g, Edge leaveEdge, Edge enterEdge)
		{
			var v = leaveEdge.From;
			var w = leaveEdge.To;
			t.RemoveEdge(v, w);

			if (enterEdge != null)
			{
				t.SetEdge(enterEdge.From, enterEdge.To);
			}

			InitLowLimValues(t);
			InitCutValues(t, g);
			UpdateRanks(t, g);
		}

		private static void UpdateRanks(Graph t, Graph g)
		{
			var root = t.Nodes.First(n => g.GetNode(n.Id).Parent == null);
			var vs = Algorithm.PreOrder(t, new[] { root });
			vs = vs.GetRange(1, vs.Count - 1);

			foreach (var v in vs)
			{
				var parent = t.GetNode(v).Parent ?? throw new InvalidOperationException("Cannot find parent from original Graph!");
				parent = g.GetNode(parent.Id);	// Note! parent from Graph g!! Had to dig for this bug
				var edge = g.FindEdge(v, parent.Id);
				var flipped = false;

				if (edge == null)
				{
					edge = g.GetEdge(parent.Id, v);
					flipped = true;
				}

				g.GetNode(v).Rank = parent.Rank + (flipped ? edge.MinLength : -edge.MinLength);
			}
		}
	}
}
