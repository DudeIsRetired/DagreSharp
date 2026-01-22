using DagreSharp.GraphLibrary;

namespace DagreSharp
{
	public static class CoordinateSystem
	{
		public static void Adjust(Graph g)
		{
			if (g.Options.RankDirection == RankDirection.LeftRight || g.Options.RankDirection == RankDirection.RightLeft)
			{
				SwapWidthHeight(g);
			}
		}

		private static void SwapWidthHeight(Graph g)
		{
			foreach (var node in g.Nodes)
			{
				SwapWidthHeightOne(node);
			}

			foreach (var edge in g.Edges)
			{
				SwapWidthHeightOne(edge);
			}
		}

		private static void SwapWidthHeightOne(Node node)
		{
			(node.Height, node.Width) = (node.Width, node.Height);
		}

		private static void SwapWidthHeightOne(Edge edge)
		{
			(edge.Height, edge.Width) = (edge.Width, edge.Height);
		}

		public static void Undo(Graph g)
		{
			var rankDir = g.Options.RankDirection;
			if (rankDir == RankDirection.BottomTop || rankDir == RankDirection.RightLeft)
			{
				ReverseY(g);
			}

			if (rankDir == RankDirection.LeftRight || rankDir == RankDirection.RightLeft)
			{
				SwapXY(g);
				SwapWidthHeight(g);
			}
		}

		private static void ReverseY(Graph g)
		{
			foreach (var node in g.Nodes)
			{
				ReverseYOne(node);
			}

			foreach (var edge in g.Edges)
			{
				foreach (var point in edge.Points)
				{
					ReverseYOne(point);
				}

				if (edge.Y.HasValue)
				{
					ReverseYOne(edge);
				}
			}
		}

		private static void ReverseYOne(Node node)
		{
			node.Y = -node.Y;
		}

		private static void ReverseYOne(Edge edge)
		{
			edge.Y = -edge.Y;
		}

		private static void ReverseYOne(Point point)
		{
			point.Y = -point.Y;
		}

		private static void SwapXY(Graph g)
		{
			foreach (var node in g.Nodes)
			{
				SwapXYOne(node);
			}

			foreach (var edge in g.Edges)
			{
				foreach (var point in edge.Points)
				{
					SwapXYOne(point);
				}

				if (edge.Y.HasValue)
				{
					SwapXYOne(edge);
				}
			}
		}

		private static void SwapXYOne(Node node)
		{
			(node.Y, node.X) = (node.X, node.Y);
		}

		private static void SwapXYOne(Edge edge)
		{
			(edge.Y, edge.X) = (edge.X, edge.Y);
		}

		private static void SwapXYOne(Point point)
		{
			(point.Y, point.X) = (point.X, point.Y);
		}

	}
}
