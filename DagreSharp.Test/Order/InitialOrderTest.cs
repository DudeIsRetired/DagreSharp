using DagreSharp.GraphLibrary;
using DagreSharp.Order;

namespace DagreSharp.Test.Order
{
	public class InitialOrderTest
	{
		private readonly Graph g = new(true, false, true) { ConfigureDefaultEdge = e => e.Weight = 1 };

		[Fact]
		public void AssignsNonOverlappingOrdersForEachRankInATree()
		{
			var a = g.SetNode("a", n => { n.Rank = 0; });
			var b = g.SetNode("b", n => { n.Rank = 1; });
			var c = g.SetNode("c", n => { n.Rank = 2; });
			var d = g.SetNode("d", n => { n.Rank = 2; });
			var e = g.SetNode("e", n => { n.Rank = 1; });

			g.SetPath(["a", "b", "c"]);
			g.SetEdge("b", "d");
			g.SetEdge("a", "e");

			var layering = InitialOrder.Run(g);

			Assert.Equal([a], layering[0]);

			var layering1 = layering[1];
			layering1.Sort();
			Assert.Equal([b, e], layering1);

			var layering2 = layering[2];
			layering2.Sort();
			Assert.Equal([c, d], layering2);
		}

		[Fact]
		public void AssignsNonOverlappingOrdersForEachRankInADAG()
		{
			var a = g.SetNode("a", n => { n.Rank = 0; });
			var b = g.SetNode("b", n => { n.Rank = 1; });
			var c = g.SetNode("c", n => { n.Rank = 1; });
			var d = g.SetNode("d", n => { n.Rank = 2; });

			g.SetPath(["a", "b", "d"]);
			g.SetPath(["a", "c", "d"]);

			var layering = InitialOrder.Run(g);

			Assert.Equal([a], layering[0]);

			var layering1 = layering[1];
			layering1.Sort();
			Assert.Equal([b, c], layering1);

			Assert.Equal([d], layering[2]);
		}

		[Fact]
		public void DoesNotAssignAnOrderToSubgraphNodes()
		{
			var a = g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("sg1");
			g.SetParent("a", "sg1");

			var layering = InitialOrder.Run(g);

			Assert.Equal([[a]], layering);
		}
	}
}
