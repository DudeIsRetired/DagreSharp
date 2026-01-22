using DagreSharp.GraphLibrary;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.Order
{
	public static class SubGraph
	{
		private class FalseBiasComparer : IComparer<MappedEntry>
		{
			public int Compare(MappedEntry x, MappedEntry y)
			{
				return CompareWithBias(x, y, false);
			}
		}

		private class TrueBiasComparer : IComparer<MappedEntry>
		{
			public int Compare(MappedEntry x, MappedEntry y)
			{
				return CompareWithBias(x, y, true);
			}
		}

		private static int CompareWithBias(MappedEntry x, MappedEntry y, bool bias)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}
			else if (y == null)
			{
				return 1;
			}

			if (x.BaryCenter < y.BaryCenter)
			{
				return -1;
			}
			else if (x.BaryCenter > y.BaryCenter)
			{
				return 1;
			}

			return !bias ? x.Index - y.Index : y.Index - x.Index;
		}

		public static BaryCenterResult Sort(Graph g, string v, Graph cg, bool biasRight = false)
		{
			var movable = g.GetChildren(v).Select(c => c.Id);
			var node = g.FindNode(v);
			var bl = node?.BorderLeft.Values.FirstOrDefault();
			var br = node?.BorderRight.Values.FirstOrDefault();
			var subgraphs = new Dictionary<string, BaryCenterResult>();

			if (bl != null)
			{
				movable = movable.Where(w => w != bl && w != br).ToList();
			}

			var barycenters = BaryCenter.Create(g, movable);

			foreach (var entry in barycenters)
			{
				if (g.GetChildren(entry.Id).Count > 0)
				{
					var subgraphResult = Sort(g, entry.Id, cg, biasRight);
					subgraphs.Add(entry.Id, subgraphResult);
					if (subgraphResult.BaryCenter.HasValue)
					{
						BaryCenter.Merge(entry, subgraphResult);
					}
				}
			}

			var entries = ConflictResolver.ResolveConflicts(barycenters, cg);
			ExpandSubgraphs(entries, subgraphs);

			var result = Sort(entries, biasRight);

			if (bl != null && br != null)
			{
				result.Vs.Insert(0, bl);
				result.Vs.Add(br);
				var blPredecessors = g.GetPredecessors(bl);

				if (blPredecessors.Count > 0)
				{
					var blPred = blPredecessors.First();
					var brPred = g.GetPredecessors(br).First();

					if (result.BaryCenter == 0)
					{
						result.BaryCenter = 0;
						result.Weight = 0;
					}

					result.BaryCenter = (double)((result.BaryCenter ?? 0) * result.Weight + blPred.Order + brPred.Order) / (result.Weight + 2);
					result.Weight += 2;
				}
			}

			return result;
		}

		private static void ExpandSubgraphs(List<MappedEntry> entries, Dictionary<string, BaryCenterResult> subgraphs)
		{
			foreach (var entry in entries)
			{
				var vss = entry.Vs.ToList();
				foreach (var v in vss)
				{
					entry.Vs.Clear();
					if (subgraphs.TryGetValue(v, out BaryCenterResult value))
					{
						entry.Vs.AddRange(value.Vs);
					}
					else
					{
						entry.Vs.Add(v);
					}
				}
			}
		}

		public static BaryCenterResult Sort(List<MappedEntry> entries, bool biasRight = false)
		{
			var parts = Util.Partition(entries, entry => entry.BaryCenter.HasValue);
			var sortable = parts.LeftHandSide;
			parts.RightHandSide.Sort(Comparer<MappedEntry>.Create((a, b) => b.Index - a.Index));
			var unsortable = parts.RightHandSide;
			var vs = new List<List<string>>();
			var sum = 0.0;
			var weight = 0;
			var vsIndex = 0;

			IComparer<MappedEntry> comparer = null;
			if (biasRight)
			{
				comparer = new TrueBiasComparer();
			}
			else
			{
				comparer = new FalseBiasComparer();
			}

			sortable.Sort(comparer);

			vsIndex = ConsumeUnsortable(vs, unsortable, vsIndex);

			foreach (var entry in sortable)
			{
				vsIndex += entry.Vs.Count;
				vs.Add(entry.Vs);
				if (entry.BaryCenter.HasValue)
				{
					sum += entry.BaryCenter.Value * entry.Weight;
				}
				weight += entry.Weight;
				vsIndex = ConsumeUnsortable(vs, unsortable, vsIndex);
			}

			var result = new BaryCenterResult(string.Empty);
			result.Vs.AddRange(vs.SelectMany(v => v).ToList());

			if (weight != 0)
			{
				result.BaryCenter = sum / weight;
				result.Weight = weight;
			}

			return result;
		}

		private static int ConsumeUnsortable(List<List<string>> vs, List<MappedEntry> unsortable, int index)
		{
			if (unsortable.Count == 0)
			{
				return index;
			}

			var last = unsortable[unsortable.Count - 1];
			while (unsortable.Count > 0 && last.Index <= index)
			{
				unsortable.RemoveAt(unsortable.Count - 1);
				vs.Add(last.Vs);
				index++;

				if (unsortable.Count > 0)
				{
					last = unsortable[unsortable.Count - 1];
				}
			}

			return index;
		}
	}
}
