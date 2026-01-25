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

			var a1 = g.GetNode("a1");
			var a2 = g.GetNode("a2");
			var b1 = g.GetNode("b1");
			var b2 = g.GetNode("b2");

			Assert.Equal(0, CrossCounter.CrossCount(g, [[a1, a2], [b1, b2]]));
		}

		[Fact]
		public void Returns1ForALayeringWith1Crossing()
		{
			g.SetEdge("a1", "b1");
			g.SetEdge("a2", "b2");

			var a1 = g.GetNode("a1");
			var a2 = g.GetNode("a2");
			var b1 = g.GetNode("b1");
			var b2 = g.GetNode("b2");

			Assert.Equal(1, CrossCounter.CrossCount(g, [[a1, a2], [b2, b1]]));
		}

		[Fact]
		public void ReturnsAWeightedCrossingCountForALayeringWith1Crossing()
		{
			g.SetEdge("a1", "b1", null, e => { e.Weight = 2; });
			g.SetEdge("a2", "b2", null, e => { e.Weight = 3; });

			var a1 = g.GetNode("a1");
			var a2 = g.GetNode("a2");
			var b1 = g.GetNode("b1");
			var b2 = g.GetNode("b2");

			Assert.Equal(6, CrossCounter.CrossCount(g, [[a1, a2], [b2, b1]]));
		}

		[Fact]
		public void CalculatesCrossingsAcrossLayers()
		{
			g.SetPath(["a1", "b1", "c1"]);
			g.SetPath(["a2", "b2", "c2"]);

			var a1 = g.GetNode("a1");
			var a2 = g.GetNode("a2");
			var b1 = g.GetNode("b1");
			var b2 = g.GetNode("b2");
			var c1 = g.GetNode("c1");
			var c2 = g.GetNode("c2");

			Assert.Equal(2, CrossCounter.CrossCount(g, [[a1, a2], [b2, b1], [c1, c2]]));
		}

		[Fact]
		public void WorksForGraph1()
		{
			g.SetPath(["a", "b", "c"]);
			g.SetPath(["d", "e", "c"]);
			g.SetPath(["a", "f", "i"]);
			g.SetEdge("a", "e");

			var a = g.GetNode("a");
			var b = g.GetNode("b");
			var c = g.GetNode("c");
			var d = g.GetNode("d");
			var e = g.GetNode("e");
			var f = g.GetNode("f");
			var i = g.GetNode("i");

			Assert.Equal(1, CrossCounter.CrossCount(g, [[a, d], [b, e, f], [c, i]]));
			Assert.Equal(0, CrossCounter.CrossCount(g, [[d, a], [e, b, f], [c, i]]));
		}
	}
}
