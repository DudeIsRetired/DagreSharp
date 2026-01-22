using System;
using System.Collections.Generic;

namespace DagreSharp.Order
{
	public class MappedEntry
	{
		public int InDegree { get; set; }

		public List<MappedEntry> In { get; } = new List<MappedEntry>();

		public List<MappedEntry> Out { get; } = new List<MappedEntry>();

		public List<string> Vs { get; }

		public int Index { get; set; }

		public double? BaryCenter { get; set; }

		public int Weight { get; set; }

		public bool IsMerged { get; set; }

		public MappedEntry()
		{
			Vs = new List<string>();
		}

		public MappedEntry(IEnumerable<string> vs)
		{
			if (vs == null)
			{
				throw new ArgumentNullException(nameof(vs));
			}

			Vs = new List<string>(vs);
		}

	}
}
