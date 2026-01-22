
using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class NestingGraphTest
	{
		private readonly Graph g = new(true, false, true);

		[Fact]
		public void ConnectsADisconnectedGraph()
		{
			g.SetNode("a");
			g.SetNode("b");

			Assert.Equal(2, Algorithm.Components(g).Count);

			NestingGraph.Run(g);

			Assert.Single(Algorithm.Components(g));
			Assert.True(g.HasNode("a"));
			Assert.True(g.HasNode("b"));
		}

		[Fact]
		public void AddsBorderNodesToTheTopAndBottomOfASubgraph()
		{
			g.SetParent("a", "sg1");

			NestingGraph.Run(g);

			var sg1Node = g.GetNode("sg1");
			var borderTop = sg1Node.BorderTop;
			var borderBottom = sg1Node.BorderBottom;

			Assert.NotNull(borderTop);
			Assert.NotNull(borderBottom);

			Assert.Equal("sg1", g.FindParent(borderTop));
			Assert.Equal("sg1", g.FindParent(borderBottom));

			var borderTopAOutEdges = g.GetOutEdges(borderTop, "a").ToList();
			Assert.Single(borderTopAOutEdges);
			var borderTopAOutEdge = borderTopAOutEdges.First();
			Assert.Equal(1, g.GetEdge(borderTopAOutEdge.From, borderTopAOutEdge.To).MinLength);

			var aBorderBottomOutEdges = g.GetOutEdges("a", borderBottom).ToList();
			Assert.Single(aBorderBottomOutEdges);
			var aBorderBottomOutEdge = aBorderBottomOutEdges.First();
			Assert.Equal(1, g.GetEdge(aBorderBottomOutEdge.From, aBorderBottomOutEdge.To).MinLength);

			var borderTopNode = g.GetNode(borderTop);
			Assert.Equal(0, borderTopNode.Width);
			Assert.Equal(0, borderTopNode.Height);
			Assert.Equal(DummyType.Border, borderTopNode.DummyType);

			var borderBottomNode = g.GetNode(borderBottom);
			Assert.Equal(0, borderBottomNode.Width);
			Assert.Equal(0, borderBottomNode.Height);
			Assert.Equal(DummyType.Border, borderBottomNode.DummyType);
		}

		[Fact]
		public void AddsEdgesBetweenBordersOfNestedSubgraphs()
		{
			g.SetParent("sg2", "sg1");
			g.SetParent("a", "sg2");

			NestingGraph.Run(g);

			var sg1Top = g.GetNode("sg1").BorderTop;
			var sg1Bottom = g.GetNode("sg1").BorderBottom;
			var sg2Top = g.GetNode("sg2").BorderTop;
			var sg2Bottom = g.GetNode("sg2").BorderBottom;

			Assert.NotNull(sg1Top);
			Assert.NotNull(sg1Bottom);
			Assert.NotNull(sg2Top);
			Assert.NotNull(sg2Bottom);

			var topOutEdges = g.GetOutEdges(sg1Top, sg2Top);
			Assert.Single(topOutEdges);
			var topOutEdge = topOutEdges.First();
			Assert.Equal(1, g.GetEdge(topOutEdge.From, topOutEdge.To).MinLength);

			var bottomOutEdges = g.GetOutEdges(sg2Bottom, sg1Bottom);
			Assert.Single(bottomOutEdges);
			var bottomOutEdge = bottomOutEdges.First();
			Assert.Equal(1, g.GetEdge(bottomOutEdge.From, bottomOutEdge.To).MinLength);
		}

		[Fact]
		public void AddsSufficientWeightToBorderToNodeEdges()
		{
			// We want to keep subgraphs tight, so we should ensure that the weight for
			// the edge between the top (and bottom) border nodes and nodes in the
			// subgraph have weights exceeding anything in the graph.
			g.SetParent("x", "sg");
			g.SetEdge("a", "x", null, e => { e.Weight = 100; });
			g.SetEdge("x", "b", null, e => { e.Weight = 200; });

			NestingGraph.Run(g);

			var top = g.GetNode("sg").BorderTop;
			var bot = g.GetNode("sg").BorderBottom;
			Assert.True(g.GetEdge(top, "x").Weight > 300);
			Assert.True(g.GetEdge("x", bot).Weight > 300);
		}

		[Fact]
		public void AddsAnEdgeFromTheRootToTheTopsOfTopLevelSubgraphs()
		{
			g.SetParent("a", "sg1");

			NestingGraph.Run(g);

			var root = g.Options.NestingRoot;
			var borderTop = g.GetNode("sg1").BorderTop;

			Assert.NotNull(root);
			Assert.NotNull(borderTop);
			Assert.Single(g.GetOutEdges(root, borderTop));

			var outEdge = g.GetOutEdges(root, borderTop).First();
			Assert.True(g.HasEdge(outEdge.From, outEdge.To));
		}

		[Fact]
		public void AddsAnEdgeFromRootToEachNodeWithTheCorrectMinlen1()
		{
			g.SetNode("a");

			NestingGraph.Run(g);

			var root = g.Options.NestingRoot;
			Assert.NotNull(root);

			var outEdges = g.GetOutEdges(root, "a");
			Assert.Single(outEdges);
			var outEdge = outEdges.First();
			var edge = g.GetEdge(outEdge.From, outEdge.To);
			Assert.Equal(0, edge.Width);
			Assert.Equal(1, edge.MinLength);
		}

		[Fact]
		public void AddsAnEdgeFromRootToEachNodeWithTheCorrectMinlen2()
		{
			g.SetParent("a", "sg1");
			
			NestingGraph.Run(g);

			var root = g.Options.NestingRoot;
			Assert.NotNull(root);
			var outEdges = g.GetOutEdges(root, "a");
			Assert.Single(outEdges);
			var outEdge = outEdges.First();
			var edge = g.GetEdge(outEdge.From, outEdge.To);
			Assert.Equal(0, edge.Width);
			Assert.Equal(3, edge.MinLength);
		}

		[Fact]
		public void AddsAnEdgeFromRootToEachNodeWithTheCorrectMinlen3()
		{
			g.SetParent("sg2", "sg1");
			g.SetParent("a", "sg2");

			NestingGraph.Run(g);

			var root = g.Options.NestingRoot;
			Assert.NotNull(root);
			var outEdges = g.GetOutEdges(root, "a");
			Assert.Single(outEdges);
			var outEdge = outEdges.First();
			var edge = g.GetEdge(outEdge.From, outEdge.To);
			Assert.Equal(0, edge.Width);
			Assert.Equal(5, edge.MinLength);
		}

		[Fact]
		public void DoesNotAddAnEdgeFromTheRootToItself()
		{
			g.SetNode("a");

			NestingGraph.Run(g);

			var root = g.Options.NestingRoot;
			Assert.Empty(g.GetOutEdges(root, root));
		}

		[Fact]
		public void ExpandsInterNodeEdgesToSeparateSGBorderAndNodes1()
		{
			g.SetEdge("a", "b", null, e => { e.MinLength = 1; });

			NestingGraph.Run(g);

			Assert.Equal(1, g.GetEdge("a", "b").MinLength);
		}

		[Fact]
		public void ExpandsInterNodeEdgesToSeparateSGBorderAndNodes2()
		{
			g.SetParent("a", "sg1");
			g.SetEdge("a", "b", null, e => { e.MinLength = 1; });

			NestingGraph.Run(g);

			Assert.Equal(3, g.GetEdge("a", "b").MinLength);
		}

		[Fact]
		public void ExpandsInterNodeEdgesToSeparateSGBorderAndNodes3()
		{
			g.SetParent("sg2", "sg1");
			g.SetParent("a", "sg2");
			g.SetEdge("a", "b", null, e => { e.MinLength = 1; });

			NestingGraph.Run(g);

			Assert.Equal(5, g.GetEdge("a", "b").MinLength);
		}

		[Fact]
		public void SetsMinlenCorrectlyForNestedSGBorderToChildren()
		{
			g.SetParent("a", "sg1");
			g.SetParent("sg2", "sg1");
			g.SetParent("b", "sg2");

			NestingGraph.Run(g);

			// We expect the following layering:
			//
			// 0: root
			// 1: empty (close sg2)
			// 2: empty (close sg1)
			// 3: open sg1
			// 4: open sg2
			// 5: a, b
			// 6: close sg2
			// 7: close sg1

			var root = g.Options.NestingRoot;
			var sg1Top = g.GetNode("sg1").BorderTop;
			var sg1Bot = g.GetNode("sg1").BorderBottom;
			var sg2Top = g.GetNode("sg2").BorderTop;
			var sg2Bot = g.GetNode("sg2").BorderBottom;

			Assert.Equal(3, g.GetEdge(root, sg1Top).MinLength);
			Assert.Equal(1, g.GetEdge(sg1Top, sg2Top).MinLength);
			Assert.Equal(2, g.GetEdge(sg1Top, "a").MinLength);
			Assert.Equal(2, g.GetEdge("a", sg1Bot).MinLength);
			Assert.Equal(1, g.GetEdge(sg2Top, "b").MinLength);
			Assert.Equal(1, g.GetEdge("b", sg2Bot).MinLength);
			Assert.Equal(1, g.GetEdge(sg2Bot, sg1Bot).MinLength);
		}

		[Fact]
		public void CleanupRemovesNestingGraphEdges()
		{
			g.SetParent("a", "sg1");
			g.SetEdge("a", "b", null, e => { e.MinLength = 1; });

			NestingGraph.Run(g);
			NestingGraph.Cleanup(g);

			var successors = g.GetSuccessors("a");
			Assert.Single(successors);
			Assert.Equal("b", successors.First().Id);
		}

		[Fact]
		public void CleanupRemovesTheRootNode()
		{
			g.SetParent("a", "sg1");

			NestingGraph.Run(g);
			NestingGraph.Cleanup(g);

			Assert.Equal(4, g.Nodes.Count); // sg1 + sg1Top + sg1Bottom + "a"
		}
	}
}
