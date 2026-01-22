using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class TarjanTest
	{
		[Fact]
		public void ReturnsAnEmptyArrayForAnEmptyGraph()
		{
			var result = Algorithm.Tarjan(new Graph());
			Assert.Empty(result);
		}

		[Fact]
		public void ReturnsSingletonsForNodesNotInAStronglyConnectedComponent()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"]);
			g.SetEdge("d", "c");

			var result = Algorithm.Tarjan(g);

			Assert.Equal(4, result.Count);

			foreach (var subList in result)
			{
				Assert.Single(subList);
			}

			var flattened = result.SelectMany(p => p).ToList();
			Assert.Contains(flattened, s => s == "a");
			Assert.Contains(flattened, s => s == "b");
			Assert.Contains(flattened, s => s == "c");
			Assert.Contains(flattened, s => s == "d");
		}

		[Fact]
		public void ReturnsASingleComponentForACycleOf1Edge()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "a"]);

			var result = Algorithm.Tarjan(g);
			Assert.Single(result);
			Assert.Contains(result[0], s => s == "a");
			Assert.Contains(result[0], s => s == "b");
		}

		[Fact]
		public void ReturnsASingleComponentForATriangle()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c", "a"]);

			var result = Algorithm.Tarjan(g);

			Assert.Single(result);
			Assert.Contains(result[0], s => s == "a");
			Assert.Contains(result[0], s => s == "b");
			Assert.Contains(result[0], s => s == "c");
		}

		[Fact]
		public void CanFindMultipleComponents()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "a"]);
			g.SetPath(["c", "d", "e", "c"]);
			g.SetNode("f");

			var result = Algorithm.Tarjan(g);

			Assert.Equal(3, result.Count);

			for (int i = 0; i < result.Count; i++)
			{
				var subList = result[i];
				var found = true;

				if (subList.Count == 1)
				{
					Assert.Contains(subList, s => s == "f");
				}
				else if (subList.Count == 2)
				{
					Assert.Contains(subList, s => s == "a");
					Assert.Contains(subList, s => s == "b");
				}
				else if (subList.Count == 3)
				{
					Assert.Contains(subList, s => s == "c");
					Assert.Contains(subList, s => s == "d");
					Assert.Contains(subList, s => s == "e");
				}
				else
				{
					found = false;
				}

				Assert.True(found);
			}
		}
	}
}
