using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp.Order
{
	public class ConflictResolver
	{
		/*
		* Given a list of entries of the form {v, barycenter, weight} and a
		* constraint graph this function will resolve any conflicts between the
		* constraint graph and the barycenters for the entries. If the barycenters for
		* an entry would violate a constraint in the constraint graph then we coalesce
		* the nodes in the conflict into a new node that respects the contraint and
		* aggregates barycenter and weight information.
		*
		* This implementation is based on the description in Forster, "A Fast and
		* Simple Hueristic for Constrained Two-Level Crossing Reduction," thought it
		* differs in some specific details.
		*
		* Pre-conditions:
		*
		*    1. Each entry has the form {v, barycenter, weight}, or if the node has
		*       no barycenter, then {v}.
		*
		* Returns:
		*
		*    A new list of entries of the form {vs, i, barycenter, weight}. The list
		*    `vs` may either be a singleton or it may be an aggregation of nodes
		*    ordered such that they do not violate constraints from the constraint
		*    graph. The property `i` is the lowest original index of any of the
		*    elements in `vs`.
		*/
		public static List<MappedEntry> ResolveConflicts(List<BaryCenterResult> entries, Graph cg)
		{
			var mappedEntries = new Dictionary<string, MappedEntry>();

			for (int i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				var item = new MappedEntry
				{
					InDegree = 0,
					Index = i,
				};

				if (entry.BaryCenter.HasValue)
				{
					item.BaryCenter = entry.BaryCenter.Value;
					item.Weight = entry.Weight;
				}

				item.Vs.Add(entry.Id);
				mappedEntries.Add(entry.Id, item);
			}

			foreach (var e in cg.Edges)
			{
				var entryV = mappedEntries.TryGetValue(e.From, out MappedEntry value) ? value : null;
				var entryW = mappedEntries.TryGetValue(e.To, out value) ? value : null;

				if (entryV != null && entryW != null)
				{
					entryW.InDegree++;
					entryV.Out.Add(mappedEntries[e.To]);
				}
			};

			var sourceSet = mappedEntries.Values.Where(entry => entry.InDegree == 0);

			var s = new Stack<MappedEntry>(sourceSet);
			return DoResolveConflicts(s);
		}

		private static List<MappedEntry> DoResolveConflicts(Stack<MappedEntry> sourceSet)
		{
			var entries = new List<MappedEntry>();

			void HandleIn(MappedEntry vEntry, MappedEntry uEntry)
			{
				if (uEntry.IsMerged)
				{
					return;
				}

				if (!uEntry.BaryCenter.HasValue || !vEntry.BaryCenter.HasValue || uEntry.BaryCenter >= vEntry.BaryCenter)
				{
					MergeEntries(vEntry, uEntry);
				}
			}

			void HandleOut(MappedEntry vEntry, MappedEntry wEntry)
			{
				wEntry.In.Add(vEntry);
				if (--wEntry.InDegree == 0)
				{
					sourceSet.Push(wEntry);
				}
			}

			while (sourceSet.Count > 0)
			{
				var entry = sourceSet.Pop();
				entries.Add(entry);
				entry.In.Reverse();

				foreach (var me in entry.In)
				{
					HandleIn(entry, me);
				}

				foreach (var me in entry.Out)
				{
					HandleOut(entry, me);
				}
			}

			var result = entries.Where(entry => !entry.IsMerged).ToList();
			return result;
		}

		private static void MergeEntries(MappedEntry target, MappedEntry source)
		{
			var sum = 0.0;
			var weight = 0;

			if (target.Weight != 0)
			{
				if (target.BaryCenter.HasValue)
				{
					sum += target.BaryCenter.Value * target.Weight;
				}
				weight += target.Weight;
			}

			if (source.Weight != 0)
			{
				if (source.BaryCenter.HasValue)
				{
					sum += source.BaryCenter.Value * source.Weight;
				}
				weight += source.Weight;
			}

			target.Vs.InsertRange(0, source.Vs);
			target.BaryCenter = sum / weight;
			target.Weight = weight;
			target.Index = Math.Min(source.Index, target.Index);
			source.IsMerged = true;
		}
	}
}
