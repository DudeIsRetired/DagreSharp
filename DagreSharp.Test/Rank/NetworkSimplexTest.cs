using DagreSharp.GraphLibrary;
using DagreSharp.Rank;

namespace DagreSharp.Test.Rank
{
	public class NetworkSimplexTest
	{
		private Graph g;
		private Graph t;
		private readonly Graph gansnerGraph;
		private readonly Graph gansnerTree;

		public NetworkSimplexTest()
		{
			g = new Graph(true, true)
			{
				ConfigureDefaultNode = null,
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			};

			t = new Graph(false)
			{
				ConfigureDefaultNode = null,
				ConfigureDefaultEdge = null
			};

			gansnerGraph = new Graph()
			{
				ConfigureDefaultNode = null,
				ConfigureDefaultEdge = e => { e.MinLength = 1; e.Weight = 1; }
			}
			.SetPath(["a", "b", "c", "d", "h"])
			.SetPath(["a", "e", "g", "h"])
			.SetPath(["a", "f", "g"]);

			gansnerTree = new Graph(false)
			{
				ConfigureDefaultNode = null,
				ConfigureDefaultEdge = null
			}
			.SetPath(["a", "b", "c", "d", "h", "g", "e"]);
			gansnerTree.SetEdge("g", "f");
		}

		[Fact]
		public void CanAssignARankToASingleNode()
		{
			g.SetNode("a");

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
		}

		[Fact]
		public void CanAssignARankToA2NodeConnectedGraph()
		{
			g.SetEdge("a", "b");

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
		}

		[Fact]
		public void CanAssignRanksForADiamond()
		{
			g.SetPath(["a", "b", "d"]);
			g.SetPath(["a", "c", "d"]);

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
			Assert.Equal(1, g.GetNode("c").Rank);
			Assert.Equal(2, g.GetNode("d").Rank);
		}

		[Fact]
		public void UsesTheMinlenAttributeOnTheEdge()
		{
			g.SetPath(["a", "b", "d"]);
			g.SetEdge("a", "c");
			g.SetEdge("c", "d", null, e => { e.MinLength = 2; });

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			// longest path biases towards the lowest rank it can assign. Since the
			// graph has no optimization opportunities we can assume that the longest
			// path ranking is used.
			Assert.Equal(2, g.GetNode("b").Rank);
			Assert.Equal(1, g.GetNode("c").Rank);
			Assert.Equal(3, g.GetNode("d").Rank);
		}

		[Fact]
		public void CanRankTheGansnerGraph()
		{
			g = gansnerGraph;

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
			Assert.Equal(2, g.GetNode("c").Rank);
			Assert.Equal(3, g.GetNode("d").Rank);
			Assert.Equal(4, g.GetNode("h").Rank);
			Assert.Equal(1, g.GetNode("e").Rank);
			Assert.Equal(1, g.GetNode("f").Rank);
			Assert.Equal(2, g.GetNode("g").Rank);
		}

		[Fact]
		public void CanHandleMultiEdges()
		{
			g.SetPath(["a", "b", "c", "d"]);
			g.SetEdge("a", "e", null, e => { e.Weight = 2; e.MinLength = 1; });
			g.SetEdge("e", "d");
			g.SetEdge("b", "c", "multi", e => { e.Weight = 1; e.MinLength = 2; });

			Ns(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
			// b -> c has minlen = 1 and minlen = 2, so it should be 2 ranks apart.
			Assert.Equal(3, g.GetNode("c").Rank);
			Assert.Equal(4, g.GetNode("d").Rank);
			Assert.Equal(1, g.GetNode("e").Rank);
		}

		[Fact]
		public void LeaveEdgeReturnsUndefinedIfThereIsNoEdgeWithANegativeCutvalue()
		{
			var tree = new Graph(false);
			tree.SetEdge("a", "b", null, e => { e.CutValue = 1; });
			tree.SetEdge("b", "c", null, e => { e.CutValue = 1; });

			Assert.Null(NetworkSimplex.LeaveEdge(tree));
		}

		[Fact]
		public void LeaveEdgeReturnsAnEdgeIfOneIsFoundWithANegativeCutvalue()
		{
			var tree = new Graph(false);
			tree.SetEdge("a", "b", null, e => { e.CutValue = 1; });
			tree.SetEdge("b", "c", null, e => { e.CutValue = -1; });

			var result = NetworkSimplex.LeaveEdge(tree);
			
			Assert.Equal("b", result.From);
			Assert.Equal("c", result.To);
		}

		[Fact]
		public void EnterEdgeFindsAnEdgeFromTheHeadToTailComponent()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			var cNode = g.SetNode("c", n => { n.Rank = 3; }) as Node;
			g.SetPath(["a", "b", "c"]);
			g.SetEdge("a", "c");
			t.SetPath(["b", "c", "a"]);

			NetworkSimplex.InitLowLimValues(t, cNode);
			var f = NetworkSimplex.EnterEdge(t, g, t.GetEdge("b", "c") as Edge);

			Assert.True((f.From == "a" && f.To == "b") || (f.From == "b" && f.To == "a"));
		}

		[Fact]
		public void EnterEdgeWorksWhenTheRootOfTheTreeIsInTheTailComponent()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			var bNode = g.SetNode("b", n => { n.Rank = 2; }) as Node;
			g.SetNode("c", n => { n.Rank = 3; });
			g.SetPath(["a", "b", "c"]);
			g.SetEdge("a", "c");
			t.SetPath(["b", "c", "a"]);

			NetworkSimplex.InitLowLimValues(t, bNode);
			var f = NetworkSimplex.EnterEdge(t, g, g.GetEdge("b", "c") as Edge);

			Assert.True((f.From == "a" && f.To == "b") || (f.From == "b" && f.To == "a"));
		}

		[Fact]
		public void EnterEdgeFindsTheEdgeWithTheLeastSlack()
		{
			var aNode = g.SetNode("a", n => { n.Rank = 0; }) as Node;
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 3; });
			g.SetNode("d", n => { n.Rank = 4; });
			g.SetEdge("a", "d");
			g.SetPath(["a", "c", "d"]);
			g.SetEdge("b", "c");
			t.SetPath(["c", "d", "a", "b"]);

			NetworkSimplex.InitLowLimValues(t, aNode);
			var f = NetworkSimplex.EnterEdge(t, g, g.GetEdge("c", "d") as Edge);

			Assert.True((f.From == "b" && f.To == "c") || (f.From == "c" && f.To == "b"));
		}

		[Fact]
		public void EnterEdgeFindsAnAppropriateEdgeForGansnerGraph1()
		{
			g = gansnerGraph;
			t = gansnerTree;
			
			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t, g.GetNode("a") as Node);
			var f = NetworkSimplex.EnterEdge(t, g, g.GetEdge("g", "h") as Edge);

			Assert.True(f.From == "a" || f.To == "a");
			Assert.True((f.From == "e" || f.To == "e") || (f.From == "f" || f.To == "f"));
			//expect(["e", "f"]).toEqual(expect.arrayContaining([undirectedEdge(f).w]));
		}

		[Fact]
		public void EnterEdgeFindsAnAppropriateEdgeForGansnerGraph2()
		{
			g = gansnerGraph;
			t = gansnerTree;

			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t, g.GetNode("e") as Node);
			var f = NetworkSimplex.EnterEdge(t, g, g.GetEdge("g", "h") as Edge);

			Assert.True(f.From == "a" || f.To == "a");
			Assert.True((f.From == "e" || f.To == "e") || (f.From == "f" || f.To == "f"));
		}

		[Fact]
		public void EnterEdgeFindsAnAppropriateEdgeForGansnerGraph3()
		{
			g = gansnerGraph;
			t = gansnerTree;

			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t, g.GetNode("a") as Node);
			var f = NetworkSimplex.EnterEdge(t, g, t.GetEdge("h", "g") as Edge);

			Assert.True(f.From == "a" || f.To == "a");
			Assert.True((f.From == "e" || f.To == "e") || (f.From == "f" || f.To == "f"));
		}

		[Fact]
		public void EnterEdgeFindsAnAppropriateEdgeForGansnerGraph4()
		{
			g = gansnerGraph;
			t = gansnerTree;

			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t, g.GetNode("e") as Node);
			var f = NetworkSimplex.EnterEdge(t, g, t.GetEdge("h", "g") as Edge);

			Assert.True(f.From == "a" || f.To == "a");
			Assert.True((f.From == "e" || f.To == "e") || (f.From == "f" || f.To == "f"));
		}

		[Fact]
		public void InitLowLimValuesAssignsLowLimAndParentForEachNodeInATree()
		{
			g = new Graph { ConfigureDefaultNode = null };
			g.SetNodes(["a", "b", "c", "d", "e"]);
			g.SetPath(["a", "b", "a", "c", "d", "c", "e"]);

			NetworkSimplex.InitLowLimValues(g, g.GetNode("a") as Node);

			var a = g.GetNode("a");
			var b = g.GetNode("b");
			var c = g.GetNode("c");
			var d = g.GetNode("d");
			var e = g.GetNode("e");

			var limValues = g.Nodes.Select(n => n.Lim).Order().ToArray();
			Assert.Equal([1, 2, 3, 4, 5], limValues);
			Assert.True(a.Low == 1 && a.Lim == 5);

			Assert.Equal("a", b.Parent.Id);
			Assert.True(b.Lim < a.Lim);

			Assert.Equal("a", c.Parent.Id);
			Assert.True(c.Lim < a.Lim);
			Assert.True(c.Lim != b.Lim);

			Assert.Equal("c", d.Parent.Id);
			Assert.True(d.Lim < c.Lim);

			Assert.Equal("c", e.Parent.Id);
			Assert.True(e.Lim < c.Lim);
			Assert.True(e.Lim != d.Lim);
		}

		[Fact]
		public void ExchangesEdgesAndUpdatesCutValuesAndLowLimNumbers()
		{
			g = gansnerGraph;
			t = gansnerTree;

			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t);
			NetworkSimplex.ExchangeEdges(t, g, new Edge("g", "h"), new Edge("a", "e"));

			// check new cut values
			Assert.Equal(2, t.GetEdge("a", "b").CutValue);
			Assert.Equal(2, t.GetEdge("b", "c").CutValue);
			Assert.Equal(2, t.GetEdge("c", "d").CutValue);
			Assert.Equal(2, t.GetEdge("d", "h").CutValue);
			Assert.Equal(1, t.GetEdge("a", "e").CutValue);
			Assert.Equal(1, t.GetEdge("e", "g").CutValue);
			Assert.Equal(0, t.GetEdge("g", "f").CutValue);

			// ensure lim numbers look right
			var limValues = t.Nodes.Select(n => n.Lim).Order().ToArray();
			Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], limValues);
		}

		[Fact]
		public void ExchangeEdgesUpdatesRanks()
		{
			g = gansnerGraph;
			t = gansnerTree;

			Ranker.LongestPath(g);
			NetworkSimplex.InitLowLimValues(t);
			NetworkSimplex.ExchangeEdges(t, g, new Edge("g", "h"), new Edge("a", "e"));
			Util.NormalizeRanks(g);

			// check new ranks
			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
			Assert.Equal(2, g.GetNode("c").Rank);
			Assert.Equal(3, g.GetNode("d").Rank);
			Assert.Equal(1, g.GetNode("e").Rank);
			Assert.Equal(1, g.GetNode("f").Rank);
			Assert.Equal(2, g.GetNode("g").Rank);
			Assert.Equal(4, g.GetNode("h").Rank);
		}

		[Fact]
		public void CalcCutValueWorksForA2NodeTreeWithCTowardsP()
		{
			g.SetPath(["c", "p"]);
			t.SetPath(["p", "c"]);

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(1, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksForA2NodeTreeWithCAgainstP()
		{
			g.SetPath(["p", "c"]);
			t.SetPath(["p", "c"]);

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(1, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor3NodeTreeWithGCTowardsCTowardsP()
		{
			g.SetPath(["gc", "c", "p"]);
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("p", "c");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(3, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor3NodeTreeWithGCTowardsCAgainstP()
		{
			g.SetEdge("p", "c");
			g.SetEdge("gc", "c");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("p", "c");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-1, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor3NodeTreeWithGCAgainstCTowardsP()
		{
			g.SetEdge("c", "p");
			g.SetEdge("c", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("p", "c");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-1, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor3NodeTreeWithGCAgainstCAgainstP()
		{
			g.SetPath(["p", "c", "gc"]);
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("p", "c");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(3, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithGCTowardsCTowardsPTowardsOWithOTowardsC()
		{
			g.SetEdge("o", "c", null, e => { e.Weight = 7; });
			g.SetPath(["gc", "c", "p", "o"]);
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetPath(["c", "p", "o"]);

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-4, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithGCTowardsCTowardsPTowardsOWithOAgainstC()
		{
			g.SetEdge("c", "o", null, e => { e.Weight = 7; });
			g.SetPath(["gc", "c", "p", "o"]);
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetPath(["c", "p", "o"]);

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(10, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithOTowardsGCTowardsCTowardsPWithOTowardsC()
		{
			g.SetEdge("o", "c", null, e => { e.Weight = 7; });
			g.SetPath(["o", "gc", "c", "p"]);
			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-4, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithOTowardsGCTowardsCTowardsPWithOAgainstC()
		{
			g.SetEdge("c", "o", null, e => { e.Weight = 7; });
			g.SetPath(["o", "gc", "c", "p"]);
			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(10, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithGCTowardsCAgainstPTowardsOWithOTowardsC()
		{
			g.SetEdge("gc", "c");
			g.SetEdge("p", "c");
			g.SetEdge("p", "o");
			g.SetEdge("o", "c", null, e => { e.Weight = 7; });
			
			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(6, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithGCTowardsCAgainstPTowardsOWithOAgainstC()
		{
			g.SetEdge("gc", "c");
			g.SetEdge("p", "c");
			g.SetEdge("p", "o");
			g.SetEdge("c", "o", null, e => { e.Weight = 7; });

			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-8, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithOTowardsGCTowardsCAgainstPWithOTowardsC()
		{
			g.SetEdge("o", "c", null, e => { e.Weight = 7; });
			g.SetPath(["o", "gc", "c"]);
			g.SetEdge("p", "c");

			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(6, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void CalcCutValueWorksFor4NodeTreeWithOTowardsGCTowardsCAgainstPWithOAgainstC()
		{
			g.SetEdge("c", "o", null, e => { e.Weight = 7; });
			g.SetPath(["o", "gc", "c"]);
			g.SetEdge("p", "c");

			t.SetEdge("o", "gc");
			t.SetEdge("gc", "c", null, e => { e.CutValue = 3; });
			t.SetEdge("c", "p");

			NetworkSimplex.InitLowLimValues(t, g.GetNode("p") as Node);

			Assert.Equal(-8, NetworkSimplex.CalcCutValue(t, g, "c"));
		}

		[Fact]
		public void InitCutValuesWorksForGansnerGraph()
		{
			NetworkSimplex.InitLowLimValues(gansnerTree);
			NetworkSimplex.InitCutValues(gansnerTree, gansnerGraph);

			Assert.Equal(3, gansnerTree.GetEdge("a", "b").CutValue);
			Assert.Equal(3, gansnerTree.GetEdge("b", "c").CutValue);
			Assert.Equal(3, gansnerTree.GetEdge("c", "d").CutValue);
			Assert.Equal(3, gansnerTree.GetEdge("d", "h").CutValue);
			Assert.Equal(-1, gansnerTree.GetEdge("g", "h").CutValue);
			Assert.Equal(0, gansnerTree.GetEdge("e", "g").CutValue);
			Assert.Equal(0, gansnerTree.GetEdge("f", "g").CutValue);
		}

		[Fact]
		public void InitCutValuesWorksForUpdatedGansnerGraph()
		{
			gansnerTree.RemoveEdge("g", "h");
			gansnerTree.SetEdge("a", "e");

			NetworkSimplex.InitLowLimValues(gansnerTree);
			NetworkSimplex.InitCutValues(gansnerTree, gansnerGraph);

			Assert.Equal(2, gansnerTree.GetEdge("a", "b").CutValue);
			Assert.Equal(2, gansnerTree.GetEdge("b", "c").CutValue);
			Assert.Equal(2, gansnerTree.GetEdge("c", "d").CutValue);
			Assert.Equal(2, gansnerTree.GetEdge("d", "h").CutValue);
			Assert.Equal(1, gansnerTree.GetEdge("a", "e").CutValue);
			Assert.Equal(1, gansnerTree.GetEdge("e", "g").CutValue);
			Assert.Equal(0, gansnerTree.GetEdge("f", "g").CutValue);
		}

		private static void Ns(Graph g)
		{
			NetworkSimplex.Run(g);
			Util.NormalizeRanks(g);
		}
	}
}
