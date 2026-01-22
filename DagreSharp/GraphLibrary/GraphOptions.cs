using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public class GraphOptions
	{
		public RankDirection RankDirection { get; set; }

		public int MarginX { get; set; }

		public int MarginY { get; set; }

		public int NodeSeparation { get; set; } = 50;

		public int EdgeSeparation { get; set; } = 20;

		public int RankSeparation { get; set; } = 50;

		public Acyclicer Acyclicer { get; set; }

		public string NestingRoot { get; set; }

		public int NodeRankFactor { get; set; }

		public GraphRank Ranker { get; set; }

		public int MaxRank { get; set; }

		public List<Node> DummyChains { get; } = new List<Node>();

		public string Root { get; set; }

		public GraphAlignment Aligment { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		internal void CopyFrom(GraphOptions other)
		{
			RankDirection = other.RankDirection;
			MarginX = other.MarginX;
			MarginY = other.MarginY;
			NodeSeparation = other.NodeSeparation;
			EdgeSeparation = other.EdgeSeparation;
			RankSeparation = other.RankSeparation;
			Acyclicer = other.Acyclicer;
			NestingRoot = other.NestingRoot;
			NodeRankFactor = other.NodeRankFactor;
			Ranker = other.Ranker;
			MaxRank = other.MaxRank;
			Root = other.Root;
			Aligment = other.Aligment;
			Width = other.Width;
			Height = other.Height;
			DummyChains.AddRange(DummyChains);
		}
	}
}
