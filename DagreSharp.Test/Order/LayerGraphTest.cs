using DagreSharp.GraphLibrary;
using DagreSharp.Order;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace DagreSharp.Test.Order
{
	public class LayerGraphTest
	{
		private readonly Graph g = new(true, true, true);

		[Fact]
		public void PlacesMovableNodesWithNoParentsUnderTheRootNode()
		{
			g.SetNode("a", n => { n.Rank = 1; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetNode("d", n => { n.Rank = 3; });

			var lg = LayerGraph.Build(g, 1, g.GetInEdges);

			Assert.True(lg.HasNode(lg.Options.Root));
			var children = lg.GetChildren().ToList();
			Assert.Single(children);
			Assert.Equal(lg.Options.Root, children[0].Id);

			var rootChildren = lg.GetChildren(lg.Options.Root).OrderBy(n => n.Id).ToList();
			Assert.Equal(2, rootChildren.Count);
			Assert.Contains(rootChildren, c => c.Id == "a");
			Assert.Contains(rootChildren, c => c.Id == "b");
		}

		[Fact]
		public void CopiesFlatNodesFromTheLayerToTheGraph()
		{
			g.SetNode("a", n => { n.Rank = 1; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetNode("d", n => { n.Rank = 3; });

			var lg = LayerGraph.Build(g, 1, g.GetInEdges);
			Assert.Contains(lg.Nodes, c => c.Id == "a");

			lg = LayerGraph.Build(g, 1, g.GetInEdges);
			Assert.Contains(lg.Nodes, c => c.Id == "b");

			lg = LayerGraph.Build(g, 2, g.GetInEdges);
			Assert.Contains(lg.Nodes, c => c.Id == "c");

			lg = LayerGraph.Build(g, 3, (v, u) => g.GetInEdges(v, u));
			Assert.Contains(lg.Nodes, c => c.Id == "d");
		}

		[Fact]
		public void UsesTheOriginalNodeLabelForCopiedNodes()
		{
			// This allows us to make updates to the original graph and have them
			// be available automatically in the layer graph.
			g.SetNode("a", n => { n.Rank = 1; n.Name = "foo"; });
			g.SetNode("b", n => { n.Rank = 2; n.Name = "bar"; });
			g.SetEdge("a", "b", null, e => { e.Weight = 1; });

			var lg = LayerGraph.Build(g, 2, g.GetInEdges);

			Assert.Equal("foo", lg.GetNode("a").Name);
			g.GetNode("a").Name = "updated";
			Assert.Equal("updated", lg.GetNode("a").Name);

			Assert.Equal("bar", lg.GetNode("b").Name);
			g.GetNode("b").Name = "updated";
			Assert.Equal("updated", lg.GetNode("b").Name);
		}

		[Fact]
		public void CopiesEdgesIncidentOnRankNodesToTheGraphInEdges()
		{
			g.SetNode("a", n => { n.Rank = 1; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetNode("d", n => { n.Rank = 3; });

			g.SetEdge("a", "c", null, e => { e.Weight = 2; });
			g.SetEdge("b", "c", null, e => { e.Weight = 3; });
			g.SetEdge("c", "d", null, e => { e.Weight = 4; });

			Assert.Empty(LayerGraph.Build(g, 1, g.GetInEdges).Edges);

			Assert.Equal(2, LayerGraph.Build(g, 2, g.GetInEdges).Edges.Count);
			Assert.Equal(2, LayerGraph.Build(g, 2, g.GetInEdges).GetEdge("a","c").Weight);
			Assert.Equal(3, LayerGraph.Build(g, 2, g.GetInEdges).GetEdge("b", "c").Weight);

			Assert.Single(LayerGraph.Build(g, 3, g.GetInEdges).Edges);
			Assert.Equal(4, LayerGraph.Build(g, 3, g.GetInEdges).GetEdge("c", "d").Weight);
		}

		[Fact]
		public void CopiesEdgesIncidentOnRankNodesToTheGraphOutEdges()
		{
			g.SetNode("a", n => { n.Rank = 1; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetNode("c", n => { n.Rank = 2; });
			g.SetNode("d", n => { n.Rank = 3; });

			g.SetEdge("a", "c", null, e => { e.Weight = 2; });
			g.SetEdge("b", "c", null, e => { e.Weight = 3; });
			g.SetEdge("c", "d", null, e => { e.Weight = 4; });

			
			Assert.Equal(2, LayerGraph.Build(g, 1, g.GetOutEdges).Edges.Count);
			Assert.Equal(2, LayerGraph.Build(g, 1, g.GetOutEdges).GetEdge("c", "a").Weight);
			Assert.Equal(3, LayerGraph.Build(g, 1, g.GetOutEdges).GetEdge("c", "b").Weight);
			
			Assert.Single(LayerGraph.Build(g, 2, g.GetOutEdges).Edges);
			Assert.Equal(4, LayerGraph.Build(g, 2, g.GetOutEdges).GetEdge("d", "c").Weight);

			Assert.Empty(LayerGraph.Build(g, 3, g.GetOutEdges).Edges);
		}

		[Fact]
		public void CollapsesMultiEdges()
		{
			g.SetNode("a", n => { n.Rank = 1; });
			g.SetNode("b", n => { n.Rank = 2; });

			g.SetEdge("a", "b", null, e => { e.Weight = 2; });
			g.SetEdge("a", "b", "multi", e => { e.Weight = 3; });

			Assert.Equal(5, LayerGraph.Build(g, 2, g.GetInEdges).GetEdge("a", "b").Weight);
		}

		[Fact]
		public void PreservesHierarchyForTheMovableLayer()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 0; });
			g.SetNode("c", n => { n.Rank = 0; });
			var sgNode = g.SetNode("sg", n => { n.MinRank = 0; n.MaxRank = 0; });
			sgNode.BorderLeft.Add(0, "bl");
			sgNode.BorderRight.Add(0, "br");

			g.SetParent("a", "sg");
			g.SetParent("b", "sg");

			var lg = LayerGraph.Build(g, 0, g.GetInEdges);
			var root = lg.Options.Root;

			var rootChildren = lg.GetChildren(root).ToList();
			Assert.Contains(rootChildren, c => c.Id == "c");
			Assert.Contains(rootChildren, c => c.Id == "sg");

			Assert.Equal("sg", lg.FindParent("a"));
			Assert.Equal("sg", lg.FindParent("b"));
		}
	}
}
