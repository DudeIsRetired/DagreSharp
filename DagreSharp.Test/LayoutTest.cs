using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class LayoutTest
	{
		private readonly Graph g = new(true, true, true);
		private readonly Dagre dagre;

		public LayoutTest()
		{
			dagre = new Dagre(g);
		}

		[Fact]
		public void CanLayoutASingleNode()
		{
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; });

			dagre.Layout();

			var coords = ExtractCoordinates();
			Assert.Single(coords);

			var node = g.GetNode("a");
			Assert.Equal(50 / 2, node.X);
			Assert.Equal(100 / 2, node.Y);
		}

		[Fact]
		public void CanLayoutTwoNodesOnTheSameRank()
		{
			dagre.Options.NodeSeparation = 200;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 75; n.Height = 200; });

			dagre.Layout();

			AssertCoordinates(new Dictionary<string, Point>
			{
				{ "a", new Point(50/2, 200/2) },
				{ "b", new Point(50+200+(double)75/2, 200/2) }
			});
		}

		[Fact]
		public void CanLayoutTwoNodesConnectedByAnEdge()
		{
			dagre.Options.RankSeparation = 300;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 75; n.Height = 200; });
			g.SetEdge("a", "b");

			dagre.Layout();

			AssertCoordinates(new Dictionary<string, Point>
			{
				{ "a", new Point((double)75/2, 100/2) },
				{ "b", new Point((double)75/2, 100+300+200/2) }
			});

			// We should not get x, y coordinates if the edge has no label
			var edge = g.GetEdge("a", "b");
			Assert.False(edge.X.HasValue);
			Assert.False(edge.Y.HasValue);
		}

		[Fact]
		public void CanLayoutAnWdgeWithALabel()
		{
			dagre.Options.RankSeparation = 300;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 75; n.Height = 200; });
			g.SetEdge("a", "b", null, e => { e.Width = 60; e.Height = 70; e.LabelPosition = LabelPosition.Center; });

			dagre.Layout();

			AssertCoordinates(new Dictionary<string, Point>
			{
				{ "a", new Point((double)75/2, 100/2) },
				{ "b", new Point((double)75/2, 100+150+70+150+200/2) }
			});

			var edge = g.GetEdge("a", "b");
			Assert.Equal((double)75/2, edge.X);
			Assert.Equal(100+150+70/2, edge.Y);
		}

		[Fact]
		public void CanLayoutAnEdgeWithALongLabelWithRankdirTB()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.TopBottom;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "c", null, e => { e.Width = 2000; e.Height = 10; e.LabelPosition = LabelPosition.Center; });
			g.SetEdge("b", "d", null, e => { e.Width = 1; e.Height = 1; });

			dagre.Layout();

			var p1 = g.GetEdge("a", "c");
			var p2 = g.GetEdge("b", "d");

			Assert.True(p1.X.HasValue && p2.X.HasValue);
			Assert.True(Math.Abs(p1.X.Value - p2.X.Value) > 1000);
		}

		[Fact]
		public void CanLayoutAnEdgeWithALongLabelWithRankdirBT()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.BottomTop;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "c", null, e => { e.Width = 2000; e.Height = 10; e.LabelPosition = LabelPosition.Center; });
			g.SetEdge("b", "d", null, e => { e.Width = 1; e.Height = 1; });

			dagre.Layout();

			var p1 = g.GetEdge("a", "c");
			var p2 = g.GetEdge("b", "d");

			Assert.True(p1.X.HasValue && p2.X.HasValue);
			Assert.True(Math.Abs(p1.X.Value - p2.X.Value) > 1000);
		}

		[Fact]
		public void CanLayoutAnEdgeWithALongLabelWithRankdirLR()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.LeftRight;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "c", null, e => { e.Width = 2000; e.Height = 10; e.LabelPosition = LabelPosition.Center; });
			g.SetEdge("b", "d", null, e => { e.Width = 1; e.Height = 1; });

			dagre.Layout();

			var p1 = g.GetNode("a");
			var p2 = g.GetNode("c");

			Assert.True(Math.Abs(p1.X - p2.X) > 1000);
		}

		[Fact]
		public void CanLayoutAnEdgeWithALongLabelWithRankdirRL()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.RightLeft;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "c", null, e => { e.Width = 2000; e.Height = 10; e.LabelPosition = LabelPosition.Center; });
			g.SetEdge("b", "d", null, e => { e.Width = 1; e.Height = 1; });

			dagre.Layout();

			var p1 = g.GetNode("a");
			var p2 = g.GetNode("c");

			Assert.True(p1.X - p2.X > 1000);
		}

		[Fact]
		public void CanApplyAnOffsetWithRankdirTB()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.TopBottom;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Left; e.LabelOffset = 1000; });
			g.SetEdge("c", "d", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Right; e.LabelOffset = 1000; });

			dagre.Layout();

			var abEdge = g.GetEdge("a", "b");
			var cdEdge = g.GetEdge("c", "d");
			Assert.Equal(-1000-10/2, abEdge.X - abEdge.Points[0].X);
			Assert.Equal(1000+10/2, cdEdge.X - cdEdge.Points[0].X);
		}

		[Fact]
		public void CanApplyAnOffsetWithRankdirBT()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.BottomTop;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Left; e.LabelOffset = 1000; });
			g.SetEdge("c", "d", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Right; e.LabelOffset = 1000; });

			dagre.Layout();

			var abEdge = g.GetEdge("a", "b");
			var cdEdge = g.GetEdge("c", "d");
			Assert.Equal(-1000 - 10 / 2, abEdge.X - abEdge.Points[0].X);
			Assert.Equal(1000 + 10 / 2, cdEdge.X - cdEdge.Points[0].X);
		}

		[Fact]
		public void CanApplyAnOffsetWithRankdirLR()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.LeftRight;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Left; e.LabelOffset = 1000; });
			g.SetEdge("c", "d", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Right; e.LabelOffset = 1000; });

			dagre.Layout();

			var abEdge = g.GetEdge("a", "b");
			var cdEdge = g.GetEdge("c", "d");
			Assert.Equal(-1000 - 10 / 2, abEdge.Y - abEdge.Points[0].Y);
			Assert.Equal(1000 + 10 / 2, cdEdge.Y - cdEdge.Points[0].Y);
		}

		[Fact]
		public void CanApplyAnOffsetWithRankdirRL()
		{
			dagre.Options.NodeSeparation = 10;
			dagre.Options.EdgeSeparation = 10;
			dagre.Options.RankDirection = RankDirection.RightLeft;
			g.SetNode("a", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("b", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("c", n => { n.Width = 10; n.Height = 10; });
			g.SetNode("d", n => { n.Width = 10; n.Height = 10; });

			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Left; e.LabelOffset = 1000; });
			g.SetEdge("c", "d", null, e => { e.Width = 10; e.Height = 10; e.LabelPosition = LabelPosition.Right; e.LabelOffset = 1000; });

			dagre.Layout();

			var abEdge = g.GetEdge("a", "b");
			var cdEdge = g.GetEdge("c", "d");
			Assert.Equal(-1000 - 10 / 2, abEdge.Y - abEdge.Points[0].Y);
			Assert.Equal(1000 + 10 / 2, cdEdge.Y - cdEdge.Points[0].Y);
		}

		[Fact]
		public void CanLayoutALongEdgeWithALabel()
		{
			dagre.Options.RankSeparation = 300;
			g.SetNode("a", n => { n.Width = 50; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 75; n.Height = 200; });
			g.SetEdge("a", "b", null, e => { e.Width = 60; e.Height = 70; e.MinLength = 2; e.LabelPosition = LabelPosition.Center; });

			dagre.Layout();

			var abEdge = g.GetEdge("a", "b");
			Assert.Equal((double)75 / 2, abEdge.X);
			Assert.True(abEdge.Y > g.GetNode("a").Y);
			Assert.True(abEdge.Y < g.GetNode("b").Y);
		}

		[Fact]
		public void CanLayoutAShortCycle()
		{
			dagre.Options.RankSeparation = 200;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "b", null, e => { e.Weight = 2; });
			g.SetEdge("b", "a");

			dagre.Layout();

			AssertCoordinates(new Dictionary<string, Point>
			{
				{ "a", new Point(100/2, 100/2) },
				{ "b", new Point(100/2, 100+200+100/2) }
			});

			// One arrow should point down, one up
			var abEdge = g.GetEdge("a", "b");
			var baEdge = g.GetEdge("b", "a");
			Assert.True(abEdge.Points[1].Y > abEdge.Points[0].Y);
			Assert.True(baEdge.Points[0].Y > baEdge.Points[1].Y);
		}

		[Fact]
		public void AddsRectangleIntersectsForEdges()
		{
			dagre.Options.RankSeparation = 200;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "b");

			dagre.Layout();

			var points = g.GetEdge("a", "b").Points;
			Assert.Equal(3, points.Count);

			Assert.Equal(100/2, points[0].X);
			Assert.Equal(100, points[0].Y);
			Assert.Equal(100/2, points[1].X);
			Assert.Equal(100+200/2, points[1].Y);
			Assert.Equal(100 / 2, points[2].X);
			Assert.Equal(100 + 200, points[2].Y);
		}

		[Fact]
		public void AddsRectangleIntersectsForEdgesSpanningMultipleRanks()
		{
			dagre.Options.RankSeparation = 200;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "b", null, e => { e.MinLength = 2; });

			dagre.Layout();

			var points = g.GetEdge("a", "b").Points;
			Assert.Equal(5, points.Count);

			Assert.Equal(100/2, points[0].X);
			Assert.Equal(100, points[0].Y);
			Assert.Equal(100/2, points[1].X);
			Assert.Equal(100+200/2, points[1].Y);
			Assert.Equal(100/2, points[2].X);
			Assert.Equal(100+400/2, points[2].Y);
			Assert.Equal(100/2, points[3].X);
			Assert.Equal(100+600/2, points[3].Y);
			Assert.Equal(100/2, points[4].X);
			Assert.Equal(100+800/2, points[4].Y);
		}

		[Fact]
		public void CanLayoutASelfLoopTB()
		{
			dagre.Options.EdgeSeparation = 75;
			dagre.Options.RankDirection = RankDirection.TopBottom;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "a", null, e => { e.Width = 50; e.Height = 50; });

			dagre.Layout();

			var nodeA = g.GetNode("a");
			var points = g.GetEdge("a", "a").Points;
			
			Assert.Equal(7, points.Count);

			foreach (var point in points)
			{
				Assert.True(point.X > nodeA.X);
				Assert.True(Math.Abs(point.Y - nodeA.Y) <= nodeA.Width/2);
			}
		}

		[Fact]
		public void CanLayoutASelfLoopBT()
		{
			dagre.Options.EdgeSeparation = 75;
			dagre.Options.RankDirection = RankDirection.BottomTop;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "a", null, e => { e.Width = 50; e.Height = 50; });

			dagre.Layout();

			var nodeA = g.GetNode("a");
			var points = g.GetEdge("a", "a").Points;

			Assert.Equal(7, points.Count);

			foreach (var point in points)
			{
				Assert.True(point.X > nodeA.X);
				Assert.True(Math.Abs(point.Y - nodeA.Y) <= nodeA.Height / 2);
			}
		}

		[Fact]
		public void CanLayoutASelfLoopLR()
		{
			dagre.Options.EdgeSeparation = 75;
			dagre.Options.RankDirection = RankDirection.LeftRight;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "a", null, e => { e.Width = 50; e.Height = 50; });

			dagre.Layout();

			var nodeA = g.GetNode("a");
			var points = g.GetEdge("a", "a").Points;

			Assert.Equal(7, points.Count);

			foreach (var point in points)
			{
				Assert.True(point.Y > nodeA.Y);
				Assert.True(Math.Abs(point.X - nodeA.X) <= nodeA.Height / 2);
			}
		}

		[Fact]
		public void CanLayoutASelfLoopRL()
		{
			dagre.Options.EdgeSeparation = 75;
			dagre.Options.RankDirection = RankDirection.RightLeft;
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "a", null, e => { e.Width = 50; e.Height = 50; });

			dagre.Layout();

			var nodeA = g.GetNode("a");
			var points = g.GetEdge("a", "a").Points;

			Assert.Equal(7, points.Count);

			foreach (var point in points)
			{
				Assert.True(point.Y > nodeA.Y);
				Assert.True(Math.Abs(point.X - nodeA.X) <= nodeA.Height / 2);
			}
		}

		//[Fact]
		//public void CanLayoutAGraphWithSubgraphs()
		//{
		//	// To be expanded, this primarily ensures nothing blows up for the moment.
		//	g.SetNode("a", n => { n.Width = 50; n.Height = 50; });
		//	g.SetParent("a", "sg1");
		//	dagre.Layout();
		//}

		[Fact]
		public void MinimizesTheHeightIfSubgraphs()
		{
			foreach (var v in new[] { "a", "b", "c", "d", "x", "y" })
			{
				g.SetNode(v, n => { n.Width = 50; n.Height = 50; });
			}
			
			g.SetPath(["a", "b", "c", "d"]);
			g.SetEdge("a", "x", null, e => { e.Width = 100; });
			g.SetEdge("y", "d", null, e => { e.Width = 100; });
			g.SetParent("x", "sg");
			g.SetParent("y", "sg");

			// We did not set up an edge (x, y), and we set up high-weight edges from
			// outside of the subgraph to nodes in the subgraph. This is to try to
			// force nodes x and y to be on different ranks, which we want our ranker
			// to avoid.
			dagre.Layout();

			Assert.Equal(g.GetNode("x").Y, g.GetNode("y").Y);
		}

		[Fact]
		public void MinimizesSeparationBetweenNodesNotAdjacentToSubgraphs()
		{
			foreach (var v in new[] { "a", "b", "c" })
			{
				g.SetNode(v, n => { n.Width = 50; n.Height = 50; });
			}

			g.SetPath(["a", "b", "c"]);
			g.SetNode("sg");
			g.SetParent("c", "sg");

			dagre.Layout();

			Assert.Equal(100, g.GetNode("b").Y - g.GetNode("a").Y);
		}

		[Fact]
		public void CanLayoutSubgraphsWithDifferentRankdirs()
		{
			g.SetNode("a", n => { n.Width = 50; n.Height = 50; });
			g.SetNode("sg");
			g.SetParent("a", "sg");

			void Check()
			{
				Assert.True(g.GetNode("sg").Width > 50);
				Assert.True(g.GetNode("sg").Height > 50);
				Assert.True(g.GetNode("sg").X > 50/2);
				Assert.True(g.GetNode("sg").Y > 50 / 2);
			}

			foreach (RankDirection rankdir in Enum.GetValues(typeof(RankDirection)))
			{
				if (rankdir == RankDirection.None)
				{
					continue;
				}

				dagre.Options.RankDirection = rankdir;
				dagre.Layout();
				Check();
			}
		}

		[Fact]
		public void AddsDimensionsToTheGraph()
		{
			g.SetNode("a", n => { n.Width = 100; n.Height = 50; });

			dagre.Layout();

			Assert.Equal(100, dagre.Options.Width);
			Assert.Equal(50, dagre.Options.Height);
		}

		[Fact]
		public void EnsuresAllCoordinatesAreInTheBoundingBoxForTheGraphNodes()
		{
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; });

			foreach (RankDirection rankdir in Enum.GetValues(typeof(RankDirection)))
			{
				dagre.Options.RankDirection = rankdir;
				dagre.Layout();
				
				var node = g.GetNode("a");
				Assert.Equal(100/2, node.X);
				Assert.Equal(200/2, node.Y);
			}
		}

		[Fact]
		public void EnsuresAllCoordinatesAreInTheBoundingBoxForTheGraphEdges()
		{
			g.SetNode("a", n => { n.Width = 100; n.Height = 100; });
			g.SetNode("b", n => { n.Width = 100; n.Height = 100; });
			g.SetEdge("a", "b", null, e => { e.Width = 1000; e.Height = 2000; e.LabelPosition = LabelPosition.Left; e.LabelOffset = 0; });

			foreach (RankDirection rankdir in Enum.GetValues(typeof(RankDirection)))
			{
				if (rankdir == RankDirection.None)
				{
					continue;
				}

				dagre.Options.RankDirection = rankdir;
				dagre.Layout();

				var edge = g.GetEdge("a", "b");

				if (rankdir == RankDirection.TopBottom || rankdir == RankDirection.BottomTop)
				{
					Assert.Equal(1000/2, edge.X);
				}
				else
				{
					Assert.Equal(2000/2, edge.Y);
				}
			}
		}

		private void AssertCoordinates(Dictionary<string, Point> expected)
		{
			var coords = ExtractCoordinates();
			Assert.Equal(expected.Count, coords.Count);

			foreach (var kvp in expected)
			{
				Assert.True(coords.ContainsKey(kvp.Key));
				var value = coords[kvp.Key];
				Assert.Equal(kvp.Value.X, value.X);
				Assert.Equal(kvp.Value.Y, value.Y);
			}
		}

		private Dictionary<string, Point> ExtractCoordinates()
		{
			var result = new Dictionary<string, Point>();

			foreach (var node in g.Nodes)
			{
				result.Add(node.Id, new Point(node.X, node.Y));
			}

			return result;
		}
	}
}
