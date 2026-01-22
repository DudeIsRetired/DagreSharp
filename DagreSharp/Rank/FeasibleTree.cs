using DagreSharp.GraphLibrary;
using System.Linq;

namespace DagreSharp.Rank
{
	public static class FeasibleTree
	{
		/*
		* Constructs a spanning tree with tight edges and adjusted the input node's
		* ranks to achieve this. A tight edge is one that is has a length that matches
		* its "minlen" attribute.
		*
		* The basic structure for this function is derived from Gansner, et al., "A
		* Technique for Drawing Directed Graphs."
		*
		* Pre-conditions:
		*
		*    1. Graph must be a DAG.
		*    2. Graph must be connected.
		*    3. Graph must have at least one node.
		*    5. Graph nodes must have been previously assigned a "rank" property that
		*       respects the "minlen" property of incident edges.
		*    6. Graph edges must have a "minlen" property.
		*
		* Post-conditions:
		*
		*    - Graph nodes will have their rank adjusted to ensure that all edges are
		*      tight.
		*
		* Returns a tree (undirected graph) that is constructed using only "tight"
		* edges.
		*/
		public static Graph Run(Graph g)
		{
			var t = new Graph(false);

			// Choose arbitrary node from which to start our tree
			var start = g.Nodes.First();
			var size = g.Nodes.Count;
			t.SetNode(start.Copy());

			var delta = 0;
			while (TightTree(t, g) < size)
			{
				var edge = FindMinSlackEdge(t, g);
				
				if (edge != null)
				{
					delta = t.HasNode(edge.From) ? Ranker.Slack(g, edge) : -Ranker.Slack(g, edge);
				}

				ShiftRanks(t, g, delta);
			}

			return t;
		}

		/*
		* Finds a maximal tree of tight edges and returns the number of nodes in the
		* tree.
		*/
		private static int TightTree(Graph t, Graph g)
		{
			void dfs(string v)
			{
				var nodeEdges = g.GetAllEdges(v);
				foreach (var e in nodeEdges)
				{
					var edgeV = e.From;
					var w = (v == edgeV) ? e.To : edgeV;

					if (!t.HasNode(w) && Ranker.Slack(g, e) == 0)
					{
						t.SetNode(w);
						t.SetEdge(v, w, null);
						dfs(w);
					}
				}
			}

			var nodes = t.Nodes.ToList();
			foreach (var node in nodes)
			{
				dfs(node.Id);
			}

			return t.Nodes.Count;
		}

		/*
		* Finds the edge with the smallest slack that is incident on tree and returns
		* it.
		*/
		private static Edge FindMinSlackEdge(Graph t, Graph g)
		{
			var minSlack = int.MaxValue;
			Edge minEdge = null;

			foreach (var edge in g.Edges)
			{
				var edgeSlack = int.MaxValue;

				if (t.HasNode(edge.From) != t.HasNode(edge.To))
				{
					edgeSlack = Ranker.Slack(g, edge);
				}

				if (edgeSlack < minSlack)
				{
					minSlack = edgeSlack;
					minEdge = edge;
				}
			}

			return minEdge;
		}

		private static void ShiftRanks(Graph t, Graph g, int delta)
		{
			foreach (var node in t.Nodes)
			{
				g.GetNode(node.Id).Rank += delta;
			}
		}
	}
}
