using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class ParentDummyChainsTest
	{
		private readonly Graph g = new(true, false, true);

		[Fact]
		public void DoesNotSetAParentIfBothTheTailAndHeadHaveNoParent()
		{
			g.SetNode("a");
			g.SetNode("b");
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); }) as Node;
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "b"]);

			ParentDummyChains.Run(g);

			Assert.Null(g.FindParent("d1"));
		}

		[Fact]
		public void UsesTheTailsParentForTheFirstNodeIfItIsNotTheRoot()
		{
			g.SetParent("a", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 2; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 2; }) as Node;
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg1", g.FindParent("d1"));
		}

		[Fact]
		public void UsesTheHeadsParentForTheFirstNodeIfTailsIsRoot()
		{
			g.SetParent("b", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 1; n.MaxRank = 3; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 1; }) as Node;
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg1", g.FindParent("d1"));
		}

		[Fact]
		public void HandlesALongChainStartingInASubgraph()
		{
			g.SetParent("a", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 2; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 2; }) as Node;
			g.SetNode("d2", n => { n.Rank = 3; });
			g.SetNode("d3", n => { n.Rank = 4; });
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "d3", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg1", g.FindParent("d1"));
			Assert.Null(g.FindParent("d2"));
			Assert.Null(g.FindParent("d3"));
		}

		[Fact]
		public void HandlesALongChainEndingInASubgraph()
		{
			g.SetParent("b", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 3; n.MaxRank = 5; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 1; }) as Node;
			g.SetNode("d2", n => { n.Rank = 2; });
			g.SetNode("d3", n => { n.Rank = 3; });
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "d3", "b"]);

			ParentDummyChains.Run(g);

			
			Assert.Null(g.FindParent("d1"));
			Assert.Null(g.FindParent("d2"));
			Assert.Equal("sg1", g.FindParent("d3"));
		}

		[Fact]
		public void HandlesNestedSubgraphs()
		{
			g.SetParent("a", "sg2");
			g.SetParent("sg2", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 4; });
			g.SetNode("sg2", n => { n.MinRank = 1; n.MaxRank = 3; });
			g.SetParent("b", "sg4");
			g.SetParent("sg4", "sg3");
			g.SetNode("sg3", n => { n.MinRank = 6; n.MaxRank = 10; });
			g.SetNode("sg4", n => { n.MinRank = 7; n.MaxRank = 9; });

			for (var i = 0; i < 5; ++i)
			{
				g.SetNode("d" + (i + 1), n => { n.Rank = i + 3; });
			}

			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); }) as Node;
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "d3", "d4", "d5", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg2", g.FindParent("d1"));
			Assert.Equal("sg1", g.FindParent("d2"));
			Assert.Null(g.FindParent("d3"));
			Assert.Equal("sg3", g.FindParent("d4"));
			Assert.Equal("sg4", g.FindParent("d5"));
		}

		[Fact]
		public void HandlesOverlappingRankRanges()
		{
			g.SetParent("a", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 3; });
			g.SetParent("b", "sg2");
			g.SetNode("sg2", n => { n.MinRank = 2; n.MaxRank = 6; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 2; }) as Node;
			g.SetNode("d2", n => { n.Rank = 3; });
			g.SetNode("d3", n => { n.Rank = 4; });
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "d3", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg1", g.FindParent("d1"));
			Assert.Equal("sg1", g.FindParent("d2"));
			Assert.Equal("sg2", g.FindParent("d3"));
		}

		[Fact]
		public void HandlesAnLCAThatIsNotTheRootOfTheGraph1()
		{
			g.SetParent("a", "sg1");
			g.SetParent("sg2", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 6; });
			g.SetParent("b", "sg2");
			g.SetNode("sg2", n => { n.MinRank = 3; n.MaxRank = 5; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 2; }) as Node;
			g.SetNode("d2", n => { n.Rank = 3; });
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg1", g.FindParent("d1"));
			Assert.Equal("sg2", g.FindParent("d2"));
		}

		[Fact]
		public void HandlesAnLCAThatIsNotTheRootOfTheGraph2()
		{
			g.SetParent("a", "sg2");
			g.SetParent("sg2", "sg1");
			g.SetNode("sg1", n => { n.MinRank = 0; n.MaxRank = 6; });
			g.SetParent("b", "sg1");
			g.SetNode("sg2", n => { n.MinRank = 1; n.MaxRank = 3; });
			var nodeD1 = g.SetNode("d1", n => { n.DummyEdge = new Edge("a", "b"); n.Rank = 3; }) as Node;
			g.SetNode("d2", n => { n.Rank = 4; });
			g.Options.DummyChains.Add(nodeD1);
			g.SetPath(["a", "d1", "d2", "b"]);

			ParentDummyChains.Run(g);

			Assert.Equal("sg2", g.FindParent("d1"));
			Assert.Equal("sg1", g.FindParent("d2"));
		}
	}
}
