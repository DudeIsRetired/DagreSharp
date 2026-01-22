using DagreSharp.GraphLibrary;
namespace DagreSharp.Console
{
	internal class Example1
	{
		public static Dagre Create()
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

			return dagre;
		}
	}
}
