
using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public interface IGraphOptions
	{
		RankDirection RankDirection { get; set; }
		GraphAlignment Aligment { get; set; }
		int NodeSeparation { get; set; }
		int EdgeSeparation { get; set; }
		int RankSeparation { get; set; }
		int MarginX { get; set; }
		int MarginY { get; set; }
		Acyclicer Acyclicer { get; set; }
		GraphRank Ranker { get; set; }
		double Width { get; set; }
		double Height { get; set; }
		string NestingRoot { get; set; }
		List<Node> DummyChains { get; }
		int NodeRankFactor { get; set; }
		string Root { get; set; }
	}
}