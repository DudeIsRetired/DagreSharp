
using System;
using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public interface INode
	{
		string Id { get; }
		string Name { get; set; }
		double Width { get; set; }
		double Height { get; set; }
		double X { get; set; }
		double Y { get; set; }
		LabelPosition LabelPosition { get; set; }
		int? Rank { get; set; }
		int Order { get; set; }
		int? MinRank { get; set; }
		int? MaxRank { get; set; }
		string BorderType { get; set; }
		Dictionary<int, string> BorderLeft { get; }
		Dictionary<int, string> BorderRight { get; }
		string BorderTop { get; set; }
		string BorderBottom { get; set; }
		DummyType DummyType { get; set; }
		Edge DummyEdge { get; set; }
		int Low { get; set; }
		int Lim { get; set; }
		Node Parent { get; set; }
	}
}