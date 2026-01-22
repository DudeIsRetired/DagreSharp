using DagreSharp.GraphLibrary;
using DagreSharp.Rank;

namespace DagreSharp.Test.Rank
{
	public class FeasibleTreeTest
	{
		[Fact]
		public void CreatesATreeForATrivialInputGraph()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetEdge("a", "b", null, e => { e.MinLength = 1; });

			var tree = FeasibleTree.Run(g);

			Assert.Equal(g.GetNode("a").Rank + 1, g.GetNode("b").Rank);

			var neighborsA = tree.GetNeighbors("a");
			Assert.Single(neighborsA);
			Assert.Equal("b", neighborsA.First().Id);
		}

		[Fact]
		public void CorrectlyShortensSlackByPullingANodeUp()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetNode("d", n => { n.Rank = 2; });
			g.SetPath(["a", "b", "c"], e => { e.MinLength = 1; });
			g.SetEdge("a", "d", null, e => { e.MinLength = 1; });

			var tree = FeasibleTree.Run(g);

			Assert.Equal(g.GetNode("a").Rank + 1, g.GetNode("b").Rank);
			Assert.Equal(g.GetNode("b").Rank + 1, g.GetNode("c").Rank);
			Assert.Equal(g.GetNode("a").Rank + 1, g.GetNode("d").Rank);

			var neighborsA = tree.GetNeighbors("a").ToList();
			Assert.Equal(2, neighborsA.Count);
			Assert.Contains(neighborsA, n => n.Id == "b");
			Assert.Contains(neighborsA, n => n.Id == "d");

			var neighborsB = tree.GetNeighbors("b").ToList();
			Assert.Equal(2, neighborsB.Count);
			Assert.Contains(neighborsB, n => n.Id == "a");
			Assert.Contains(neighborsB, n => n.Id == "c");

			var neighborsC = tree.GetNeighbors("c").ToList();
			Assert.Single(neighborsC);
			Assert.Contains(neighborsC, n => n.Id == "b");

			var neighborsD = tree.GetNeighbors("d").ToList();
			Assert.Single(neighborsD);
			Assert.Contains(neighborsD, n => n.Id == "a");
		}

		[Fact]
		public void CorrectlyShortensSlackByPullingANodeDown()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = 2; });
			g.SetNode("b", n => { n.Rank = 0; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetEdge("b", "a", null, e => { e.MinLength = 1; });
			g.SetEdge("b", "c", null, e => { e.MinLength = 1; });

			var tree = FeasibleTree.Run(g);

			Assert.Equal(g.GetNode("b").Rank + 1, g.GetNode("a").Rank);
			Assert.Equal(g.GetNode("b").Rank + 1, g.GetNode("c").Rank);

			var neighborsA = tree.GetNeighbors("a").ToList();
			Assert.Single(neighborsA);
			Assert.Contains(neighborsA, n => n.Id == "b");

			var neighborsB = tree.GetNeighbors("b").ToList();
			Assert.Equal(2, neighborsB.Count);
			Assert.Contains(neighborsB, n => n.Id == "a");
			Assert.Contains(neighborsB, n => n.Id == "c");

			var neighborsC = tree.GetNeighbors("c").ToList();
			Assert.Single(neighborsC);
			Assert.Contains(neighborsC, n => n.Id == "b");
		}
	}
}
