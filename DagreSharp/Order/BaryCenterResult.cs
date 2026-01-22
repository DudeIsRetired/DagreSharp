using System;
using System.Collections.Generic;

namespace DagreSharp.Order
{
	public class BaryCenterResult
	{
		public string Id { get; }

		public int Sum { get; set; }

		public int Weight { get; set; }

		public double? BaryCenter { get; set; }

		public List<string> Vs { get; } = new List<string>();

		public BaryCenterResult(string id)
		{
			Id = id ?? throw new ArgumentNullException(nameof(id));
		}
	}
}
