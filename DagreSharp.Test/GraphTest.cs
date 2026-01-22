using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class GraphTest
	{
		[Fact]
		public void InitialStateHasNoNodes()
		{
			var g = new Graph();
			Assert.Empty(g.Nodes);
		}

		[Fact]
		public void InitialStateHasNoEdges()
		{
			var g = new Graph();
			Assert.Empty(g.Edges);
		}

		[Fact]
		public void InitialStateDefaultsToSimpleDirectedGraph()
		{
			var g = new Graph();
			Assert.True(g.IsDirected);
			Assert.False(g.IsCompound);
			Assert.False(g.IsMultigraph);
		}

		[Fact]
		public void CanBeSetToUndirected()
		{
			var g = new Graph(false);
			Assert.False(g.IsDirected);
			Assert.False(g.IsCompound);
			Assert.False(g.IsMultigraph);
		}

		[Fact]
		public void CanBeSetToACompoundGraph()
		{
			var g = new Graph(true, false, true);
			Assert.True(g.IsDirected);
			Assert.True(g.IsCompound);
			Assert.False(g.IsMultigraph);
		}

		[Fact]
		public void CanBeSetToAMultigraph()
		{
			var g = new Graph(true, true, false);
			Assert.True(g.IsDirected);
			Assert.False(g.IsCompound);
			Assert.True(g.IsMultigraph);
		}

		[Fact]
		public void CanSetAndGetGraphOptions()
		{
			var g = new Graph();
			g.Options.Aligment = GraphAlignment.UpLeft;
			Assert.Equal(GraphAlignment.UpLeft, g.Options.Aligment);
		}

		[Fact]
		public void ReturnsNodesInTheGraph()
		{
			var g = new Graph();
			g.SetNode("a");
			g.SetNode("b");

			Assert.Equal(2, g.Nodes.Count);
			Assert.Contains(g.Nodes, n => n.Id == "a");
			Assert.Contains(g.Nodes, n => n.Id == "b");
		}

		[Fact]
		public void SourcesReturnsNodesInTheGraphThatHaveNoInEdges()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"]);
			g.SetNode("d");

			var sources = g.GetSources();

			Assert.Contains(sources, n => n.Id == "a");
			Assert.Contains(sources, n => n.Id == "d");
		}

		[Fact]
		public void SinksReturnsNodesInTheGraphThatHaveNoOutEdges()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"]);
			g.SetNode("d");

			var sinks = g.GetSinks();

			Assert.Contains(sinks, n => n.Id == "c");
			Assert.Contains(sinks, n => n.Id == "d");
		}

		[Fact]
		public void FilterNodesReturnsAnIdenticalGraphWhenTheFilterSelectsEverything()
		{
			var g = new Graph();

			g.SetNode("a");
			g.SetPath(["a", "b", "c"]);
			g.SetEdge("a", "c");

			var g2 = g.FilterNodes(s => { return true; });

			Assert.Contains(g.Nodes, n => n.Id == "a");
			Assert.Contains(g.Nodes, n => n.Id == "b");
			Assert.Contains(g.Nodes, n => n.Id == "c");

			var successors = g.GetSuccessors("a");
			Assert.Contains(successors, n => n.Id == "b");
			Assert.Contains(successors, n => n.Id == "c");

			successors = g.GetSuccessors("b");
			Assert.Contains(successors, n => n.Id == "c");
		}

		[Fact]
		public void FilterNodesReturnsAnEmptyGraphWhenTheFilterSelectsNothing()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"]);

			var g2 = g.FilterNodes(s => { return false; });

			Assert.Empty(g2.Nodes);
			Assert.Empty(g2.Edges);
		}

		[Fact]
		public void FilterNodesOnlyIncludesNodesForWhichTheFilterReturnsTrue()
		{
			var g = new Graph();
			g.SetNodes(["a", "b"]);

			var g2 = g.FilterNodes(s => { return s == "a"; });

			Assert.Single(g2.Nodes);
			Assert.Contains(g2.Nodes, n => n.Id == "a");
		}

		[Fact]
		public void RemovesEdgesThatAreConnectedToRemovedNodes()
		{
			var g = new Graph();
			g.SetEdge("a", "b");

			var g2 = g.FilterNodes(s => { return s == "a"; });

			Assert.Single(g2.Nodes);
			Assert.Contains(g2.Nodes, n => n.Id == "a");
			Assert.Empty(g2.Edges);
		}

		[Fact]
		public void PreservesTheDirectedOption()
		{
			var g = new Graph(true);
			var g2 = g.FilterNodes(s => { return true; });
			Assert.True(g2.IsDirected);

			g = new Graph(false);
			g2 = g.FilterNodes(s => { return true; });
			Assert.False(g2.IsDirected);
		}

		[Fact]
		public void PreservesTheMultigraphOption()
		{
			var g = new Graph(true, true);
			var g2 = g.FilterNodes(s => { return true; });
			Assert.True(g2.IsMultigraph);

			g = new Graph(false, false);
			g2 = g.FilterNodes(s => { return true; });
			Assert.False(g2.IsMultigraph);
		}

		[Fact]
		public void PreservesTheCompundOption()
		{
			var g = new Graph(true, true, true);
			var g2 = g.FilterNodes(s => { return true; });
			Assert.True(g2.IsCompound);

			g = new Graph(false, false, false);
			g2 = g.FilterNodes(s => { return true; });
			Assert.False(g2.IsCompound);
		}

		[Fact]
		public void IncludesSubgraphs()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");

			var g2 = g.FilterNodes(s => { return true; });

			var parent = g2.FindParent("a");
			Assert.NotNull(parent);
			Assert.Equal("parent", parent);
		}

		[Fact]
		public void IncludesMultiLevelSubgraphs()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("parent", "root");

			var g2 = g.FilterNodes(s => { return true; });
			Assert.Equal("parent", g2.FindParent("a"));
			Assert.Equal("root", g2.FindParent("parent"));
		}

		[Fact]
		public void PromotesANodeToAHigherSubgraphIfItsParentIsNotIncluded()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("parent", "root");

			var g2 = g.FilterNodes(s => { return s != "parent"; });
			Assert.Equal("root", g2.FindParent("a"));
		}

		[Fact]
		public void SetNodesCreateMultipleNodes()
		{
			var g = new Graph();
			g.SetNodes(["a", "b", "c"]);

			Assert.Contains(g.Nodes, n => n.Id == "a");
			Assert.Contains(g.Nodes, n => n.Id == "b");
			Assert.Contains(g.Nodes, n => n.Id == "c");
		}

		[Fact]
		public void SetNodesCanSetAValueForAllOfTheNodes()
		{
			var g = new Graph();
			g.SetNodes(["a", "b", "c"], n => { n.Name = "foo"; });

			Assert.Equal("foo", g.GetNode("a").Name);
			Assert.Equal("foo", g.GetNode("b").Name);
			Assert.Equal("foo", g.GetNode("c").Name);
		}

		[Fact]
		public void SetNodesIsChainable()
		{
			var g = new Graph();
			var result = g.SetNodes(["a", "b", "c"]);

			Assert.True(result is not null);
		}

		[Fact]
		public void SetNodeCreatesTheNodeIfItIsntPartOfTheGraph()
		{
			var g = new Graph();
			g.SetNode("a");

			Assert.True(g.HasNode("a"));
			Assert.Single(g.Nodes);
		}

		[Fact]
		public void SetNodeCanSetAValueForTheNode()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Name = "foo"; });

			Assert.Equal("foo", g.GetNode("a").Name );
		}

		[Fact]
		public void SetNodeDoesNotChangeTheNodesValueWithA1ArgInvocation()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Name = "foo"; });
			g.SetNode("a");

			Assert.Equal("foo", g.GetNode("a").Name);
		}

		[Fact]
		public void SetNodeCanRemoveTheNodesValue()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Name = "foo"; });
			g.SetNode("a", n => { n.Name = null; });

			Assert.Null(g.GetNode("a").Name);
		}

		[Fact]
		public void SetNodeIsIdempotent()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Name = "foo"; });
			g.SetNode("a", n => { n.Name = "foo"; });

			Assert.Equal("foo", g.GetNode("a").Name);
			Assert.Single(g.Nodes);
		}

		[Fact]
		public void SetNodeDefaultsSetsADefaultLabelForNewNodes()
		{
			var g = new Graph { ConfigureDefaultNode = n => { n.Rank = 2; }};
			g.SetNode("a");

			Assert.Equal(2, g.GetNode("a").Rank);
		}

		[Fact]
		public void SetNodeDefaultsDoesNotChangeExistingNodes()
		{
			var g = new Graph();
			g.SetNode("a");
			g.ConfigureDefaultNode = n => { n.Rank = 2; };

			Assert.NotEqual(2, g.GetNode("a").Rank);
		}

		[Fact]
		public void SetNodeDefaultsIsNotUsedIfAnExplicitValueIsSet()
		{
			var g = new Graph { ConfigureDefaultNode = n => { n.Rank = 2; } };
			g.SetNode("a", n => { n.Rank = 1; });

			Assert.Equal(1, g.GetNode("a").Rank);
		}

		[Fact]
		public void FindNodeReturnsNullIfTheNodeIsntPartOfTheGraph()
		{
			var g = new Graph();
			Assert.Null(g.FindNode("a"));
		}

		[Fact]
		public void GetNodeReturnsTheValueOfTheNodeIfItIsPartOfTheGraph()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Name = "foo"; });

			Assert.Equal("foo", g.GetNode("a").Name);
		}

		[Fact]
		public void RemoveNodeDoesNothingIfTheNodeIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.Nodes);

			g.RemoveNode("a");

			Assert.False(g.HasNode("a"));
			Assert.Empty(g.Nodes);
		}

		[Fact]
		public void RemovesTheNodeIfItIsInTheGraph()
		{
			var g = new Graph();
			g.SetNode("a");
			g.RemoveNode("a");

			Assert.False(g.HasNode("a"));
			Assert.Empty(g.Nodes);
		}

		[Fact]
		public void RemoveNodeIsIdempotent()
		{
			var g = new Graph();
			g.SetNode("a");
			g.RemoveNode("a");
			g.RemoveNode("a");

			Assert.False(g.HasNode("a"));
			Assert.Empty(g.Nodes);
		}

		[Fact]
		public void RemoveNodeRemovesEdgesIncidentOnTheNode()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");
			g.RemoveNode("b");

			Assert.Empty(g.Edges);
		}

		[Fact]
		public void RemoveNodeRemovesParentChildRelationshipsForTheNode()
		{
			var g = new Graph(true, false, true);
			g.SetParent("c", "b");
			g.SetParent("b", "a");
			g.RemoveNode("b");

			Assert.Null(g.FindParent("b"));
			Assert.Empty(g.GetChildren("b"));
			Assert.DoesNotContain(g.GetChildren("a"), c => c.Id == "b");
			Assert.Null(g.FindParent("c"));
		}

		[Fact]
		public void RemoveNodeIsChainable()
		{
			var g = new Graph();
			var result = g.RemoveNode("a");
			Assert.True(result is not null);
		}

		[Fact]
		public void SetParentThrowsIfTheGraphIsNotCompound()
		{
			var g = new Graph();
			Assert.Throws<InvalidOperationException>(() => g.SetParent("a", "parent"));
		}

		[Fact]
		public void SetParentCreatesTheParentIfItDoesNotExist()
		{
			var g = new Graph(true, false, true);
			g.SetNode("a");
			g.SetParent("a", "parent");

			Assert.True(g.HasNode("parent"));
			Assert.Equal("parent", g.FindParent("a"));
		}

		[Fact]
		public void SetParentCreatesTheChildIfItDoesNotExist()
		{
			var g = new Graph(true, false, true);
			g.SetNode("parent");
			g.SetParent("a", "parent");

			Assert.True(g.HasNode("a"));
			Assert.Equal("parent", g.FindParent("a"));
		}

		[Fact]
		public void SetParentHasTheParentAsNullIfItHasNeverBeenInvoked()
		{
			var g = new Graph();
			g.SetNode("a");

			Assert.Null(g.FindParent("a"));
		}

		[Fact]
		public void SetParentMovesTheNodeFromThePreviousParent()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("a", "parent2");

			Assert.Equal("parent2", g.FindParent("a"));
			Assert.Empty(g.GetChildren("parent"));
			Assert.Contains(g.GetChildren("parent2"), n => n.Id == "a");
		}

		[Fact]
		public void SetParentRemovesTheParentIfTheParentIsNull()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("a", null);

			Assert.Null(g.FindParent("a"));
			Assert.Equal(2, g.GetChildren().Count);
			Assert.Contains(g.GetChildren(), n => n.Id == "a");
			Assert.Contains(g.GetChildren(), n => n.Id == "parent");
		}

		[Fact]
		public void SetParentRemovesTheParentIfNoParentWasSpecified()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("a");

			Assert.Null(g.FindParent("a"));
			Assert.Equal(2, g.GetChildren().Count);
			Assert.Contains(g.GetChildren(), n => n.Id == "a");
			Assert.Contains(g.GetChildren(), n => n.Id == "parent");
		}

		[Fact]
		public void SetParentIsIdempotentToRemoveAParent()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("a");
			g.SetParent("a");

			Assert.Null(g.FindParent("a"));
			Assert.Equal(2, g.GetChildren().Count);
			Assert.Contains(g.GetChildren(), n => n.Id == "a");
			Assert.Contains(g.GetChildren(), n => n.Id == "parent");
		}

		[Fact]
		public void SetParentPreservesTheTreeInvariant()
		{
			var g = new Graph(true, false, true);
			g.SetParent("c", "b");
			g.SetParent("b", "a");

			Assert.Throws<InvalidOperationException>(() => g.SetParent("a", "c"));
		}

		[Fact]
		public void SetParentIsChainable()
		{
			var g = new Graph(true, false, true);
			var result = g.SetParent("a", "parent");
			Assert.True(result is not null);
		}

		[Fact]
		public void FindParentReturnsNullIfTheGraphIsNotCompound()
		{
			var g = new Graph(true, false, false);
			Assert.Null(g.FindParent("a"));
		}

		[Fact]
		public void FindParentReturnsNullIfTheNodeIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Null(g.FindParent("a"));
		}

		[Fact]
		public void FindParentDefaultsToNullForNewNodes()
		{
			var g = new Graph();
			g.SetNode("a");
			Assert.Null(g.FindParent("a"));
		}

		[Fact]
		public void FindParentReturnsTheCurrentParentAssignment()
		{
			var g = new Graph(true, false, true);
			g.SetNode("a");
			g.SetNode("parent");
			g.SetParent("a", "parent");

			Assert.Equal("parent", g.FindParent("a"));
		}

		[Fact]
		public void GetChildrenReturnsUndefinedIfTheNodeIsNotInTheGraph()
		{
			var g = new Graph(true, false, true);
			Assert.Empty(g.GetChildren("a"));
		}

		[Fact]
		public void GetChildrenDefaultsToAnEmptyListForNewNodes()
		{
			var g = new Graph(true, false, true);
			g.SetNode("a");
			Assert.Empty(g.GetChildren("a"));
		}

		[Fact]
		public void GetChildrenReturnsEmptyForANonCompoundGraphWithoutTheNode()
		{
			var g = new Graph();
			Assert.Empty(g.GetChildren("a"));
		}

		[Fact]
		public void GetChildrenReturnsEmptyForANonCompoundGraphWithTheNode()
		{
			var g = new Graph();
			g.SetNode("a");
			Assert.Empty(g.GetChildren("a"));
		}

		[Fact]
		public void GetChildrenReturnsAllNodesForTheRootOfANonCompoundGraph()
		{
			var g = new Graph();
			g.SetNode("a");
			g.SetNode("b");

			var children = g.GetChildren();

			Assert.Equal(2, children.Count);
			Assert.Contains(children, n => n.Id == "a");
			Assert.Contains(children, n => n.Id == "b");
		}

		[Fact]
		public void GetChildrenReturnsChildrenForTheNode()
		{
			var g = new Graph(true, false, true);
			g.SetParent("a", "parent");
			g.SetParent("b", "parent");

			var children = g.GetChildren("parent");

			Assert.Equal(2, children.Count);
			Assert.Contains(children, n => n.Id == "a");
			Assert.Contains(children, n => n.Id == "b");
		}

		[Fact]
		public void GetChildrenReturnsAllNodesWithoutAParentWhenTheParentIsNotSet()
		{
			var g = new Graph(true, false, true);
			g.SetNode("a");
			g.SetNode("b");
			g.SetNode("c");
			g.SetNode("parent");
			g.SetParent("a", "parent");

			var children = g.GetChildren();

			Assert.Equal(3, children.Count);
			Assert.Contains(children, n => n.Id == "b");
			Assert.Contains(children, n => n.Id == "c");
			Assert.Contains(children, n => n.Id == "parent");
		}

		[Fact]
		public void GetPredecessorsReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetPredecessors("a"));
		}

		[Fact]
		public void GetPredecessorsReturnsThePredecessorsOfANode()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");
			g.SetEdge("a", "a");

			Assert.Contains(g.GetPredecessors("a"), n => n.Id == "a");
			Assert.Contains(g.GetPredecessors("b"), n => n.Id == "a");
			Assert.Contains(g.GetPredecessors("c"), n => n.Id == "b");
		}

		[Fact]
		public void GetSuccessorsReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetSuccessors("a"));
		}

		[Fact]
		public void GetSuccessorsReturnsTheSuccessorsOfANode()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");
			g.SetEdge("a", "a");

			Assert.Contains(g.GetSuccessors("a"), n => n.Id == "a");
			Assert.Contains(g.GetSuccessors("a"), n => n.Id == "b");
			Assert.Equal(2, g.GetSuccessors("a").Count);

			Assert.Contains(g.GetSuccessors("b"), n => n.Id == "c");
			Assert.Single(g.GetSuccessors("b"));

			Assert.Empty(g.GetSuccessors("c"));
		}

		[Fact]
		public void GetNeighborsReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetNeighbors("a"));
		}

		[Fact]
		public void GetNeighborsReturnsTheNeighborsOfANode()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");
			g.SetEdge("a", "a");

			Assert.Contains(g.GetNeighbors("a"), n => n.Id == "a");
			Assert.Contains(g.GetNeighbors("a"), n => n.Id == "b");
			Assert.Equal(2, g.GetNeighbors("a").Count);

			Assert.Contains(g.GetNeighbors("b"), n => n.Id == "a");
			Assert.Contains(g.GetNeighbors("b"), n => n.Id == "c");
			Assert.Equal(2, g.GetNeighbors("b").Count);

			Assert.Contains(g.GetNeighbors("c"), n => n.Id == "b");
			Assert.Single(g.GetNeighbors("c"));
		}

		[Fact]
		public void IsLeafReturnsFalseForConnectedNodeInIndirectedGraph()
		{
			var g = new Graph(false);
			g.SetNode("a");
			g.SetNode("b");
			g.SetEdge("a", "b");
			Assert.False(g.IsLeaf("b"));
		}

		[Fact]
		public void IsLeafReturnsTrueForAnUnconnectedNodeInUndirectedGraph()
		{
			var g = new Graph(false);
			g.SetNode("a");
			Assert.True(g.IsLeaf("a"));
		}

		[Fact]
		public void IsLeafReturnsTrueForAnUnconnectedNodeInDirectedGraph()
		{
			var g = new Graph(false);
			g.SetNode("a");
			Assert.True(g.IsLeaf("a"));
		}

		[Fact]
		public void IsLeafReturnsFalseForPredecessorNodeInDirectedGraph()
		{
			var g = new Graph(true);
			g.SetNode("a");
			g.SetNode("b");
			g.SetEdge("a", "b");

			Assert.False(g.IsLeaf("a"));
		}

		[Fact]
		public void IsLeafReturnsFalseForSuccessorNodeInDirectedGraph()
		{
			var g = new Graph();
			g.SetNode("a");
			g.SetNode("b");
			g.SetEdge("a", "b");

			Assert.True(g.IsLeaf("b"));
		}

		[Fact]
		public void EdgesIsEmptyIfThereAreNoEdgesInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.Edges);
		}

		[Fact]
		public void EdgesReturnsTheEdgesInTheGraph()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");

			Assert.Equal(2, g.Edges.Count);
			Assert.Contains(g.Edges, e => e.From == "a" && e.To == "b");
			Assert.Contains(g.Edges, e => e.From == "b" && e.To == "c");
		}

		[Fact]
		public void SetPathCreatesAPathOfMultipleEdges()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"]);

			Assert.True(g.HasEdge("a", "b"));
			Assert.True(g.HasEdge("b", "c"));
		}

		[Fact]
		public void SetPathCanSetAValueForAllOfTheEdges()
		{
			var g = new Graph();
			g.SetPath(["a", "b", "c"], e => { e.Name = "foo"; });

			Assert.Equal("foo", g.GetEdge("a", "b").Name);
			Assert.Equal("foo", g.GetEdge("b", "c").Name);
		}

		[Fact]
		public void SetPathIsChainable()
		{
			var g = new Graph();
			Assert.Equal(g, g.SetPath(["a", "b", "c"]));
		}

		[Fact]
		public void SetEdgeCreatesTheEdgeIfItIsntPartOfTheGraph()
		{
			var g = new Graph();
			g.SetNode("a");
			g.SetNode("b");
			g.SetEdge("a", "b");

			Assert.True(g.HasEdge("a", "b"));
			Assert.Contains(g.Edges, e => e.From == "a" && e.To == "b");
			Assert.Single(g.Edges);
		}

		[Fact]
		public void SetEdgeCreatesTheNodesForTheEdgeIfTheyAreNotPartOfTheGraph()
		{
			var g = new Graph();
			g.SetEdge("a", "b");

			Assert.True(g.HasNode("a"));
			Assert.True(g.HasNode("b"));
			Assert.Equal(2, g.Nodes.Count);
		}

		[Fact]
		public void SetEdgeCreatesAMultiEdgeIfItIsntPartOfTheGraph()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", "name");
			Assert.False(g.HasEdge("a", "b"));
			Assert.True(g.HasEdge("a", "b", "name"));
		}

		[Fact]
		public void SetEdgeThrowsIfAMultiEdgeIsUsedWithANonMultigraph()
		{
			var g = new Graph();
			Assert.Throws<InvalidOperationException>(() => g.SetEdge("a", "b", "name"));
		}

		[Fact]
		public void SetEdgeChangesTheValueForAnEdgeIfItIsAlreadyInTheGraph()
		{
			var g = new Graph();
			g.SetEdge("a", "b", null, e => { e.Name = "foo"; });
			g.SetEdge("a", "b", null, e => { e.Name = "bar"; });

			Assert.Equal("bar", g.GetEdge("a", "b").Name);
		}

		[Fact]
		public void SetEdgeChangesTheValueForAMultiEdgeIfItIsAlreadyInTheGraph()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", "name", e => { e.Name = "value"; });
			g.SetEdge("a", "b", "name", e => { e.Name = null; });

			Assert.Null(g.GetEdge("a", "b", "name").Name);
		}

		[Fact]
		public void SetEdgeTreatsEdgesInOppositeDirectionsAsDistinctInADigraph()
		{
			var g = new Graph();
			g.SetEdge("a", "b");

			Assert.True(g.HasEdge("a", "b"));
			Assert.False(g.HasEdge("b", "a"));
		}

		[Fact]
		public void SetEdgeHandlesUndirectedGraphEdges()
		{
			var g = new Graph(false);
			var edge = g.SetEdge("a", "b");

			Assert.Equal(edge, g.GetEdge("a", "b"));
			Assert.Equal(edge, g.GetEdge("b", "a"));
		}

		[Fact]
		public void FindEdgeReturnsNullIfTheEdgeIsntPartOfTheGraph()
		{
			var g = new Graph();
			Assert.Null(g.FindEdge("a", "b"));
		}

		[Fact]
		public void GetEdgeReturnsTheEdgeIfItIsPartOfTheGraph()
		{
			var g = new Graph();
			var edge = g.SetEdge("a", "b");

			Assert.Equal(edge, g.GetEdge("a", "b"));
		}

		[Fact]
		public void GetEdgeReturnsMultiEdgeIfItIsPartOfTheGraph()
		{
			var g = new Graph(true, true);
			var edge = g.SetEdge("a", "b", "foo");

			Assert.Equal(edge, g.GetEdge("a", "b", "foo"));
			Assert.Null(g.FindEdge("a", "b"));
		}

		[Fact]
		public void GetEdgeReturnsAnEdgeInEitherDirectionInAnUndirectedGraph()
		{
			var g = new Graph(false);
			var edge = g.SetEdge("a", "b");

			Assert.Equal(edge, g.GetEdge("a", "b"));
			Assert.Equal(edge, g.GetEdge("b", "a"));
		}

		[Fact]
		public void RemoveEdgeHasNoEffectIfTheEdgeIsNotInTheGraph()
		{
			var g = new Graph();
			g.RemoveEdge("a", "b");

			Assert.False(g.HasEdge("a", "b"));
			Assert.Empty(g.Edges);
		}

		[Fact]
		public void RemoveEdgeCanRemoveAnEdgeBySeparateIds()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", "foo");
			g.RemoveEdge("a", "b", "foo");

			Assert.False(g.HasEdge("a", "b", "foo"));
			Assert.Empty(g.Edges);
		}

		[Fact]
		public void RemoveEdgeCorrectlyRemovesNeighbors()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.RemoveEdge("a", "b");

			Assert.Empty(g.GetSuccessors("a"));
			Assert.Empty(g.GetNeighbors("a"));
			Assert.Empty(g.GetPredecessors("b"));
			Assert.Empty(g.GetNeighbors("b"));
		}

		[Fact]
		public void RemoveEdgeCorrectlyDecrementsNeighborCounts()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "foo");
			g.RemoveEdge("a", "b");

			Assert.True(g.HasEdge("a", "b", "foo"));
			Assert.Contains(g.GetSuccessors("a"), n => n.Id == "b");
			Assert.Single(g.GetSuccessors("a"));

			Assert.Contains(g.GetNeighbors("a"), n => n.Id == "b");
			Assert.Single(g.GetNeighbors("a"));

			Assert.Contains(g.GetPredecessors("b"), n => n.Id == "a");
			Assert.Single(g.GetPredecessors("b"));

			Assert.Contains(g.GetNeighbors("b"), n => n.Id == "a");
			Assert.Single(g.GetNeighbors("b"));
		}

		[Fact]
		public void RemoveEdgeWorksWithUndirectedGraphs()
		{
			var g = new Graph(false);
			g.SetEdge("h", "g");
			g.RemoveEdge("g", "h");

			Assert.Empty(g.GetNeighbors("g"));
			Assert.Empty(g.GetNeighbors("h"));
		}

		[Fact]
		public void RemoveEdgeIsChainable()
		{
			var g = new Graph();
			Assert.Equal(g, g.RemoveEdge("g", "h"));
		}

		[Fact]
		public void GetInEdgesReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetInEdges("a"));
		}

		[Fact]
		public void GetInEdgesReturnsTheEdgesThatPointAtTheSpecifiedNode()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");

			Assert.Empty(g.GetInEdges("a"));

			Assert.Single(g.GetInEdges("b"));
			Assert.Contains(g.GetInEdges("b"), e => e.From == "a" && e.To == "b");

			Assert.Single(g.GetInEdges("c"));
			Assert.Contains(g.GetInEdges("c"), e => e.From == "b" && e.To == "c");
		}

		[Fact]
		public void GetInEdgesWorksForMultigraphs()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "bar");
			g.SetEdge("a", "b", "foo");

			Assert.Empty(g.GetInEdges("a"));
			Assert.Equal(3, g.GetInEdges("b").Count);
			Assert.Contains(g.GetInEdges("b"), e => e.From == "a" && e.To == "b" && e.Name == "bar");
			Assert.Contains(g.GetInEdges("b"), e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(g.GetInEdges("b"), e => e.From == "a" && e.To == "b");
		}

		[Fact]
		public void GetInEdgesCanReturnOnlyEdgesFromASpecifiedNode()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "foo");
			g.SetEdge("a", "c");
			g.SetEdge("b", "c");
			g.SetEdge("z", "a");
			g.SetEdge("z", "b");

			Assert.Empty(g.GetInEdges("a", "b"));
			Assert.Equal(2, g.GetInEdges("b", "a").Count);
			Assert.Contains(g.GetInEdges("b", "a"), e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(g.GetInEdges("b", "a"), e => e.From == "a" && e.To == "b");
		}

		[Fact]
		public void GetOutEdgesReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetOutEdges("a"));
		}

		[Fact]
		public void GetOutEdgesReturnsAllEdgesThatThisNodePointsAt()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");

			Assert.Single(g.GetOutEdges("a"));
			Assert.Contains(g.GetOutEdges("a"), e => e.From == "a" && e.To == "b");

			Assert.Single(g.GetOutEdges("b"));
			Assert.Contains(g.GetOutEdges("b"), e => e.From == "b" && e.To == "c");

			Assert.Empty(g.GetOutEdges("c"));
		}

		[Fact]
		public void GetOutEdgesWorksForMultigraphs()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "bar");
			g.SetEdge("a", "b", "foo");

			Assert.Equal(3, g.GetOutEdges("a").Count);
			Assert.Contains(g.GetOutEdges("a"), e => e.From == "a" && e.To == "b" && e.Name == "bar");
			Assert.Contains(g.GetOutEdges("a"), e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(g.GetOutEdges("a"), e => e.From == "a" && e.To == "b");
			Assert.Empty(g.GetOutEdges("b"));
		}

		[Fact]
		public void GetOutEdgesCanReturnOnlyEdgesToASpecifiedNode()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "foo");
			g.SetEdge("a", "c");
			g.SetEdge("b", "c");
			g.SetEdge("z", "a");
			g.SetEdge("z", "b");

			Assert.Equal(2, g.GetOutEdges("a", "b").Count);
			Assert.Contains(g.GetOutEdges("a", "b"), e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(g.GetOutEdges("a", "b"), e => e.From == "a" && e.To == "b");
			Assert.Empty(g.GetOutEdges("b", "a"));
		}

		[Fact]
		public void GetNodeEdgesReturnsEmptyForANodeThatIsNotInTheGraph()
		{
			var g = new Graph();
			Assert.Empty(g.GetNodeEdges("a"));
		}

		[Fact]
		public void GetNodeEdgesReturnsAlledgesthatThisNodePointsAt()
		{
			var g = new Graph();
			g.SetEdge("a", "b");
			g.SetEdge("b", "c");

			var nodeEdgesA = g.GetNodeEdges("a");
			var nodeEdgesB = g.GetNodeEdges("b");
			var nodeEdgesC = g.GetNodeEdges("c");

			Assert.Single(nodeEdgesA);
			Assert.Contains(nodeEdgesA, e => e.From == "a" && e.To == "b");
			
			Assert.Equal(2, nodeEdgesB.Count);
			Assert.Contains(nodeEdgesB, e => e.From == "a" && e.To == "b");
			Assert.Contains(nodeEdgesB, e => e.From == "b" && e.To == "c");

			Assert.Single(nodeEdgesC);
			Assert.Contains(nodeEdgesC, e => e.From == "b" && e.To == "c");
		}

		[Fact]
		public void GetNodeEdgesWorksForMultigraphs()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "bar");
			g.SetEdge("a", "b", "foo");

			var nodeEdgesA = g.GetNodeEdges("a");
			var nodeEdgesB = g.GetNodeEdges("b");

			Assert.Equal(3, nodeEdgesA.Count);
			Assert.Contains(nodeEdgesA, e => e.From == "a" && e.To == "b" && e.Name == "bar");
			Assert.Contains(nodeEdgesA, e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(nodeEdgesA, e => e.From == "a" && e.To == "b");

			Assert.Equal(3, nodeEdgesB.Count);
			Assert.Contains(nodeEdgesB, e => e.From == "a" && e.To == "b" && e.Name == "bar");
			Assert.Contains(nodeEdgesB, e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(nodeEdgesB, e => e.From == "a" && e.To == "b");
		}

		[Fact]
		public void GetNodeEdgesCanReturnOnlyEdgesBetweenSpecificNodes()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b");
			g.SetEdge("a", "b", "foo");
			g.SetEdge("a", "c");
			g.SetEdge("b", "c");
			g.SetEdge("z", "a");
			g.SetEdge("z", "b");

			var nodeEdgesAB = g.GetNodeEdges("a", "b");
			var nodeEdgesBA = g.GetNodeEdges("b", "a");

			Assert.Equal(2, nodeEdgesAB.Count);
			Assert.Contains(nodeEdgesAB, e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(nodeEdgesAB, e => e.From == "a" && e.To == "b");

			Assert.Equal(2, nodeEdgesBA.Count);
			Assert.Contains(nodeEdgesBA, e => e.From == "a" && e.To == "b" && e.Name == "foo");
			Assert.Contains(nodeEdgesBA, e => e.From == "a" && e.To == "b");
		}
	}
}
