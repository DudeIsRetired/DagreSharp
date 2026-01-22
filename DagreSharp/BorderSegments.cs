using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp
{
	public static class BorderSegments
	{
		public static void Add(Graph g)
		{
			void DepthFirstSearch(Node node)
			{
				var children = g.GetChildrenInternal(node.Id);

				foreach (var child in children)
				{
					DepthFirstSearch(child);
				}

				if (node.MinRank.HasValue)
				{
					node.BorderLeft.Clear();
					node.BorderRight.Clear();
					var maxRank = node.MaxRank + 1;
					for (var rank = node.MinRank.Value; rank < maxRank; ++rank)
					{
						AddBorderNode(g, () => node.BorderLeft, "_bl", node.Id, "borderLeft", rank);
						AddBorderNode(g, () => node.BorderRight, "_br", node.Id, "borderRight", rank);
					}
				}
			}

			var childList = g.GetChildrenInternal().ToList();
			foreach (var child in childList)
			{
				DepthFirstSearch(child);
			}
		}

		private static void AddBorderNode(Graph g, Func<Dictionary<int, string>> prop, string prefix, string sg, string propName, int rank)
		{
			var property = prop();
			var prevRank = rank - 1;
			var prev = property.TryGetValue(prevRank, out string value) ? value : null;
			var curr = Util.AddDummyNode(g, DummyType.Border, prefix, n =>
			{
				n.Width = 0;
				n.Height = 0;
				n.Rank = rank;
				n.BorderType = propName;
			});

			property[rank] = curr.Id;
			g.SetParent(curr.Id, sg);

			if (!string.IsNullOrEmpty(prev))
			{
				g.SetEdge(prev, curr.Id, null, e => { e.Weight = 1; });
			}
		}
	}
}
