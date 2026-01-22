using DagreSharp.GraphLibrary;
using System.Xml.Linq;

namespace DagreSharp.Test
{
	public class PositionTest
	{
		private readonly Graph g = new(true, false, true);

		public PositionTest()
		{
			g.Options.RankSeparation = 50;
			g.Options.NodeSeparation = 50;
			g.Options.EdgeSeparation = 10;
		}

		[Fact]
		public void RespectsRanksep()
		{
			g.Options.RankSeparation = 1000;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Width = 50; n.Height = 80; n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "b");

			Dagre.Position(g);

			Assert.Equal(100+1000+80/2, g.GetNode("b").Y);
		}

		[Fact]
		public void UseTheLargestHeightInEachRankWithRanksep()
		{
			g.Options.RankSeparation = 1000;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Width = 50; n.Height = 80; n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Width = 50; n.Height = 90; n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "c");

			Dagre.Position(g);

			Assert.Equal(100/2, g.GetNode("a").Y);
			Assert.Equal(100/2, g.GetNode("b").Y);    // Note we used 100 and not 80 here
			Assert.Equal(100+1000+90/2, g.GetNode("c").Y);
		}

		[Fact]
		public void RespectsNodesep()
		{
			g.Options.NodeSeparation = 1000;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Width = 70; n.Height = 80; n.Rank = 0; n.Order = 1; });

			Dagre.Position(g);

			Assert.Equal(g.GetNode("a").X+50/2+1000+70/2, g.GetNode("b").X);
		}

		[Fact]
		public void ShouldNotTryToPositionTheSubgraphNodeItself()
		{
			g.SetNode("a", n => { n.Width = 50; n.Height = 50; n.Rank = 0; n.Order = 0; });
			g.SetNode("sg1");
			g.SetParent("a", "sg1");

			Dagre.Position(g);

			var sg1Node = g.GetNode("sg1");
			Assert.Equal(0, sg1Node.X);
			Assert.Equal(0, sg1Node.Y);
		}
	}
}
