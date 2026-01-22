using DagreSharp.GraphLibrary;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DagreSharp.Test
{
	public class GreedyFASTest
	{
		private static int WeightFunc(Edge e) => e.Weight;

		[Fact]
		public void ReturnsTheEmptySetForEmptyGraphs()
		{
			var g = new Graph();
			Assert.Empty(GreedyFAS.Run(g));
		}

		[Fact]
		public void ReturnsTheEmptySetForSingleNodeGraphs()
		{
			var g = new Graph();
			g.SetNode("a");
			Assert.Empty(GreedyFAS.Run(g));
		}

		[Fact]
		public void ReturnsAnEmptySetIfTheInputGraphIsAcyclic()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");
			g.SetEdge("b", "d");
			g.SetEdge("a", "e");
			Assert.Empty(GreedyFAS.Run(g));
		}

		[Fact]
		public void ReturnsASingleEdgeWithASimpleCycle()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "a");
			CheckFAS(g, GreedyFAS.Run(g));
		}

		[Fact]
		public void ReturnsASingleEdgeInA4NodeCycle()
		{
			var g = new Graph();
			g.SetEdge("n1", "n2");
			g.SetPath(["n2", "n3", "n4", "n5", "n2"]);
			g.SetEdge("n3", "n5");
			g.SetEdge("n4", "n2");
			g.SetEdge("n4", "n6");
			CheckFAS(g, GreedyFAS.Run(g));
		}

		[Fact]
		public void ReturnsTwoEdgesForTwo4NodeCycles()
		{
			var g = new Graph();
			g.SetEdge("n1", "n2");
			g.SetPath(["n2", "n3", "n4", "n5", "n2"]);
			g.SetEdge("n3", "n5");
			g.SetEdge("n4", "n2");
			g.SetEdge("n4", "n6");
			g.SetPath(["n6", "n7", "n8", "n9", "n6"]);
			g.SetEdge("n7", "n9");
			g.SetEdge("n8", "n6");
			g.SetEdge("n8", "n10");
			CheckFAS(g, GreedyFAS.Run(g));
		}

		[Fact]
		public void WorksWithArbitrarilyWeightedEdges()
		{
			// Our algorithm should also work for graphs with multi-edges, a graph
			// where more than one edge can be pointing in the same direction between
			// the same pair of incident nodes. We try this by assigning weights to
			// our edges representing the number of edges from one node to the other.
			
			var g1 = new Graph();
			g1.SetEdge("n1", "n2", null, e => { e.Weight = 2; });
			g1.SetEdge("n2", "n1", null, e => { e.Weight = 1; });
			var result = GreedyFAS.Run(g1, WeightFunc);
			Assert.Single(result);
			Assert.Contains(result, e => e.From == "n2" && e.To == "n1");

			var g2 = new Graph();
			g1.SetEdge("n1", "n2", null, e => { e.Weight = 1; });
			g1.SetEdge("n2", "n1", null, e => { e.Weight = 2; });
			result = GreedyFAS.Run(g1, WeightFunc);
			Assert.Single(result);
			Assert.Contains(result, e => e.From == "n1" && e.To == "n2");
		}

		[Fact]
		public void WorksForMultigraphs()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", "foo", e => { e.Weight = 5; });
			g.SetEdge("b", "a", "bar", e => { e.Weight = 2; });
			g.SetEdge("b", "a", "baz", e => { e.Weight = 2; });

			var result = GreedyFAS.Run(g, WeightFunc);

			Assert.Equal(2, result.Count);
			Assert.Contains(result, e => e.From == "b" && e.To == "a" && e.Name == "bar");
			Assert.Contains(result, e => e.From == "b" && e.To == "a" && e.Name == "baz");
		}

		private static void CheckFAS(Graph g, List<Edge> fas)
		{
			var n = g.Nodes.Count;
			var m = g.Edges.Count;

			foreach (var edge in fas)
			{
				g.RemoveEdge(edge.From, edge.To);
			}

			Assert.Empty(Algorithm.FindCycles(g));

			// The more direct m/2 - n/6 fails for the simple cycle A <-> B, where one
			// edge must be reversed, but the performance bound implies that only 2/3rds
			// of an edge can be reversed. I'm using floors to acount for this.
			Assert.True(fas.Count <= Math.Floor((double)m / 2) - Math.Floor((double)n / 6));
		}
	}
}
