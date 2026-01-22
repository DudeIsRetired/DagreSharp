using System.Collections.Generic;

namespace DagreSharp
{
	public class Partition<T>
	{
		public List<T> LeftHandSide { get; } = new List<T>();

		public List<T> RightHandSide { get; } = new List<T>();
	}
}
