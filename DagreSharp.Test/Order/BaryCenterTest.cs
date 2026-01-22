using DagreSharp.GraphLibrary;
using DagreSharp.Order;

namespace DagreSharp.Test.Order
{
	public class BaryCenterTest
	{
		private readonly Graph g = new() { ConfigureDefaultEdge = e => e.Weight = 1 };

		[Fact]
		public void AssignsAnUndefinedBaryCenterForANodeWithNoPredecessors()
		{
			g.SetNode("x");

			var results = BaryCenter.Create(g, ["x"]);

			Assert.Single(results);
			Assert.Equal("x", results[0].Id);
		}

		[Fact]
		public void AssignsThePositionOfTheSolePredecessors()
		{
			g.SetNode("a", n => { n.Order = 2; });
			g.SetEdge("a", "x");

			var results = BaryCenter.Create(g, ["x"]);

			Assert.Single(results);
			Assert.Equal("x", results[0].Id);
			Assert.Equal(2, results[0].BaryCenter);
			Assert.Equal(1, results[0].Weight);
		}

		[Fact]
		public void AssignsTheAverageOfMultiplePredecessors()
		{
			g.SetNode("a", n => { n.Order = 2; });
			g.SetNode("b", n => { n.Order = 4; });
			g.SetEdge("a", "x");
			g.SetEdge("b", "x");

			var results = BaryCenter.Create(g, ["x"]);

			Assert.Single(results);
			Assert.Equal("x", results[0].Id);
			Assert.Equal(3, results[0].BaryCenter);
			Assert.Equal(2, results[0].Weight);
		}

		[Fact]
		public void TakesIntoAccountTheWeightOfEdges()
		{
			g.SetNode("a", n => { n.Order = 2; });
			g.SetNode("b", n => { n.Order = 4; });
			g.SetEdge("a", "x", null, e => { e.Weight = 3; });
			g.SetEdge("b", "x");

			var results = BaryCenter.Create(g, ["x"]);

			Assert.Single(results);
			Assert.Equal("x", results[0].Id);
			Assert.Equal(2.5, results[0].BaryCenter);
			Assert.Equal(4, results[0].Weight);
		}

		[Fact]
		public void CalculatesBaryCentersForAllNodesInTheMovableLayer()
		{
			g.SetNode("a", n => { n.Order = 1; });
			g.SetNode("b", n => { n.Order = 2; });
			g.SetNode("c", n => { n.Order = 4; });
			g.SetEdge("a", "x");
			g.SetEdge("b", "x");
			g.SetNode("y");
			g.SetEdge("a", "z", null, e => { e.Weight = 2; });
			g.SetEdge("c", "z");

			var results = BaryCenter.Create(g, ["x", "y", "z"]).OrderBy(bc => bc.Id).ToList();

			Assert.Equal(3, results.Count);

			Assert.Equal("x", results[0].Id);
			Assert.Equal(1.5, results[0].BaryCenter);
			Assert.Equal(2, results[0].Weight);

			Assert.Equal("y", results[1].Id);

			Assert.Equal("z", results[2].Id);
			Assert.Equal(2, results[2].BaryCenter);
			Assert.Equal(3, results[2].Weight);
		}

	}
}
