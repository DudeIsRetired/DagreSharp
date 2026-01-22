using System;

namespace DagreSharp.GraphLibrary
{
	internal class SelfEdge
	{
		public Edge Edge { get; set; }

		public SelfEdge(Edge edge)
		{
			Edge = edge ?? throw new ArgumentNullException(nameof(edge));
		}
	}
}
