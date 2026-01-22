using DagreSharp.GraphLibrary;
using DagreSharp.Order;
using System;

namespace DagreSharp.Test.Order
{
	public class HeuristicOrderTest
	{
		private readonly Graph g = new(true, false, true) { ConfigureDefaultEdge = e => e.Weight = 1 };

		[Fact]
		public void DoesNotAddCrossingsToATreeStructure()
		{
			g.SetNode("a", n => { n.Rank = 1; });

			foreach (var v in new[] { "b", "e" })
			{
				g.SetNode(v, n => { n.Rank = 2; });
			}

			foreach (var v in new[] { "c", "d", "f" })
			{
				g.SetNode(v, n => { n.Rank = 3; });
			}

			g.SetPath(["a", "b", "c"]);
			g.SetEdge("b", "d");
			g.SetPath(["a", "e", "f"]);

			HeuristicOrder.Run(g);

			var layering = Util.BuildLayerMatrix(g);
			Assert.Equal(0, CrossCounter.CrossCount(g, layering));
		}

		[Fact]
		public void CanSolveASimpleGraph()
		{
			// This graph resulted in a single crossing for previous versions of dagre.
			foreach (var v in new[] { "a", "d" })
			{
				g.SetNode(v, n => { n.Rank = 1; });
			}

			foreach (var v in new[] { "b", "f", "e" })
			{
				g.SetNode(v, n => { n.Rank = 2; });
			}

			foreach (var v in new[] { "c", "g" })
			{
				g.SetNode(v, n => { n.Rank = 3; });
			}

			HeuristicOrder.Run(g);

			var layering = Util.BuildLayerMatrix(g);
			Assert.Equal(0, CrossCounter.CrossCount(g, layering));
		}

		[Fact]
		public void CanMinimizeCrossings()
		{
			g.SetNode("a", n => { n.Rank = 1; });

			foreach (var v in new[] { "b", "e", "g" })
			{
				g.SetNode(v, n => { n.Rank = 2; });
			}

			foreach (var v in new[] { "c", "f", "h" })
			{
				g.SetNode(v, n => { n.Rank = 3; });
			}

			g.SetNode("d", n => { n.Rank = 4; });

			HeuristicOrder.Run(g);

			var layering = Util.BuildLayerMatrix(g);
			Assert.True(CrossCounter.CrossCount(g, layering) <= 1);
		}
	}
}
