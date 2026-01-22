
using System;
using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public interface IEdge
	{
		string Id { get; }
		string From { get; set; }
		string To { get; set; }
		string Name { get; set; }
		double Height { get; set; }
		double Width { get; set; }
		int Weight { get; set; }
		double? X { get; set; }
		double? Y { get; set; }
		int LabelOffset { get; set; }
		LabelPosition LabelPosition { get; set; }
		int LabelRank { get; set; }
		int MinLength { get; set; }
		List<Point> Points { get; }
		int CutValue { get; set; }

		void CopyFrom(IEdge edge);
	}
}