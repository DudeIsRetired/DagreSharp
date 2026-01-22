using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class AddBorderSegmentsTest
	{
		private readonly Graph g = new(true, false, true);

		[Fact]
		public void DoesNotAddBorderNodesForANonCompoundGraph()
		{
			var graph = new Graph();
			graph.SetNode("a", n => { n.Rank = 0; });

			BorderSegments.Add(graph);

			Assert.Single(graph.Nodes);
			Assert.Equal(0, graph.GetNode("a").Rank);
		}

		[Fact]
		public void DoesNotAddBorderNodesForAGraphWithNoClusters()
		{
			g.SetNode("a", n => { n.Rank = 0; });

			BorderSegments.Add(g);

			Assert.Single(g.Nodes);
			Assert.Equal(0, g.GetNode("a").Rank);
		}

		[Fact]
		public void AddsABorderForASingleRankSubgraph()
		{
			g.SetNode("sg", n => { n.MinRank = 1; n.MaxRank = 1; });

			BorderSegments.Add(g);

			var bl = g.GetNode("sg").BorderLeft[1];
			var br = g.GetNode("sg").BorderRight[1];

			var blNode = g.GetNode(bl);
			Assert.Equal("borderLeft", blNode.BorderType);
			Assert.Equal(1, blNode.Rank);
			Assert.Equal(0, blNode.Width);
			Assert.Equal(0, blNode.Height);
			Assert.Equal("sg", g.FindParent(bl));

			var brNode = g.GetNode(br);
			Assert.Equal("borderRight", brNode.BorderType);
			Assert.Equal(1, brNode.Rank);
			Assert.Equal(0, brNode.Width);
			Assert.Equal(0, brNode.Height);
			Assert.Equal("sg", g.FindParent(br));
		}

		[Fact]
		public void AddsABorderForAMultiRankSubgraph()
		{
			g.SetNode("sg", n => { n.MinRank = 1; n.MaxRank = 2; });

			BorderSegments.Add(g);

			var sgNode = g.GetNode("sg");

			var bl2 = sgNode.BorderLeft[1];
			var br2 = sgNode.BorderRight[1];

			var bl2Node = g.GetNode(bl2);
			Assert.Equal("borderLeft", bl2Node.BorderType);
			Assert.Equal(1, bl2Node.Rank);
			Assert.Equal(0, bl2Node.Width);
			Assert.Equal(0, bl2Node.Height);
			Assert.Equal("sg", g.FindParent(bl2));

			var br2Node = g.GetNode(br2);
			Assert.Equal("borderRight", br2Node.BorderType);
			Assert.Equal(1, br2Node.Rank);
			Assert.Equal(0, br2Node.Width);
			Assert.Equal(0, br2Node.Height);
			Assert.Equal("sg", g.FindParent(br2));

			var bl1 = sgNode.BorderLeft[2];
			var br1 = sgNode.BorderRight[2];

			var bl1Node = g.GetNode(bl1);
			Assert.Equal("borderLeft", bl1Node.BorderType);
			Assert.Equal(2, bl1Node.Rank);
			Assert.Equal(0, bl1Node.Width);
			Assert.Equal(0, bl1Node.Height);
			Assert.Equal("sg", g.FindParent(bl1));

			var br1Node = g.GetNode(br1);
			Assert.Equal("borderRight", br1Node.BorderType);
			Assert.Equal(2, br1Node.Rank);
			Assert.Equal(0, br1Node.Width);
			Assert.Equal(0, br1Node.Height);
			Assert.Equal("sg", g.FindParent(br1));

			Assert.True(g.HasEdge(sgNode.BorderLeft[1], sgNode.BorderLeft[2]));
			Assert.True(g.HasEdge(sgNode.BorderRight[1], sgNode.BorderRight[2]));
		}

		[Fact]
		public void AddsBordersForNestedSubgraphs()
		{
			g.SetNode("sg1", n => { n.MinRank = 1; n.MaxRank = 1; });
			g.SetNode("sg2", n => { n.MinRank = 1; n.MaxRank = 1; });
			g.SetParent("sg2", "sg1");

			BorderSegments.Add(g);

			var bl1 = g.GetNode("sg1").BorderLeft[1];
			var br1 = g.GetNode("sg1").BorderRight[1];

			var bl1Node = g.GetNode(bl1);
			Assert.Equal("borderLeft", bl1Node.BorderType);
			Assert.Equal(1, bl1Node.Rank);
			Assert.Equal(0, bl1Node.Width);
			Assert.Equal(0, bl1Node.Height);
			Assert.Equal("sg1", g.FindParent(bl1));

			var br1Node = g.GetNode(br1);
			Assert.Equal("borderRight", br1Node.BorderType);
			Assert.Equal(1, br1Node.Rank);
			Assert.Equal(0, br1Node.Width);
			Assert.Equal(0, br1Node.Height);
			Assert.Equal("sg1", g.FindParent(br1));

			var bl2 = g.GetNode("sg2").BorderLeft[1];
			var br2 = g.GetNode("sg2").BorderRight[1];

			var bl2Node = g.GetNode(bl2);
			Assert.Equal("borderLeft", bl2Node.BorderType);
			Assert.Equal(1, bl2Node.Rank);
			Assert.Equal(0, bl2Node.Width);
			Assert.Equal(0, bl2Node.Height);
			Assert.Equal("sg2", g.FindParent(bl2));

			var br2Node = g.GetNode(br2);
			Assert.Equal("borderRight", br2Node.BorderType);
			Assert.Equal(1, br2Node.Rank);
			Assert.Equal(0, br2Node.Width);
			Assert.Equal(0, br2Node.Height);
			Assert.Equal("sg2", g.FindParent(br2));
		}
	}
}
