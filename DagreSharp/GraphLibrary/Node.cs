using System;
using System.Collections.Generic;

namespace DagreSharp.GraphLibrary
{
	public class Node : IEquatable<Node>, IComparable<Node>
	{
		public string Id { get; }

		public string Name { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		internal List<SelfEdge> SelfEdges { get; } = new List<SelfEdge>();

		// greedyFAS -->
		public int In { get; set; }

		public int Out { get; set; }
		// <-- greedyFAS

		public int? Rank { get; set; }

		public int Order { get; set; }

		public string BorderTop { get; set; }

		public string BorderBottom { get; set; }

		public int Low { get; set; }

		public int Lim { get; set; }

		public Node Parent { get; set; }

		public int? MinRank { get; set; }

		public int? MaxRank { get; set; }

		public DummyType DummyType { get; set; }

		public Edge DummyEdge { get; set; }

		public LabelPosition LabelPosition { get; set; }

		public double X { get; set; }

		public double Y { get; set; }

		public string BorderType { get; set; }

		public Dictionary<int, string> BorderLeft { get; } = new Dictionary<int, string>();

		public Dictionary<int, string> BorderRight { get; } = new Dictionary<int, string>();

		public List<Node> Children { get; } = new List<Node>();

		public List<Node> Predecessors { get; } = new List<Node>();

		public List<Node> Successors { get; } = new List<Node>();

		public Node(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			Id = id;
			Name = Id;
		}

		public Node Copy()
		{
			var node = new Node(Id)
			{
				Name = Name,
				Width = Width,
				Height = Height,
				In = In,
				Out = Out,
				Rank = Rank,
				Order = Order,
				BorderTop = BorderTop,
				BorderBottom = BorderBottom,
				Low = Low,
				Lim = Lim,
				Parent = Parent,
				DummyEdge = DummyEdge,
				MinRank = MinRank,
				MaxRank = MaxRank,
				DummyType = DummyType,
				LabelPosition = LabelPosition,
				X = X,
				Y = Y
			};

			node.SelfEdges.AddRange(SelfEdges);
			return node;
		}

		public bool Equals(Node other)
		{
			if (other == null)
			{
				return false;
			}

			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public int CompareTo(Node other)
		{
			if (other == null)
			{
				return 1;
			}

			return Id.CompareTo(other.Id);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Node);
		}
	}
}
