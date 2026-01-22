using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class BrandesKopfTest
	{
		private Graph g = new();

		[Fact]
		public void FindType1ConflictsDoesNotMarkEdgesThatHaveNoConflict()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			// Set up crossing
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			g.RemoveEdge("a", "d");
			g.RemoveEdge("b", "c");
			g.SetEdge("a", "c");
			g.SetEdge("b", "d");

			var conflicts = BrandesKopf.FindType1Conflicts(g, layering);

			Assert.False(BrandesKopf.HasConflict(conflicts, "a", "c"));
			Assert.False(BrandesKopf.HasConflict(conflicts, "b", "d"));
		}

		[Fact]
		public void FindType1ConflictsDoesNotMarkType0ConflictsNoDummies()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			// Set up crossing
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);
			var conflicts = BrandesKopf.FindType1Conflicts(g, layering);

			Assert.False(BrandesKopf.HasConflict(conflicts, "a", "d"));
			Assert.False(BrandesKopf.HasConflict(conflicts, "b", "c"));
		}

		[Fact]
		public void FindType1ConflictsDoesNotMarkType0ConflictsNodeIsDummy()
		{
			foreach (var v in new[] { "a", "b", "c", "d" })
			{
				g = new Graph();
				g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
				g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
				g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
				g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
				// Set up crossing
				g.SetEdge("a", "d");
				g.SetEdge("b", "c");

				var layering = Util.BuildLayerMatrix(g);
				g.GetNode(v).DummyType = DummyType.Border;

				var conflicts = BrandesKopf.FindType1Conflicts(g, layering);

				Assert.False(BrandesKopf.HasConflict(conflicts, "a", "d"));
				Assert.False(BrandesKopf.HasConflict(conflicts, "b", "c"));
			}
		}

		[Fact]
		public void FindType1ConflictsDoesMarkType1ConflictsNodeIsNonDummy()
		{
			foreach (var v in new[] { "a", "b", "c", "d" })
			{
				g = new Graph();
				g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
				g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
				g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
				g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
				// Set up crossing
				g.SetEdge("a", "d");
				g.SetEdge("b", "c");

				var layering = Util.BuildLayerMatrix(g);

				foreach (var w in new[] { "a", "b", "c", "d" })
				{
					if (v != w)
					{
						g.GetNode(w).DummyType = DummyType.Border;
					}
				}

				var conflicts = BrandesKopf.FindType1Conflicts(g, layering);

				if (v == "a" || v == "d")
				{
					Assert.True(BrandesKopf.HasConflict(conflicts, "a", "d"));
					Assert.False(BrandesKopf.HasConflict(conflicts, "b", "c"));
				}
				else
				{
					Assert.False(BrandesKopf.HasConflict(conflicts, "a", "d"));
					Assert.True(BrandesKopf.HasConflict(conflicts, "b", "c"));
				}
			}
		}

		[Fact]
		public void FindType1ConflictsDoesNotMarkType2ConflictsAllDummies()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			// Set up crossing
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			foreach (var v in new[] { "a", "b", "c", "d" })
			{
				g.GetNode(v).DummyType = DummyType.Border;
			}

			var conflicts = BrandesKopf.FindType1Conflicts(g, layering);

			Assert.False(BrandesKopf.HasConflict(conflicts, "a", "d"));
			Assert.False(BrandesKopf.HasConflict(conflicts, "b", "c"));
		}

		[Fact]
		public void FindType2ConflictsMarksType2ConflictsFavoringBorderSegments1()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			// Set up crossing
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			foreach (var v in new[] { "a", "d" })
			{
				g.GetNode(v).DummyType = DummyType.SelfEdge;
			}

			foreach (var v in new[] { "b", "c" })
			{
				g.GetNode(v).DummyType = DummyType.Border;
			}

			var conflicts = BrandesKopf.FindType2Conflicts(g, layering);

			Assert.True(BrandesKopf.HasConflict(conflicts, "a", "d"));
			Assert.False(BrandesKopf.HasConflict(conflicts, "b", "c"));
		}

		[Fact]
		public void FindType2ConflictsMarksType2ConflictsFavoringBorderSegments2()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			// Set up crossing
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			foreach (var v in new[] { "b", "c" })
			{
				g.GetNode(v).DummyType = DummyType.SelfEdge;
			}

			foreach (var v in new[] { "a", "d" })
			{
				g.GetNode(v).DummyType = DummyType.Border;
			}

			var conflicts = BrandesKopf.FindType2Conflicts(g, layering);

			Assert.False(BrandesKopf.HasConflict(conflicts, "a", "d"));
			Assert.True(BrandesKopf.HasConflict(conflicts, "b", "c"));
		}

		[Fact]
		public void HasConflictCanTestForAType1ConflictRegardlessOfEdgeOrientation()
		{
			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			BrandesKopf.AddConflict(conflicts, "b", "a");

			Assert.True(BrandesKopf.HasConflict(conflicts, "a", "b"));
			Assert.True(BrandesKopf.HasConflict(conflicts, "b", "a"));
		}

		[Fact]
		public void HasConflictWorksForMultipleConflictsWithTheSameNode()
		{
			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			BrandesKopf.AddConflict(conflicts, "b", "a");
			BrandesKopf.AddConflict(conflicts, "a", "c");

			Assert.True(BrandesKopf.HasConflict(conflicts, "a", "b"));
			Assert.True(BrandesKopf.HasConflict(conflicts, "a", "c"));
		}

		[Fact]
		public void VerticalAlignmentAlignsWithItselfIfTheNodeHasNoAdjacencies()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; });

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(2, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("b", result.Root["b"]);

			Assert.Equal(2, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.Equal("a", result.Align["a"]);
			Assert.Equal("b", result.Align["b"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsWithItsSoleAdjacency()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "b");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(2, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("a", result.Root["b"]);

			Assert.Equal(2, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.Equal("b", result.Align["a"]);
			Assert.Equal("a", result.Align["b"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsWithItsLeftMedianWhenPossible()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "c");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(3, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("b", result.Root["b"]);
			Assert.Equal("a", result.Root["c"]);

			Assert.Equal(3, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.Equal("c", result.Align["a"]);
			Assert.Equal("b", result.Align["b"]);
			Assert.Equal("a", result.Align["c"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsCorrectlyEvenRegardlessOfNodeNameInsertionOrder()
		{
			// This test ensures that we're actually properly sorting nodes by
			// position when searching for candidates. Many of these tests previously
			// passed because the node insertion order matched the order of the nodes
			// in the layering.
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 1; });
			g.SetNode("z", n => { n.Rank = 0; n.Order = 0; });
			g.SetEdge("z", "c");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(3, result.Root.Count);
			Assert.True(result.Root.ContainsKey("z"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.Equal("z", result.Root["c"]);
			Assert.Equal("b", result.Root["b"]);
			Assert.Equal("z", result.Root["z"]);

			Assert.Equal(3, result.Align.Count);
			Assert.True(result.Align.ContainsKey("z"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.Equal("c", result.Align["z"]);
			Assert.Equal("b", result.Align["b"]);
			Assert.Equal("z", result.Align["c"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsWithItsRightMedianWhenLeftIsUnavailable()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "c");
			g.SetEdge("b", "c");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			BrandesKopf.AddConflict(conflicts, "a", "c");
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(3, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("b", result.Root["b"]);
			Assert.Equal("b", result.Root["c"]);

			Assert.Equal(3, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.Equal("a", result.Align["a"]);
			Assert.Equal("c", result.Align["b"]);
			Assert.Equal("b", result.Align["c"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsWithNeitherMedianIfBothAreUnavailable()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			g.SetEdge("a", "d");
			g.SetEdge("b", "c");
			g.SetEdge("b", "d");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			// c will align with b, so d will not be able to align with a, because
			// (a,d) and (c,b) cross.
			Assert.Equal(4, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.True(result.Root.ContainsKey("d"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("b", result.Root["b"]);
			Assert.Equal("b", result.Root["c"]);
			Assert.Equal("d", result.Root["d"]);

			Assert.Equal(4, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.True(result.Align.ContainsKey("d"));
			Assert.Equal("a", result.Align["a"]);
			Assert.Equal("c", result.Align["b"]);
			Assert.Equal("b", result.Align["c"]);
			Assert.Equal("d", result.Align["d"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsWithTheSingleMedianForAnOddNumberOfAdjacencies()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 0; n.Order = 2; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 0; });
			g.SetEdge("a", "d");
			g.SetEdge("b", "d");
			g.SetEdge("c", "d");

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(4, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.True(result.Root.ContainsKey("d"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("b", result.Root["b"]);
			Assert.Equal("c", result.Root["c"]);
			Assert.Equal("b", result.Root["d"]);

			Assert.Equal(4, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.True(result.Align.ContainsKey("d"));
			Assert.Equal("a", result.Align["a"]);
			Assert.Equal("d", result.Align["b"]);
			Assert.Equal("c", result.Align["c"]);
			Assert.Equal("b", result.Align["d"]);
		}

		[Fact]
		public void VerticalAlignmentAlignsBlocksAcrossMultipleLayers()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 1; });
			g.SetNode("d", n => { n.Rank = 2; n.Order = 0; });
			g.SetPath(["a", "b", "d"]);
			g.SetPath(["a", "c", "d"]);

			var layering = Util.BuildLayerMatrix(g);

			var conflicts = new Dictionary<string, Dictionary<string, bool>>();
			var result = BrandesKopf.VerticalAlign(layering, conflicts, g.GetPredecessorsInternal);

			Assert.Equal(4, result.Root.Count);
			Assert.True(result.Root.ContainsKey("a"));
			Assert.True(result.Root.ContainsKey("b"));
			Assert.True(result.Root.ContainsKey("c"));
			Assert.True(result.Root.ContainsKey("d"));
			Assert.Equal("a", result.Root["a"]);
			Assert.Equal("a", result.Root["b"]);
			Assert.Equal("c", result.Root["c"]);
			Assert.Equal("a", result.Root["d"]);

			Assert.Equal(4, result.Align.Count);
			Assert.True(result.Align.ContainsKey("a"));
			Assert.True(result.Align.ContainsKey("b"));
			Assert.True(result.Align.ContainsKey("c"));
			Assert.True(result.Align.ContainsKey("d"));
			Assert.Equal("b", result.Align["a"]);
			Assert.Equal("d", result.Align["b"]);
			Assert.Equal("c", result.Align["c"]);
			Assert.Equal("a", result.Align["d"]);
		}

		[Fact]
		public void HorizontalCompactionPlacesTheCenterOfASingleNodeGraphAtOrigin()
		{
			var root = new Dictionary<string, string>() { { "a", "a" } };
			var align = new Dictionary<string, string>() { { "a", "a" } };
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0.0, xs["a"]);
		}

		[Fact]
		public void HorizontalCompactionSeparatesAdjacentNodesBySpecifiedNodeSeparation()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
			g.Options.NodeSeparation = 100;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0.0, xs["a"]);
			Assert.Equal(100/2+100+200/2, xs["b"]);
		}

		[Fact]
		public void HorizontalCompactionSeparatesAdjacentEdgesBySpecifiedNodeSeparation()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
			g.Options.EdgeSeparation = 20;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; n.DummyType = DummyType.Edge; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; n.DummyType = DummyType.Edge; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0.0, xs["a"]);
			Assert.Equal(100/2+20+200/2, xs["b"]);
		}

		[Fact]
		public void HorizontalCompactionAlignsTheCentersOfNodesInTheSameBlock()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "a" } };
			var align = new Dictionary<string, string>() { { "a", "b" }, { "b", "a" } };
			g = new Graph();
			g.Options.EdgeSeparation = 20;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 1; n.Width = 200; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0.0, xs["a"]);
			Assert.Equal(0.0, xs["b"]);
		}

		[Fact]
		public void HorizontalCompactionSeparatesBlocksWithTheAppropriateSeparation()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "a" }, { "c", "c" } };
			var align = new Dictionary<string, string>() { { "a", "b" }, { "b", "a" }, { "c", "c" } };
			g.Options.NodeSeparation = 75;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 1; n.Width = 200; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 50; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(50/2+75+200/2, xs["a"]);
			Assert.Equal(50/2+75+200/2, xs["b"]);
			Assert.Equal(0.0, xs["c"]);
		}

		[Fact]
		public void HorizontalCompactionSeparatesClassesWithTheAppropriateSeparation()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" }, { "d", "b" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "d" }, { "c", "c" }, { "d", "b" } };
			g.Options.NodeSeparation = 75;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 50; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 80; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0.0, xs["a"]);
			Assert.Equal(100 / 2 + 75 + 200 / 2, xs["b"]);
			Assert.Equal(100/2+75+200/2-80/2-75-50/2, xs["c"]);
			Assert.Equal(100/2+75+200/2, xs["d"]);
		}

		[Fact]
		public void HorizontalCompactionShiftsClassesByMaxSepFromTheAdjacentBlock1()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "a" }, { "d", "b" } };
			var align = new Dictionary<string, string>() { { "a", "c" }, { "b", "d" }, { "c", "a" }, { "d", "b" } };
			g.Options.NodeSeparation = 75;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 50; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 150; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 60; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 70; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0, xs["a"]);
			Assert.Equal(50/2+75+150/2, xs["b"]);
			Assert.Equal(0, xs["c"]);
			Assert.Equal(50/2+75+150/2, xs["d"]);
		}

		[Fact]
		public void HorizontalCompactionShiftsClassesByMaxSepFromTheAdjacentBlock2()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "a" }, { "d", "b" } };
			var align = new Dictionary<string, string>() { { "a", "c" }, { "b", "d" }, { "c", "a" }, { "d", "b" } };
			g.Options.NodeSeparation = 75;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 50; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 70; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 60; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 150; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0, xs["a"]);
			Assert.Equal(60/2+75+150/2, xs["b"]);
			Assert.Equal(0, xs["c"]);
			Assert.Equal(60/2+75+150/2, xs["d"]);
		}

		[Fact]
		public void HorizontalCompactionCascadesClassShift()
		{
			var root = new Dictionary<string, string>()
			{
				{ "a", "a" },
				{ "b", "b" },
				{ "c", "c" },
				{ "d", "d" },
				{ "e", "b" },
				{ "f", "f" },
				{ "g", "d" }
			};
			var align = new Dictionary<string, string>()
			{
				{ "a", "a" },
				{ "b", "e" },
				{ "c", "c" },
				{ "d", "g" },
				{ "e", "b" },
				{ "f", "f" },
				{ "g", "d" }
			};

			g = new Graph();
			g.Options.NodeSeparation = 75;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 50; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 50; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 50; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 50; });
			g.SetNode("e", n => { n.Rank = 1; n.Order = 2; n.Width = 50; });
			g.SetNode("f", n => { n.Rank = 2; n.Order = 0; n.Width = 50; });
			g.SetNode("g", n => { n.Rank = 2; n.Order = 1; n.Width = 50; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			// Use f as 0, everything is relative to it
			Assert.Equal(xs["b"]-50/2-75-50/2, xs["a"]);
			Assert.Equal(xs["e"], xs["b"]);
			Assert.Equal(xs["f"], xs["c"]);
			Assert.Equal(xs["c"]+50/2+75+50/2, xs["d"]);
			Assert.Equal(xs["d"] + 50 / 2 + 75 + 50 / 2, xs["e"]);
			Assert.Equal(xs["f"] + 50 / 2 + 75 + 50 / 2, xs["g"]);
		}

		[Fact]
		public void HorizontalCompactionHandlesLabelposL()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			g = new Graph();
			g.Options.EdgeSeparation = 50;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; n.DummyType = DummyType.Edge; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; n.DummyType = DummyType.EdgeLabel; n.LabelPosition = LabelPosition.Left; });
			g.SetNode("c", n => { n.Rank = 0; n.Order = 2; n.Width = 300; n.DummyType = DummyType.Edge; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0, xs["a"]);
			Assert.Equal(xs["a"]+100/2+50+200, xs["b"]);
			Assert.Equal(xs["b"]+0+50+300/2, xs["c"]);
		}

		[Fact]
		public void HorizontalCompactionHandlesLabelposC()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			g.Options.EdgeSeparation = 50;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; n.DummyType = DummyType.Edge; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; n.DummyType = DummyType.EdgeLabel; n.LabelPosition = LabelPosition.Center; });
			g.SetNode("c", n => { n.Rank = 0; n.Order = 2; n.Width = 300; n.DummyType = DummyType.Edge; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0, xs["a"]);
			Assert.Equal(xs["a"] + 100 / 2 + 50 + 200/2, xs["b"]);
			Assert.Equal(xs["b"] + 200/2 + 50 + 300 / 2, xs["c"]);
		}

		[Fact]
		public void HorizontalCompactionHandlesLabelposr()
		{
			var root = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			var align = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
			g.Options.EdgeSeparation = 50;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; n.DummyType = DummyType.Edge; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 200; n.DummyType = DummyType.EdgeLabel; n.LabelPosition = LabelPosition.Right; });
			g.SetNode("c", n => { n.Rank = 0; n.Order = 2; n.Width = 300; n.DummyType = DummyType.Edge; });

			var xs = BrandesKopf.HorizontalCompaction(g, Util.BuildLayerMatrix(g), root, align);

			Assert.Equal(0, xs["a"]);
			Assert.Equal(xs["a"] + 100 / 2 + 50 + 0, xs["b"]);
			Assert.Equal(xs["b"] + 200 + 50 + 300 / 2, xs["c"]);
		}

		[Fact]
		public void AlignCoordinatesAlignsASingleNode()
		{
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 50 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 100 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 50 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 200 } } }
			};

			BrandesKopf.AlignCoordinates(xss, xss[GraphAlignment.UpLeft]);

			Assert.Single(xss[GraphAlignment.UpLeft]);
			Assert.True(xss[GraphAlignment.UpLeft].ContainsKey("a") && xss[GraphAlignment.UpLeft]["a"] == 50);
			Assert.Single(xss[GraphAlignment.UpRight]);
			Assert.True(xss[GraphAlignment.UpRight].ContainsKey("a") && xss[GraphAlignment.UpRight]["a"] == 50);
			Assert.Single(xss[GraphAlignment.DownLeft]);
			Assert.True(xss[GraphAlignment.DownLeft].ContainsKey("a") && xss[GraphAlignment.DownLeft]["a"] == 50);
			Assert.Single(xss[GraphAlignment.DownRight]);
			Assert.True(xss[GraphAlignment.DownRight].ContainsKey("a") && xss[GraphAlignment.DownRight]["a"] == 50);
		}

		[Fact]
		public void AlignCoordinatesAlignsMultipleNodes()
		{
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 50 }, { "b", 1000 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 100 }, { "b", 900 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 150 }, { "b", 800 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 200 }, { "b", 700 } } }
			};

			BrandesKopf.AlignCoordinates(xss, xss[GraphAlignment.UpLeft]);

			Assert.Equal(2, xss[GraphAlignment.UpLeft].Count);
			Assert.True(xss[GraphAlignment.UpLeft].ContainsKey("a") && xss[GraphAlignment.UpLeft]["a"] == 50);
			Assert.True(xss[GraphAlignment.UpLeft].ContainsKey("b") && xss[GraphAlignment.UpLeft]["b"] == 1000);
			Assert.Equal(2, xss[GraphAlignment.UpRight].Count);
			Assert.True(xss[GraphAlignment.UpRight].ContainsKey("a") && xss[GraphAlignment.UpRight]["a"] == 200);
			Assert.True(xss[GraphAlignment.UpRight].ContainsKey("b") && xss[GraphAlignment.UpRight]["b"] == 1000);
			Assert.Equal(2, xss[GraphAlignment.DownLeft].Count);
			Assert.True(xss[GraphAlignment.DownLeft].ContainsKey("a") && xss[GraphAlignment.DownLeft]["a"] == 50);
			Assert.True(xss[GraphAlignment.DownLeft].ContainsKey("b") && xss[GraphAlignment.DownLeft]["b"] == 700);
			Assert.Equal(2, xss[GraphAlignment.DownRight].Count);
			Assert.True(xss[GraphAlignment.DownRight].ContainsKey("a") && xss[GraphAlignment.DownRight]["a"] == 500);
			Assert.True(xss[GraphAlignment.DownRight].ContainsKey("b") && xss[GraphAlignment.DownRight]["b"] == 1000);
		}

		[Fact]
		public void FindSmallestWidthAlignmentFindsTheAlignmentWithTheSmallestWidth()
		{
			g.SetNode("a", n => { n.Width = 50; });
			g.SetNode("b", n => { n.Width = 50; });

			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 0 }, { "b", 1000 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", -5 }, { "b", 1000 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 5 }, { "b", 2000 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 0 }, { "b", 200 } } }
			};

			var result = BrandesKopf.FindSmallestWidthAlignment(g, xss);

			Assert.Equal(xss[GraphAlignment.DownRight], result);
		}

		[Fact]
		public void FindSmallestWidthAlignmentTakesNodeWidthIntoAccount()
		{
			g.SetNode("a", n => { n.Width = 50; });
			g.SetNode("b", n => { n.Width = 50; });
			g.SetNode("c", n => { n.Width = 200; });

			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 0 }, { "b", 100 }, { "c", 75 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 0 }, { "b", 100 }, { "c", 80 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 0 }, { "b", 100 }, { "c", 85 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 0 }, { "b", 100 }, { "c", 90 } } }
			};

			var result = BrandesKopf.FindSmallestWidthAlignment(g, xss);

			Assert.Equal(xss[GraphAlignment.UpLeft], result);
		}

		[Fact]
		public void BalanceAlignsASingleNodeToTheSharedMedianValue()
		{
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 0 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 100 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 100 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 200 } } }
			};

			var result = BrandesKopf.Balance(xss);

			Assert.Single(result);
			Assert.True(result.ContainsKey("a") && result["a"] == 100);
		}

		[Fact]
		public void BalanceAlignsASingleNodeToTheAverageOfDifferentMedianValues()
		{
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 0 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 75 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 125 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 200 } } }
			};

			var result = BrandesKopf.Balance(xss);

			Assert.Single(result);
			Assert.True(result.ContainsKey("a") && result["a"] == 100);
		}

		[Fact]
		public void BalanceBalancesMultipleNodes()
		{
			var xss = new Dictionary<GraphAlignment, Dictionary<string, double>>()
			{
				{ GraphAlignment.UpLeft, new Dictionary<string, double>() { {"a", 0 }, { "b", 50 } } },
				{ GraphAlignment.UpRight, new Dictionary<string, double>() { {"a", 75 }, { "b", 0 } } },
				{ GraphAlignment.DownLeft, new Dictionary<string, double>() { {"a", 125 }, { "b", 60 } } },
				{ GraphAlignment.DownRight, new Dictionary<string, double>() { {"a", 200 }, { "b", 75 } } }
			};

			var result = BrandesKopf.Balance(xss);

			Assert.Equal(2, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == 100);
			Assert.True(result.ContainsKey("b") && result["b"] == 55);
		}

		[Fact]
		public void PositionXPositionsASingleNodeAtOrigin()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });

			var result = BrandesKopf.PositionX(g);

			Assert.Single(result);
			Assert.True(result.ContainsKey("a") && result["a"] == 0);
		}

		[Fact]
		public void PositionXPositionsASingleNodeBlockAtOrigin()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 100; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; n.Width = 100; });
			g.SetEdge("a", "b");

			var result = BrandesKopf.PositionX(g);

			Assert.Equal(2, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == 0);
			Assert.True(result.ContainsKey("b") && result["b"] == 0);
		}

		[Fact]
		public void PositionXPositionsASingleNodeBlockAtOriginEvenWhenTheirSizesDiffer()
		{
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 40; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; n.Width = 500; });
			g.SetNode("c", n => { n.Rank = 2; n.Order = 0; n.Width = 20; });
			g.SetPath(["a", "b", "c"]);

			var result = BrandesKopf.PositionX(g);

			Assert.Equal(3, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == 0);
			Assert.True(result.ContainsKey("b") && result["b"] == 0);
			Assert.True(result.ContainsKey("c") && result["c"] == 0);
		}

		[Fact]
		public void PositionXCentersANodeIfItIsAPredecessorOfTwoSameSizedNodes()
		{
			g.Options.NodeSeparation = 10;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 20; });
			g.SetNode("b", n => { n.Rank = 1; n.Order = 0; n.Width = 50; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 1; n.Width = 50; });
			g.SetEdge("a", "b");
			g.SetEdge("a", "c");

			var result = BrandesKopf.PositionX(g);
			var a = result["a"];

			Assert.Equal(3, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == a);
			Assert.True(result.ContainsKey("b") && result["b"] == a-(25+5));
			Assert.True(result.ContainsKey("c") && result["c"] == a+(25+5));
		}

		[Fact]
		public void PositionXShiftsBlocksOnBothSidesOfAlignedBlock()
		{
			g.Options.NodeSeparation = 10;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 50; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 60; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 70; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 80; });
			g.SetEdge("b", "c");

			var result = BrandesKopf.PositionX(g);
			var b = result["b"];
			var c = b;

			Assert.Equal(4, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == b - 60 / 2 - 10 - 50 / 2);
			Assert.True(result.ContainsKey("b") && result["b"] == b);
			Assert.True(result.ContainsKey("c") && result["c"] == c);
			Assert.True(result.ContainsKey("d") && result["d"] == c + 70 / 2 + 10 + 80 / 2);
		}

		[Fact]
		public void PositionXAlignsInnerSegments()
		{
			g.Options.NodeSeparation = 10;
			g.Options.EdgeSeparation = 10;
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; n.Width = 50; n.DummyType = DummyType.Edge; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; n.Width = 60; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; n.Width = 70; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; n.Width = 80; n.DummyType = DummyType.Edge; });
			g.SetEdge("b", "c");
			g.SetEdge("a", "d");

			var result = BrandesKopf.PositionX(g);
			var a = result["a"];
			var d = a;

			Assert.Equal(4, result.Count);
			Assert.True(result.ContainsKey("a") && result["a"] == a);
			Assert.True(result.ContainsKey("b") && result["b"] == a + 50 / 2 + 10 + 60 / 2);
			Assert.True(result.ContainsKey("c") && result["c"] == d - 70 / 2 - 10 - 80 / 2);
			Assert.True(result.ContainsKey("d") && result["d"] == d);
		}
	}
}
