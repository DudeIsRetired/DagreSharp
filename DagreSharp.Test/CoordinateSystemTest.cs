using DagreSharp.GraphLibrary;
using System.Xml.Linq;

namespace DagreSharp.Test
{
	public class CoordinateSystemTest
	{
		private readonly Graph g = new(true, false, true);

		[Fact]
		public void DoesNothingToNodeDimensionsWithRankdirTB()
		{
			g.Options.RankDirection = RankDirection.TopBottom;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; });

			CoordinateSystem.Adjust(g);

			var node = g.GetNode("a");
			Assert.Equal(100, node.Width);
			Assert.Equal(200, node.Height);
		}

		[Fact]
		public void DoesNothingToNodeDimensionsWithRankdirBT()
		{
			g.Options.RankDirection = RankDirection.BottomTop;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; });

			CoordinateSystem.Adjust(g);

			var node = g.GetNode("a");
			Assert.Equal(100, node.Width);
			Assert.Equal(200, node.Height);
		}

		[Fact]
		public void SwapsWidthAndHeightForNodesWithRankdirLR()
		{
			g.Options.RankDirection = RankDirection.LeftRight;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; });

			CoordinateSystem.Adjust(g);

			var node = g.GetNode("a");
			Assert.Equal(200, node.Width);
			Assert.Equal(100, node.Height);
		}

		[Fact]
		public void SwapsWidthAndHeightForNodesWithRankdirRL()
		{
			g.Options.RankDirection = RankDirection.RightLeft;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; });

			CoordinateSystem.Adjust(g);

			var node = g.GetNode("a");
			Assert.Equal(200, node.Width);
			Assert.Equal(100, node.Height);
		}

		[Fact]
		public void UndoDoesNothingToPointsWithRankdirTB()
		{
			g.Options.RankDirection = RankDirection.TopBottom;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; n.X = 20; n.Y = 40; });

			CoordinateSystem.Undo(g);

			var node = g.GetNode("a");
			Assert.Equal(100, node.Width);
			Assert.Equal(200, node.Height);
			Assert.Equal(20, node.X);
			Assert.Equal(40, node.Y);
		}

		[Fact]
		public void UndoFlipsTheYCoordinateForPointsWithRankdirBT()
		{
			g.Options.RankDirection = RankDirection.BottomTop;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; n.X = 20; n.Y = 40; });

			CoordinateSystem.Undo(g);

			var node = g.GetNode("a");
			Assert.Equal(100, node.Width);
			Assert.Equal(200, node.Height);
			Assert.Equal(20, node.X);
			Assert.Equal(-40, node.Y);
		}

		[Fact]
		public void UndoSwapsDimensionsAndCoordinatesForPointsWithRankdirLR()
		{
			g.Options.RankDirection = RankDirection.LeftRight;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; n.X = 20; n.Y = 40; });

			CoordinateSystem.Undo(g);

			var node = g.GetNode("a");
			Assert.Equal(200, node.Width);
			Assert.Equal(100, node.Height);
			Assert.Equal(40, node.X);
			Assert.Equal(20, node.Y);
		}

		[Fact]
		public void UndoSwapsDimsAndCoordsAndFlipsXForPointsWithRankdirRL()
		{
			g.Options.RankDirection = RankDirection.RightLeft;
			g.SetNode("a", n => { n.Width = 100; n.Height = 200; n.X = 20; n.Y = 40; });

			CoordinateSystem.Undo(g);

			var node = g.GetNode("a");
			Assert.Equal(200, node.Width);
			Assert.Equal(100, node.Height);
			Assert.Equal(-40, node.X);
			Assert.Equal(20, node.Y);
		}
	}
}
