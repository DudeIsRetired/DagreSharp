using DagreSharp.GraphLibrary;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;

namespace DagreSharp.Test
{
	public class UtilTest
	{
		[Fact]
		public void SimplifyCopiesWithoutChangeAGraphWithNoMultiEdges()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", null, e => { e.Weight = 1; e.MinLength = 1; });

			var g2 = Util.Simplify(g);

			var edge = g2.GetEdge("a", "b");
			Assert.Equal(1, edge.Weight);
			Assert.Equal(1, edge.MinLength);
			Assert.Single(g2.Edges);
		}

		[Fact]
		public void SimplifyCollapsesMultiEdges()
		{
			var g = new Graph(true, true);
			g.SetEdge("a", "b", null, e => { e.Weight = 1; e.MinLength = 1; });
			g.SetEdge("a", "b", "multi", e => { e.Weight = 2; e.MinLength = 2; });

			var g2 = Util.Simplify(g);

			Assert.False(g2.IsMultigraph);

			var edge = g2.GetEdge("a", "b");
			Assert.Equal(3, edge.Weight);
			Assert.Equal(2, edge.MinLength);
			Assert.Single(g2.Edges);
		}

		[Fact]
		public void SimplifyCopiesTheGraphObject()
		{
			var g = new Graph(true, true);
			g.Options.NestingRoot = "bar";

			var g2 = Util.Simplify(g);

			Assert.Equal("bar", g2.Options.NestingRoot);
		}

		[Fact]
		public void AsNonCompoundGraphCopiesAllNodes()
		{
			var g = new Graph(true, true, true);
			g.SetNode("a", n => { n.Rank = 14; });
			g.SetNode("b");

			var g2 = Util.AsNonCompoundGraph(g);

			Assert.Equal(14, g2.GetNode("a").Rank);
			Assert.True(g2.HasNode("b"));
		}

		[Fact]
		public void AsNonCompoundGraphCopiesAllEdges()
		{
			var g = new Graph(true, true, true);
			g.SetEdge("a", "b", null, e => { e.LabelOffset = 14; });
			g.SetEdge("a", "b", "multi", e => { e.LabelOffset = 15; });

			var g2 = Util.AsNonCompoundGraph(g);

			Assert.Equal(14, g2.GetEdge("a", "b").LabelOffset);
			Assert.Equal(15, g2.GetEdge("a", "b", "multi").LabelOffset);
		}

		[Fact]
		public void AsNonCompoundGraphDoesNotCopyCompoundNodes()
		{
			var g = new Graph(true, true, true);
			g.SetParent("a", "sg1");

			var g2 = Util.AsNonCompoundGraph(g);

			Assert.False(g2.IsCompound);
			//expect(g2.parent(g)).toBeUndefined();
		}

		[Fact]
		public void AsNonCompoundGraphCopiesTheGraphObject()
		{
			var g = new Graph(true, true, true);
			g.Options.NestingRoot = "bar";

			var g2 = Util.AsNonCompoundGraph(g);

			Assert.Equal("bar", g2.Options.NestingRoot);
		}

		[Fact]
		public void IntersectRectCreatesASlopeThatWillIntersectTheRectanglesCenter()
		{
			var rect = new Node("foo") { X = 0, Y = 0, Width = 1, Height = 1 };
			ExpectIntersects(rect, new Point(2, 6));
			ExpectIntersects(rect, new Point(2, -6));
			ExpectIntersects(rect, new Point(6, 2));
			ExpectIntersects(rect, new Point(-6, 2));
			ExpectIntersects(rect, new Point(5, 0));
			ExpectIntersects(rect, new Point(0, 5));
		}

		[Fact]
		public void IntersectRectTouchesTheBorderOfTheRectangle()
		{
			var rect = new Node("foo") { X = 0, Y = 0, Width = 1, Height = 1 };
			ExpectTouchesBorder(rect, new Point(2, 6));
			ExpectTouchesBorder(rect, new Point(2, -6));
			ExpectTouchesBorder(rect, new Point(6, 2));
			ExpectTouchesBorder(rect, new Point(-6, 2));
			ExpectTouchesBorder(rect, new Point(5, 0));
			ExpectTouchesBorder(rect, new Point(0, 5));
		}

		private static void ExpectIntersects(Node rect, Point point)
		{
			var cross = Util.IntersectRect(rect, point);
			if (cross.X != point.X)
			{
				Assert.Equal(cross.Y - rect.Y, cross.X - rect.X, 0.9);
				//expect(cross.y - rect.y).toBeCloseTo(m * (cross.x - rect.x));
			}
		}

		private static void ExpectTouchesBorder(Node rect, Point point)
		{
			var cross = Util.IntersectRect(rect, point);
			if (Math.Abs(rect.X - cross.X) != rect.Width / 2)
			{
				Assert.Equal(rect.Height / 2, Math.Abs(rect.Y - cross.Y));
			}
		}

		[Fact]
		public void IntersectRectThrowsAnErrorIfThePointIsAtTheCenterOfTheRectangle()
		{
			var rect = new Node("foo") { X = 0, Y = 0, Width = 1, Height = 1 };

			Assert.Throws<InvalidOperationException>(() => Util.IntersectRect(rect, new Point(0,0)));
		}

		[Fact]
		public void BuildLayerMatrixCreatesAMatrixBasedOnRankAndOrderOfNodesInTheGraph()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = 0; n.Order = 0; });
			g.SetNode("b", n => { n.Rank = 0; n.Order = 1; });
			g.SetNode("c", n => { n.Rank = 1; n.Order = 0; });
			g.SetNode("d", n => { n.Rank = 1; n.Order = 1; });
			g.SetNode("e", n => { n.Rank = 2; n.Order = 0; });

			var matrix = Util.BuildLayerMatrix(g);

			Assert.Equal(3, matrix.Count);
			Assert.Contains(matrix, m => m.Contains("a") && m.Contains("b"));
			Assert.Contains(matrix, m => m.Contains("c") && m.Contains("d"));
			Assert.Contains(matrix, m => m.Contains("e"));
		}

		[Fact]
		public void NormalizeRanksAdjustRanksSuchThatAllAreGreaterOrEqual0AndAtLeastOneIs0()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = 3; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetNode("c", n => { n.Rank = 4; });

			Util.NormalizeRanks(g);

			Assert.Equal(1, g.GetNode("a").Rank);
			Assert.Equal(0, g.GetNode("b").Rank);
			Assert.Equal(2, g.GetNode("c").Rank);
		}

		[Fact]
		public void NormalizeRanksWorksForNegativeRanks()
		{
			var g = new Graph();
			g.SetNode("a", n => { n.Rank = -3; });
			g.SetNode("b", n => { n.Rank = -2; });

			Util.NormalizeRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
		}

		[Fact]
		public void NormalizeRanksDoesNotAssignARankToSubgraphs()
		{
			var g = new Graph(true, false, true);
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("sg");
			g.SetParent("a", "sg");

			Util.NormalizeRanks(g);

			Assert.False(g.GetNode("sg").Rank.HasValue);
			Assert.Equal(0, g.GetNode("a").Rank);
		}

		[Fact]
		public void RemoveEmptyRanksRemovesBorderRanksWithoutAnyNodes()
		{
			var g = new Graph();
			g.Options.NodeRankFactor = 4;
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 4; });

			Util.RemoveEmptyRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
		}

		[Fact]
		public void RemoveEmptyRanksDoesNotRemoveNonBorderRanks()
		{
			var g = new Graph();
			g.Options.NodeRankFactor = 4;
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 8; });

			Util.RemoveEmptyRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(2, g.GetNode("b").Rank);
		}

		[Fact]
		public void RemoveEmptyRanksHandlesParentsWithUndefinedRanks()
		{
			var g = new Graph(true, false, true);
			g.Options.NodeRankFactor = 3;
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 6; });
			g.SetNode("sg");
			g.SetParent("a", "sg");

			Util.RemoveEmptyRanks(g);

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(2, g.GetNode("b").Rank);
			Assert.False(g.GetNode("sg").Rank.HasValue);
		}
	}
}
