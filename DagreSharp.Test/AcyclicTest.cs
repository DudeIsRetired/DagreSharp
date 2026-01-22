using DagreSharp.GraphLibrary;
using System.Collections.Generic;

namespace DagreSharp.Test
{
	public class AcyclicTest
	{
		[Fact]
		public void DoesNotChangeAnAlreadyAcyclicGraph()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			g.SetPath(["a", "b", "d"]);
			g.SetPath(["a", "c", "d"]);
			Acyclic.Run(g);

			Assert.Equal(4, g.Edges.Count);
			Assert.Contains(g.Edges, e => e.From == "a" && e.To == "b");
			Assert.Contains(g.Edges, e => e.From == "a" && e.To == "c");
			Assert.Contains(g.Edges, e => e.From == "b" && e.To == "d");
			Assert.Contains(g.Edges, e => e.From == "c" && e.To == "d");
		}

		[Fact]
		public void BreaksCyclesInTheInputGraph()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			g.SetPath(["a", "b", "c", "d", "a"]);
			Acyclic.Run(g);
			Assert.Empty(Algorithm.FindCycles(g));
		}

		[Fact]
		public void CreatesAMultiEdgeWhereNecessary()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			g.SetPath(["a", "b", "a"]);
			Acyclic.Run(g);

			Assert.Empty(Algorithm.FindCycles(g));

			if (g.HasEdge("a", "b"))
			{
				Assert.Equal(2, g.GetOutEdges("a", "b").Count);
			}
			else
			{
				Assert.Equal(2, g.GetOutEdges("b", "a").Count);
			}

			Assert.Equal(2, g.Edges.Count);
		}

		[Fact]
		public void UndoDoesNotChangeEdgesWhereTheOriginalGraphWasAcyclic()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			g.SetEdge("a", "b", null, e => { e.MinLength = 2; e.Weight = 3; });
			Acyclic.Run(g);
			Acyclic.Undo(g);

			var edge = g.GetEdge("a", "b");
			Assert.Equal(2, edge.MinLength);
			Assert.Equal(3, edge.Weight);
			Assert.Single(g.Edges);
		}

		[Fact]
		public void UndoCanRestorePreviosulyReversedEdges()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			g.SetEdge("a", "b", null, e => { e.MinLength = 2; e.Weight = 3; });
			g.SetEdge("b", "a", null, e => { e.MinLength = 3; e.Weight = 4; });
			
			Acyclic.Run(g);
			Acyclic.Undo(g);

			var edge = g.GetEdge("a", "b");
			Assert.Equal(2, edge.MinLength);
			Assert.Equal(3, edge.Weight);

			edge = g.GetEdge("b", "a");
			Assert.Equal(3, edge.MinLength);
			Assert.Equal(4, edge.Weight);

			Assert.Equal(2, g.Edges.Count);
		}

		[Fact]
		public void GreedySpecificFunctionalityPrefersToBreakCyclesAtLowWeightEdges()
		{
			var g = new Graph(true, true)
			{
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 2; },
			};
			g.Options.Acyclicer = Acyclicer.Greedy;

			g.SetPath(["a", "b", "c", "d", "a"]);
			g.SetEdge("c", "d", null, e => { e.Weight = 1; });
			
			Acyclic.Run(g);

			Assert.Empty(Algorithm.FindCycles(g));
			Assert.False(g.HasEdge("c", "d"));
		}
	}
}
