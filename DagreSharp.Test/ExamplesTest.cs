using DagreSharp.GraphLibrary;
using Xunit.Abstractions;

namespace DagreSharp.Test
{
	public class ExamplesTest(ITestOutputHelper output)
	{
		private readonly ITestOutputHelper _output = output;

		private static void AssertNodes(IEnumerable<INode> expected, IEnumerable<INode> actual)
		{
			Assert.Equal(expected.Count(), actual.Count());

			foreach (var node in expected)
			{
				var target = actual.FirstOrDefault(n => n.Id == node.Id);
				Assert.NotNull(target);

				Assert.Equal(node.Name, target.Name);
				Assert.Equal(node.X, target.X);
				Assert.Equal(node.Y, target.Y);
			}
		}

		private static void AssertEdges(IEnumerable<IEdge> expected, IEnumerable<IEdge> actual)
		{
			Assert.Equal(expected.Count(), actual.Count());

			foreach (var edge in expected)
			{
				var target = actual.FirstOrDefault(e => e.From == edge.From && e.To == edge.To);
				Assert.NotNull(target);

				foreach (var point in edge.Points)
				{
					Assert.Contains(target.Points, p => p.X == point.X && p.Y == point.Y);
				}
			}
		}

		private void Print(IEnumerable<INode> nodes, IEnumerable<IEdge> edges)
		{
			foreach (var node in nodes)
			{
				_output.WriteLine($"Node {node.Id}: x = {node.X}, y = {node.Y}");
			}

			foreach (var edge in edges)
			{
				var pointsString = "[";
				foreach (var point in edge.Points)
				{
					pointsString += point.ToString();
				}
				pointsString += "]";
				_output.WriteLine($"Edge {edge.From} -> {edge.To}: points = {pointsString}");
			}
		}

		[Fact]
		public void Example1()  // not sure where I got this one from
		{
			var g = new Graph();
			var dagre = new Dagre(g);
			dagre.Options.RankDirection = RankDirection.TopBottom;
			dagre.Options.MarginX = 20;
			dagre.Options.MarginY = 20;
			dagre.Options.NodeSeparation = 50;
			dagre.Options.EdgeSeparation = 20;
			dagre.Options.RankSeparation = 100;

			// Add nodes with width and height
			dagre.SetNode("A", n => { n.Name = "Start"; n.Width = 80; n.Height = 40; });
			dagre.SetNode("B", n => { n.Name = "Process 1"; n.Width = 80; n.Height = 40; });
			dagre.SetNode("C", n => { n.Name = "Process 2"; n.Width = 80; n.Height = 40; });
			dagre.SetNode("D", n => { n.Name = "End"; n.Width = 80; n.Height = 40; });

			// Add edges
			dagre.SetEdge("A", "B");
			dagre.SetEdge("B", "C");
			dagre.SetEdge("C", "D");

			// Run the layout
			dagre.Layout();

			Node[] expectedNodes = [
				new Node("A") { Name = "Start", X = 60, Y = 40 },
				new Node("B") { Name = "Process 1", X = 60, Y = 180 },
				new Node("C") { Name = "Process 2", X = 60, Y = 320 },
				new Node("D") { Name = "End", X = 60, Y = 460 }
				];

			Edge[] expectedEdges = [
				new Edge("A", "B"),
				new Edge("B", "C"),
				new Edge("C", "D")
				];

			expectedEdges[0].Points.AddRange([new Point(60, 60), new Point(60, 110), new Point (60, 160)]);
			expectedEdges[1].Points.AddRange([new Point(60, 200), new Point(60, 250), new Point(60, 300)]);
			expectedEdges[2].Points.AddRange([new Point(60, 340), new Point(60, 390), new Point(60, 440)]);

			Print(dagre.Nodes, dagre.Edges);
			AssertNodes(expectedNodes, dagre.Nodes);
			AssertEdges(expectedEdges, dagre.Edges);
		}

		[Fact]
		public void ExampleFromDagreJsDocumentation()
		{
			var g = new Graph();
			var dagre = new Dagre(g);

			dagre.SetNode("kspacey", n => { n.Name = "Kevin Spacey"; n.Width = 144; n.Height = 100; });
			dagre.SetNode("swilliams", n => { n.Name = "Saul Williams"; n.Width = 160; n.Height = 100; });
			dagre.SetNode("bpitt", n => { n.Name = "Brad Pitt"; n.Width = 108; n.Height = 100; });
			dagre.SetNode("hford", n => { n.Name = "Harrison Ford"; n.Width = 168; n.Height = 100; });
			dagre.SetNode("lwilson", n => { n.Name = "Luke Wilson"; n.Width = 144; n.Height = 100; });
			dagre.SetNode("kbacon", n => { n.Name = "Kevin Bacon"; n.Width = 121; n.Height = 100; });

			dagre.SetEdge("kspacey", "swilliams");
			dagre.SetEdge("swilliams", "kbacon");
			dagre.SetEdge("bpitt", "kbacon");
			dagre.SetEdge("hford", "lwilson");
			dagre.SetEdge("lwilson", "kbacon");

			// Run the layout
			dagre.Layout();

			Node[] expectedNodes = [
				new Node("kspacey") { Name = "Kevin Spacey", X = 80, Y = 50 },
				new Node("swilliams") { Name = "Saul Williams", X = 80, Y = 200 },
				new Node("bpitt") { Name = "Brad Pitt", X = 264, Y = 200 },
				new Node("hford") { Name = "Harrison Ford", X = 440, Y = 50 },
				new Node("lwilson") { Name = "Luke Wilson", X = 440, Y = 200 },
				new Node("kbacon") { Name = "Kevin Bacon", X = 264, Y = 350 }
				];

			Edge[] expectedEdges = [
				new Edge("kspacey", "swilliams"),
				new Edge("swilliams", "kbacon"),
				new Edge("bpitt", "kbacon"),
				new Edge("hford", "lwilson"),
				new Edge("lwilson", "kbacon")
				];

			expectedEdges[0].Points.AddRange([new Point(80, 100), new Point(80, 125), new Point(80, 150)]);
			expectedEdges[1].Points.AddRange([new Point(80, 250), new Point(80, 275), new Point(203.5, 325.3396739130435)]);
			expectedEdges[2].Points.AddRange([new Point(264, 250), new Point(264, 275), new Point(264, 300)]);
			expectedEdges[3].Points.AddRange([new Point(440, 100), new Point(440, 125), new Point(440, 150)]);
			expectedEdges[4].Points.AddRange([new Point(440, 250), new Point(440, 275), new Point(324.5, 324.21875)]);

			Print(dagre.Nodes, dagre.Edges);
			AssertNodes(expectedNodes, dagre.Nodes);
			AssertEdges(expectedEdges, dagre.Edges);
		}

	}
}