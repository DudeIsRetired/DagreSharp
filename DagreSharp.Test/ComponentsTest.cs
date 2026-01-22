using DagreSharp.GraphLibrary;
using System.Xml.Linq;

namespace DagreSharp.Test
{
	public class ComponentsTest
	{
		[Fact]
		public void ReturnsAnEmptyListForAnEmptyGraph()
		{
			var g = new Graph(false);
			Assert.Empty(Algorithm.Components(g));
		}

		[Fact]
		public void ReturnsSingletonListsForUnconnectedNodes()
		{
			var g = new Graph(false);
			g.SetNode("a");
			g.SetNode("b");

			var result = Algorithm.Components(g);

			Assert.Equal(2, result.Count);

			var flattened = result.SelectMany(s => s).ToList();
			Assert.Equal(2, flattened.Count);
			Assert.Contains(flattened, s => s == "a");
			Assert.Contains(flattened, s => s == "b");
		}

		[Fact]
		public void ReturnsAListOfNodesInAComponent()
		{
			var g = new Graph(false);
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");

			var result = Algorithm.Components(g).SelectMany(s => s).ToList();

			Assert.Equal(3, result.Count);
			Assert.Contains(result, s => s == "a");
			Assert.Contains(result, s => s == "b");
			Assert.Contains(result, s => s == "c");
		}

		[Fact]
		public void ReturnsNodesConnectedByANeighborRelationshipInADigraph()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c", "a"]);
			g.SetEdge("d", "c");
			g.SetEdge("e", "f");

			var result = Algorithm.Components(g);
			Assert.Equal(2, result.Count);

			var first = result[0];
			var second = result[1];

			if (first.Count == 4)
			{
				Assert.Contains(first, s => s == "a");
				Assert.Contains(first, s => s == "b");
				Assert.Contains(first, s => s == "c");
				Assert.Contains(first, s => s == "d");

				Assert.Contains(second, s => s == "e");
				Assert.Contains(second, s => s == "f");
			}
			else
			{
				Assert.Contains(second, s => s == "a");
				Assert.Contains(second, s => s == "b");
				Assert.Contains(second, s => s == "c");
				Assert.Contains(second, s => s == "d");

				Assert.Contains(first, s => s == "e");
				Assert.Contains(first, s => s == "f");
			}
		}
	}
}
