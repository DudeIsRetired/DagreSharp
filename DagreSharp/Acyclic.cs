using DagreSharp.GraphLibrary;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp
{
	public class Acyclic
	{
		public static void Run(Graph g)
		{
			var fas = g.Options.Acyclicer == Acyclicer.Greedy
				? GreedyFAS.Run(g, e => e.Weight)
				: DepthFirstSearchFAS(g);

			foreach (var edge in fas) {
				g.RemoveEdge(edge);
				var e = g.SetEdge(edge.To, edge.From, Util.UniqueId("rev"));
				e.CopyFrom(edge);
				e.ForwardName = edge.Name;
				e.IsReversed = true;
			}
		}

		private static List<Edge> DepthFirstSearchFAS(Graph g)
		{
			var fas = new List<Edge>();
			var stack = new HashSet<string>();
			var visited = new HashSet<string>();

			void dfs(Node node)
			{
				if (visited.Contains(node.Id))
				{
					return;
				}

				visited.Add(node.Id);
				stack.Add(node.Id);

				foreach(var e in g.GetOutEdges(node.Id))
				{
					if (stack.Contains(e.To))
					{
						fas.Add(e);
					}
					else
					{
						var wNode = g.GetNode(e.To);
						dfs(wNode);
					}
				}

				stack.Remove(node.Id);
			}

			foreach (var node in g.Nodes)
			{
				dfs(node);
			}

			return fas;
		}

		public static void Undo(Graph g)
		{
			foreach (var edge in g.Edges.ToList())
			{
				if (edge.IsReversed)
				{
					g.RemoveEdge(edge.From, edge.To, edge.Name);

					var forwardName = edge.ForwardName;
					edge.IsReversed = false;
					edge.ForwardName = string.Empty;
					var e = g.SetEdge(edge.To, edge.From);//, null, e => { e.CopyFrom(edge); });
					e.CopyFrom(edge);
				}
			}
		}
	}
}
