using DagreSharp.GraphLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DagreSharp
{
	public static class Util
	{
		public static bool IsDebug { get; set; }

		private static int _idCounter = 0;

		public static string UniqueId(string prefix)
		{
			_idCounter++;
			return prefix + _idCounter.ToString();
			//return prefix + Guid.NewGuid().ToString();
		}

		/*
		* Adds a dummy node to the graph and return v.
		*/
		public static Node AddDummyNode(Graph g, DummyType type, string name, Action<Node> configure = null)
		{
			var v = UniqueId(name);

			var node = g.SetNode(new Node(v), n =>
			{
				n.DummyType = type;
				configure?.Invoke(n);
			});

			return node;
		}

		public static Node AddBorderNode(Graph g, string prefix, int? rank = null, int? order = null)
		{
			return AddDummyNode(g, DummyType.Border, prefix, n =>
			{
				n.Width = 0;
				n.Height = 0;

				if (rank.HasValue && order.HasValue)
				{
					n.Rank = rank.Value;
					n.Order = order.Value;
				}
			});
		}

		public static Graph AsNonCompoundGraph(Graph g)
		{
			var simplified = new Graph(true, g.IsMultigraph);
			simplified.Options.CopyFrom(g.Options);

			foreach (var node in g.Nodes)
			{
				if (g.GetChildren(node.Id).Count == 0)
				{
					simplified.SetNode(node);
				}
			}

			foreach (var e in g.Edges)
			{
				simplified.SetEdge(e);
			}

			return simplified;
		}

		/*
		* Returns a new graph with only simple edges. Handles aggregation of data
		* associated with multi-edges.
		*/
		public static Graph Simplify(Graph g)
		{
			var simplified = new Graph();
			simplified.Options.CopyFrom(g.Options);

			foreach (var node in g.Nodes)
			{
				simplified.SetNode(node);
			}

			foreach (var edge in g.Edges)
			{
				var simpleEdge = simplified.FindEdge(edge.From, edge.To);
				simplified.SetEdge(edge.From, edge.To, null, e =>
				{
					if (simpleEdge != null)
					{
						e.Weight = simpleEdge.Weight + edge.Weight;
						e.MinLength = Math.Max(simpleEdge.MinLength, edge.MinLength);
					}
					else
					{
						e.Weight = edge.Weight;
						e.MinLength = Math.Max(1, edge.MinLength);
					}
				});
			}

			return simplified;
		}

		public static void RemoveEmptyRanks(Graph g)
		{
			// Ranks may not start at 0, so we need to offset them
			var rankNodes = g.Nodes.Where(n => n.Rank.HasValue).OrderBy(n => n.Rank).ToList();
			var offset = rankNodes.Select(n => n.Rank.Value).Min();
			var maxRank = int.MinValue;

			var layers = new Dictionary<int, List<Node>>();

			foreach (var node in rankNodes)
			{
				var rank = node.Rank.Value - offset;

				if (rank > maxRank)
				{
					maxRank = rank;
				}

				if (!layers.TryGetValue(rank, out List<Node> value))
				{
					value = new List<Node>();
					layers.Add(rank, value);
				}

				value.Add(node);
			}

			var delta = 0;
			var nodeRankFactor = g.Options.NodeRankFactor;

			for (int i = 0; i <= maxRank; i++)
			{
				var containsKey = layers.ContainsKey(i);

				if (!containsKey && i % nodeRankFactor != 0)
				{
					--delta;
				}
				else if (containsKey && delta != 0)
				{
					var vs = layers[i];

					foreach (var v in vs)
					{
						v.Rank += delta;
					}
				}
			}
		}

		/*
		* Adjusts the ranks for all nodes in the graph such that all nodes v have
		* rank(v) >= 0 and at least one node w has rank(w) = 0.
		*/
		public static void NormalizeRanks(Graph g)
		{
			var min = g.Nodes.Select(n => n.Rank).Min();

			foreach (var node in g.Nodes)
			{
				node.Rank -= min;
			}
		}

		public static int MaxRank(Graph g)
		{
			return MaxRank(g.Nodes);
		}

		public static int MaxRank(IEnumerable<Node> nodes)
		{
			var maxRank = int.MinValue;

			foreach (var node in nodes)
			{
				if (node.Rank.HasValue && node.Rank.Value > maxRank)
				{
					maxRank = node.Rank.Value;
				}
			}

			return maxRank;
		}

		public static ICollection<int> Range(int start, int? limit = null, int? step = 1)
		{
			if (limit == null)
			{
				limit = start;
				start = 0;
			}

			Func<int, bool> endCon = (i) => i < limit;

			if (!step.HasValue)
			{
				step = 1;
			}

			if (step < 0)
			{
				endCon = (i) => limit < i;
			}

			var range = new List<int>();

			for (var i = start; endCon(i); i += step.Value)
			{
				range.Add(i);
			}

			return range;
		}

		/*
		* Partition a collection into two groups: `lhs` and `rhs`. If the supplied
		* function returns true for an entry it goes into `lhs`. Otherwise it goes
		* into `rhs.
		*/
		public static Partition<T> Partition<T>(ICollection<T> collection, Func<T, bool> func)
		{
			var result = new Partition<T>();

			foreach (var item in collection)
			{
				if (func(item))
				{
					result.LeftHandSide.Add(item);
				}
				else
				{
					result.RightHandSide.Add(item);
				}
			}

			return result;
		}

		/*
		* Given a DAG with each node assigned "rank" and "order" properties, this
		* function will produce a matrix with the ids of each node.
		*/
		public static List<List<string>> BuildLayerMatrix(Graph g)
		{
			var maxRank = MaxRank(g) + 1;
			var layering = new List<List<string>>(maxRank);

			for (int i = 0; i < maxRank; ++i)
			{
				layering.Add(new List<string>());
			}

			foreach (var node in g.Nodes.OrderBy(n => n.Order))
			{
				if (node.Rank.HasValue)
				{
					layering[node.Rank.Value].Add(node.Id);
				}
			}

			return layering;
		}

		public static Dictionary<string, int> ZipObject(List<string> props, int[] values)
		{
			var result = new Dictionary<string, int>();
			
			for (var i = 0; i < props.Count; i++)
			{
				result.Add(props[i], values[i]);
			}

			return result;
		}

		public static Dictionary<TKey, TValue> MapValues<TKey, TValue>(Dictionary<TKey, TValue> obj, Func<TValue, TValue> func)
			//where TKey: notnull
		{
			var result = new Dictionary<TKey, TValue>();

			foreach (var kvp in obj)
			{
				result[kvp.Key] = func(kvp.Value);
			}

			return result;
		}

		/*
		* Finds where a line starting at point ({x, y}) would intersect a rectangle
		* ({x, y, width, height}) if it were pointing at the rectangle's center.
		*/
		public static Point IntersectRect(Node rect, Point point)
		{
			var x = rect.X;
			var y = rect.Y;

			// Rectangle intersection algorithm from:
			// http://math.stackexchange.com/questions/108113/find-edge-between-two-boxes
			var dx = point.X - x;
			var dy = point.Y - y;
			var w = (double)rect.Width / 2;
			var h = (double)rect.Height / 2;

			if (dx == 0 && dy == 0)
			{
				throw new InvalidOperationException("Not possible to find intersection inside of the rectangle");
			}

			double sx, sy;
			if (Math.Abs(dy) * w > Math.Abs(dx) * h)
			{
				// Intersection is top or bottom of rect.
				if (dy < 0)
				{
					h = -h;
				}
				sx = (double)h * dx / dy;
				sy = h;
			}
			else
			{
				// Intersection is left or right of rect.
				if (dx < 0)
				{
					w = -w;
				}
				sx = w;

				if (dx == 0) dx = 1;	// Note!
				sy = (double)w * dy / dx;
			}

			return new Point(x + sx, y + sy);
		}

		public static void Time(string caption, Action action)
		{
			if (!IsDebug)
			{
				action();
				return;
			}

			var stopwatch = Stopwatch.StartNew();
			action();
			stopwatch.Stop();
			Console.WriteLine($"{caption} execution time: {stopwatch.ElapsedMilliseconds} ms");
		}

		public static T Time<T>(string caption, Func<T> func)
		{
			if (!IsDebug)
			{
				return func();
			}

			var stopwatch = Stopwatch.StartNew();
			var result = func();
			stopwatch.Stop();
			Console.WriteLine($"{caption} execution time: {stopwatch.ElapsedMilliseconds} ms");
			return result;
		}
	}
}
