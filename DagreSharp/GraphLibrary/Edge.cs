using System;
using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public class Edge
	{
		public string Id { get; internal set; }

		public string From { get; set; }

		public string To { get; set; }

		public string Name { get; set; }

		public int MinLength { get; set; } = 1;

		public LabelPosition LabelPosition { get; set; } = LabelPosition.Right;

		public double Width { get; set; }

		public double Height { get; set; }

		public int LabelOffset { get; set; } = 10;

		public int Weight { get; set; } = 1;

		// Acyclic -->
		public string ForwardName { get; set; }

		public bool IsReversed { get; set; }
		// <-- Acyclic

		public bool IsNestingEdge { get; set; }

		public int CutValue { get; set; }

		public int LabelRank { get; set; }

		public List<Point> Points { get; } = new List<Point>();

		public double? X { get; set; }

		public double? Y { get; set; }

		public double NodeX { get; set; }  // Find a better name

		public Edge(string from, string to)
		{
			if (string.IsNullOrEmpty(from))
			{
				throw new ArgumentNullException(nameof(from));
			}

			if (string.IsNullOrEmpty(to))
			{
				throw new ArgumentNullException(nameof(to));
			}

			Id = from + to;
			From = from;
			To = to;
			//EdgeLabelFunc = e => $"{e.From} - {e.To}";
		}

		public Edge Copy()
		{
			var edge = new Edge(From, To)
			{
				Id = Id,
				Name = Name,
				//EdgeLabelFunc = EdgeLabelFunc,
				MinLength = MinLength,
				LabelPosition = LabelPosition,
				Width = Width,
				Height = Height,
				LabelOffset = LabelOffset,
				Weight = Weight,
				ForwardName = ForwardName,
				IsReversed = IsReversed,
				IsNestingEdge = IsNestingEdge,
				CutValue = CutValue,
				LabelRank = LabelRank,
				X = X,
				Y = Y
			};

			edge.Points.AddRange(Points);
			return edge;
		}

		public void CopyFrom(Edge edge)
		{
			Height = edge.Height;
			Width = edge.Width;
			Weight = edge.Weight;
			X = edge.X;
			Y = edge.Y;
			LabelOffset = edge.LabelOffset;
			LabelPosition = edge.LabelPosition;
			LabelRank = edge.LabelRank;
			MinLength = edge.MinLength;

			Points.Clear();
			Points.AddRange(edge.Points);
		}
	}
}
