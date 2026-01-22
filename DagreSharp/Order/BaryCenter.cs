using DagreSharp.GraphLibrary;
using System.Collections.Generic;

namespace DagreSharp.Order
{
	public static class BaryCenter
	{
		public static List<BaryCenterResult> Create(Graph g, IEnumerable<string> movable)
		{
			var result = new List<BaryCenterResult>();

			foreach (var v in movable)
			{
				var inV = g.GetInEdges(v);
				var bcr = new BaryCenterResult(v);

				if (inV.Count == 0)
				{
					result.Add(bcr);
					continue;
				}

				foreach (var e in inV)
				{
					var nodeU = g.GetNode(e.From);
					bcr.Sum += (e.Weight * nodeU.Order);
					bcr.Weight += e.Weight;
				}

				bcr.BaryCenter = (double)bcr.Sum / bcr.Weight;

				result.Add(bcr);
			}

			return result;
		}

		public static void Merge(BaryCenterResult target, BaryCenterResult other)
		{
			if (target.BaryCenter != 0)
			{
				target.BaryCenter = (target.BaryCenter * target.Weight +
									 other.BaryCenter * other.Weight) /
									(target.Weight + other.Weight);
				target.Weight += other.Weight;
			}
			else
			{
				target.BaryCenter = other.BaryCenter;
				target.Weight = other.Weight;
			}
		}

	}
}
