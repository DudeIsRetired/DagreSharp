using DagreSharp.GraphLibrary;
using DagreSharp.Order;

namespace DagreSharp.Test.Order
{
	public class CrossCounterTest
	{
		private readonly Graph g = new() { ConfigureDefaultEdge = e => e.Weight = 1 };

		[Fact]
		public void Returns0ForAnEmptyLayering()
		{
			Assert.Equal(0, CrossCounter.CrossCount(g, []));
		}

		[Fact]
		public void Returns0ForALayeringWithNoCrossings()
		{
			g.SetEdge("a1", "b1");
			g.SetEdge("a2", "b2");

			Assert.Equal(0, CrossCounter.CrossCount(g, [["a1", "a2"], ["b1", "b2"]]));
		}

		[Fact]
		public void Returns1ForALayeringWith1Crossing()
		{
			g.SetEdge("a1", "b1");
			g.SetEdge("a2", "b2");

			Assert.Equal(1, CrossCounter.CrossCount(g, [["a1", "a2"], ["b2", "b1"]]));
		}

		[Fact]
		public void ReturnsAWeightedCrossingCountForALayeringWith1Crossing()
		{
			g.SetEdge("a1", "b1", null, e => { e.Weight = 2; });
			g.SetEdge("a2", "b2", null, e => { e.Weight = 3; });

			Assert.Equal(6, CrossCounter.CrossCount(g, [["a1", "a2"], ["b2", "b1"]]));
		}

		[Fact]
		public void CalculatesCrossingsAcrossLayers()
		{
			g.SetPath(["a1", "b1", "c1"]);
			g.SetPath(["a2", "b2", "c2"]);

			Assert.Equal(2, CrossCounter.CrossCount(g, [["a1", "a2"], ["b2", "b1"], ["c1", "c2"]]));
		}

		[Fact]
		public void WorksForGraph1()
		{
			g.SetPath(["a", "b", "c"]);
			g.SetPath(["d", "e", "c"]);
			g.SetPath(["a", "f", "i"]);
			g.SetEdge("a", "e");

			Assert.Equal(1, CrossCounter.CrossCount(g, [["a", "d"], ["b", "e", "f"], ["c", "i"]]));
			Assert.Equal(0, CrossCounter.CrossCount(g, [["d", "a"], ["e", "b", "f"], ["c", "i"]]));
		}
	}
}
