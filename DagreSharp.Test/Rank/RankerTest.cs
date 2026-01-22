using DagreSharp.GraphLibrary;
using DagreSharp.Rank;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace DagreSharp.Test.Rank
{
	public class RankerTest
	{
		private readonly Graph _graph;

		public RankerTest()
		{
			_graph = new Graph()
			{
				ConfigureDefaultNode = null,
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};
			_graph.SetPath(["a", "b", "c", "d", "h"]);
			_graph.SetPath(["a", "e", "g", "h"]);
			_graph.SetPath(["a", "f", "g"]);
		}

		[Fact]
		public void RespectsTheMinlenAttribute()
		{
			foreach (GraphRank ranker in Enum.GetValues(typeof(GraphRank)))
			{
				_graph.Options.Ranker = ranker;
				Ranker.Rank(_graph);

				foreach (var e in _graph.Edges)
				{
					var vRank = _graph.GetNode(e.From).Rank;
					var wRank = _graph.GetNode(e.To).Rank;

					Assert.True(wRank - vRank >= e.MinLength);
				}
			}
		}

		[Fact]
		public void CanRankASingleNodeGraph()
		{
			foreach (GraphRank ranker in Enum.GetValues(typeof(GraphRank)))
			{
				var g = new Graph();
				g.Options.Ranker = ranker;
				g.SetNode("a");
				Ranker.Rank(g);

				Assert.Equal(0, g.GetNode("a").Rank);
			}
		}

		[Fact]
		public void LongestPathCanAssignARankToASingleNodeGraph()
		{
			var g = new Graph() { ConfigureDefaultEdge = e => { e.MinLength = 1; } };
			g.SetNode("a");

			Ranker.LongestPath(g);
			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
		}

		[Fact]
		public void LongestPathCanAssignRanksToUnconnectedNodes()
		{
			var g = new Graph() { ConfigureDefaultEdge = e => { e.MinLength = 1; } };
			g.SetNode("a");
			g.SetNode("b");

			Ranker.LongestPath(g);
			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(0, g.GetNode("b").Rank);
		}

		[Fact]
		public void LongestPathCanAssignRanksToConnectedNodes()
		{
			var g = new Graph() { ConfigureDefaultEdge = e => { e.MinLength = 1; } };
			g.SetEdge("a", "b");

			Ranker.LongestPath(g);
			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
		}

		[Fact]
		public void LongestPathCanAssignRanksForADiamond()
		{
			var g = new Graph() { ConfigureDefaultEdge = e => { e.MinLength = 1; } };
			g.SetPath(["a", "b", "d"]);
			g.SetPath(["a", "c", "d"]);

			Ranker.LongestPath(g);
			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
			Assert.Equal(1, g.GetNode("c").Rank);
			Assert.Equal(2, g.GetNode("d").Rank);
		}

		[Fact]
		public void LongestPathUsesTheMinlenAttributeOnTheEdge()
		{
			var g = new Graph() { ConfigureDefaultEdge = e => { e.MinLength = 1; } };
			g.SetPath(["a", "b", "d"]);
			g.SetEdge("a", "c");
			g.SetEdge("c", "d", null, e => { e.MinLength = 2; });

			Ranker.LongestPath(g);
			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			// longest path biases towards the lowest rank it can assign
			Assert.Equal(2, g.GetNode("b").Rank);
			Assert.Equal(1, g.GetNode("c").Rank);
			Assert.Equal(3, g.GetNode("d").Rank);
		}

	}
}
