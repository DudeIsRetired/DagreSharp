using DagreSharp.GraphLibrary;

namespace DagreSharp.Test
{
	public class NormalizeTest
	{
		private readonly Graph g = new(true, true, true);

		[Fact]
		public void DoesNotChangeAShortEdge()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 1; });
			g.SetEdge("a", "b");

			Normalize.Run(g);

			Assert.Single(g.Edges);
			var edge = g.Edges.First();
			Assert.Equal("a", edge.From);
			Assert.Equal("b", edge.To);
			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(1, g.GetNode("b").Rank);
		}

		[Fact]
		public void SplitsATwoLayerEdgeIntoTwoSegments()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b");

			Normalize.Run(g);

			var successorsA = g.GetSuccessors("a");
			Assert.Single(successorsA);

			var successor = successorsA.First();
			Assert.Equal(DummyType.Edge, successor.DummyType);
			Assert.Equal(1, successor.Rank);

			var successorsB = g.GetSuccessors(successor.Id);
			Assert.Single(successorsB);
			Assert.Contains(successorsB, s => s.Id == "b");

			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(2, g.GetNode("b").Rank);

			Assert.Single(g.Options.DummyChains);
			Assert.Equal(successor, g.Options.DummyChains.First());
		}

		[Fact]
		public void AssignsWidth0Height0ToDummyNodesByDefault()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 10; });

			Normalize.Run(g);

			var successorsA = g.GetSuccessors("a");
			Assert.Single(successorsA);
			
			var successor = successorsA.First();
			Assert.Equal(0, successor.Width);
			Assert.Equal(0, successor.Height);
		}

		[Fact]
		public void AssignsWidthAndHeightFromTheEdgeForTheNodeOnLabelRank()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 4; });
			g.SetEdge("a", "b", null, e => { e.Width = 20; e.Height = 10; e.LabelRank = 2; });

			Normalize.Run(g);

			var successorA = g.GetSuccessors("a").First();
			var labelNode = g.GetSuccessors(successorA.Id).First();

			Assert.Equal(20, labelNode.Width);
			Assert.Equal(10, labelNode.Height);
		}

		[Fact]
		public void PreservesTheWeightForTheEdge()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b", null, e => { e.Weight = 2; });

			Normalize.Run(g);

			var successorsA = g.GetSuccessors("a");
			Assert.Single(successorsA);
			
			var edge = g.GetEdge("a", successorsA.First().Id);
			Assert.Equal(2, edge.Weight);
		}

		[Fact]
		public void UndoReversesTheRunOperation()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b");

			Normalize.Run(g);
			Normalize.Undo(g);

			Assert.Single(g.Edges);
			Assert.Contains(g.Edges, e => e.From == "a" && e.To == "b");
			
			Assert.Equal(0, g.GetNode("a").Rank);
			Assert.Equal(2, g.GetNode("b").Rank);
		}

		[Fact]
		public void UndoRestoresPreviousEdgeLabels()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b", null, e => { e.LabelOffset = 2; });

			Normalize.Run(g);
			Normalize.Undo(g);

			Assert.Equal(2, g.GetEdge("a", "b").LabelOffset);
		}

		[Fact]
		public void UndoCollectsAssignedCoordinatesIntoThePointsAttribute()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b");

			Normalize.Run(g);
			
			var dummyLabel = g.GetNode(g.GetNeighbors("a").First().Id);
			dummyLabel.X = 5;
			dummyLabel.Y = 10;

			Normalize.Undo(g);

			var points = g.GetEdge("a", "b").Points;
			Assert.Single(points);
			Assert.Equal(5, points.First().X);
			Assert.Equal(10, points.First().Y);
		}

		[Fact]
		public void UndoMergesAssignedCoordinatesIntoThePointsAttribute()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 4; });
			g.SetEdge("a", "b");

			Normalize.Run(g);

			var aSucLabel = g.GetNode(g.GetNeighbors("a").First().Id);
			aSucLabel.X = 5;
			aSucLabel.Y = 10;

			var midLabel = g.GetNode(g.GetSuccessors(g.GetSuccessors("a").First().Id).First().Id);
			midLabel.X = 20;
			midLabel.Y = 25;

			var bPredLabel = g.GetNode(g.GetNeighbors("b").First().Id);
			bPredLabel.X = 100;
			bPredLabel.Y = 200;

			Normalize.Undo(g);

			var points = g.GetEdge("a", "b").Points;
			Assert.Equal(3, points.Count);
			
			Assert.Equal(5, points[0].X);
			Assert.Equal(10, points[0].Y);

			Assert.Equal(20, points[1].X);
			Assert.Equal(25, points[1].Y);

			Assert.Equal(100, points[2].X);
			Assert.Equal(200, points[2].Y);
		}

		[Fact]
		public void UndoSetsCoordsAndDimsForTheLabelIfTheEdgeHasOne()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 20; e.LabelRank = 1; });

			Normalize.Run(g);

			var labelNode = g.GetNode(g.GetSuccessors("a").First().Id);
			labelNode.X = 50;
			labelNode.Y = 60;
			labelNode.Width = 20;
			labelNode.Height = 10;

			Normalize.Undo(g);

			var edge = g.GetEdge("a", "b");
			Assert.Equal(50, edge.X);
			Assert.Equal(60, edge.Y);
			Assert.Equal(20, edge.Width);
			Assert.Equal(10, edge.Height);
		}

		[Fact]
		public void UndoSetsCoordsAndDimsForTheLabelIfTheLongEdgeHasOne()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 4; });
			g.SetEdge("a", "b", null, e => { e.Width = 10; e.Height = 20; e.LabelRank = 2; });

			Normalize.Run(g);

			var labelNode = g.GetNode(g.GetSuccessors(g.GetSuccessors("a").First().Id).First().Id);
			labelNode.X = 50;
			labelNode.Y = 60;
			labelNode.Width = 20;
			labelNode.Height = 10;

			Normalize.Undo(g);

			var edge = g.GetEdge("a", "b");
			Assert.Equal(50, edge.X);
			Assert.Equal(60, edge.Y);
			Assert.Equal(20, edge.Width);
			Assert.Equal(10, edge.Height);
		}

		[Fact]
		public void UndoRestoresMultiEdges()
		{
			g.SetNode("a", n => { n.Rank = 0; });
			g.SetNode("b", n => { n.Rank = 2; });
			g.SetEdge("a", "b", "bar");
			g.SetEdge("a", "b", "foo");

			Normalize.Run(g);

			var outEdges = g.GetOutEdges("a").ToList();	//.sort((a, b) => a.name.localeCompare(b.name));
			Assert.Equal(2, outEdges.Count);

			var barDummy = g.GetNode(outEdges[0].To);
			barDummy.X = 5;
			barDummy.Y = 10;

			var fooDummy = g.GetNode(outEdges[1].To);
			fooDummy.X = 15;
			fooDummy.Y = 20;

			Normalize.Undo(g);

			Assert.False(g.HasEdge("a", "b"));
			
			var barPoints = g.GetEdge("a", "b", "bar").Points;
			Assert.Single(barPoints);
			Assert.Equal(5, barPoints[0].X);
			Assert.Equal(10, barPoints[0].Y);

			var fooPoints = g.GetEdge("a", "b", "foo").Points;
			Assert.Single(fooPoints);
			Assert.Equal(15, fooPoints[0].X);
			Assert.Equal(20, fooPoints[0].Y);
		}
	}
}
