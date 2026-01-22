using DagreSharp.GraphLibrary;
using DagreSharp.Order;

namespace DagreSharp.Test.Order
{
	public class AddSubgraphConstraintsTest
	{
		private readonly Graph g = new(true, false, true);
		private readonly Graph cg = new();

		[Fact]
		public void DoesNotChangeCGForAFlatSetOfNodes()
		{
			var vs = new List<string>(["a", "b", "c", "d"]);
			foreach (var v in vs)
			{
				g.SetNode(v);
			}

			HeuristicOrder.AddSubgraphConstraints(g, cg, vs);

			Assert.Empty(cg.Nodes);
			Assert.Empty(cg.Edges);
		}

		[Fact]
		public void DoesntCreateAConstraintForContiguousSubgraphNodes()
		{
			var vs = new List<string>(["a", "b", "c"]);
			foreach (var v in vs)
			{
				g.SetParent(v, "sg");
			}

			HeuristicOrder.AddSubgraphConstraints(g, cg, vs);

			Assert.Empty(cg.Nodes);
			Assert.Empty(cg.Edges);
		}

		[Fact]
		public void AddsAConstraintWhenTheParentsForAdjacentNodesAreDifferent()
		{
			var vs = new List<string>(["a", "b"]);
			g.SetParent("a", "sg1");
			g.SetParent("b", "sg2");

			HeuristicOrder.AddSubgraphConstraints(g, cg, vs);

			Assert.Single(cg.Edges);
			var edge = cg.Edges.First();
			Assert.Equal("sg1", edge.From);
			Assert.Equal("sg2", edge.To);
		}

		[Fact]
		public void WorksForMultipleLevels()
		{
			var vs = new List<string>(["a", "b", "c", "d", "e", "f", "g", "h"]);
			
			foreach (var v in vs)
			{
				g.SetNode(v);
			}

			g.SetParent("b", "sg2");
			g.SetParent("sg2", "sg1");
			g.SetParent("c", "sg1");
			g.SetParent("d", "sg3");
			g.SetParent("sg3", "sg1");
			g.SetParent("f", "sg4");
			g.SetParent("g", "sg5");
			g.SetParent("sg5", "sg4");

			HeuristicOrder.AddSubgraphConstraints(g, cg, vs);

			Assert.Equal(2, cg.Edges.Count);
			var edges = cg.Edges.OrderBy(e => e.From).ToList();
			var edge1 = edges[0];
			var edge2 = edges[1];

			Assert.Equal("sg1", edge1.From);
			Assert.Equal("sg4", edge1.To);
			Assert.Equal("sg2", edge2.From);
			Assert.Equal("sg3", edge2.To);
		}
	}
}
