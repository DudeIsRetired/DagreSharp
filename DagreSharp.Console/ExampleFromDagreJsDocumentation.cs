using DagreSharp.GraphLibrary;

namespace DagreSharp.Console
{
	internal class ExampleFromDagreJsDocumentation
	{
		public static Dagre Create()
		{
			var g = new Graph();
			var dagre = new Dagre(g);
			//dagre.Options.Acyclicer = Acyclicer.Greedy;
			//dagre.Options.RankDirection = RankDirection.RightLeft;
			//dagre.Options.Aligment = GraphAlignment.DownRight;
			//dagre.Options.Ranker = GraphRank.LongestPath;

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

			return dagre;
		}
	}
}
