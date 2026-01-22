using DagreSharp.GraphLibrary;
using System.Collections.Generic;
using System.Linq;

namespace DagreSharp
{
	public class NestingGraph
	{
		/*
		* A nesting graph creates dummy nodes for the tops and bottoms of subgraphs,
		* adds appropriate edges to ensure that all cluster nodes are placed between
		* these boundries, and ensures that the graph is connected.
		*
		* In addition we ensure, through the use of the minlen property, that nodes
		* and subgraph border nodes to not end up on the same rank.
		*
		* Preconditions:
		*
		*    1. Input graph is a DAG
		*    2. Nodes in the input graph has a minlen attribute
		*
		* Postconditions:
		*
		*    1. Input graph is connected.
		*    2. Dummy nodes are added for the tops and bottoms of subgraphs.
		*    3. The minlen attribute for nodes is adjusted to ensure nodes do not
		*       get placed on the same rank as subgraph border nodes.
		*
		* The nesting graph idea comes from Sander, "Layout of Compound Directed
		* Graphs."
		*/
		public static void Run(Graph g)
		{
			var root = Util.AddDummyNode(g, DummyType.Root, "_root");
			var depths = TreeDepths(g);
			var height = depths.Values.Count > 0 ? depths.Values.Max() - 1 : 0;
			var nodeSep = 2 * height + 1;
			var weight = 0;

			g.OptionsInternal.NestingRoot = root.Id;

			// Multiply minlen by nodeSep to align nodes on non-border ranks.
			foreach (var edge in g.GetEdges())
			{
				edge.MinLength *= nodeSep;
				weight += edge.Weight;
			}

			// Calculate a weight that is sufficient to keep subgraphs vertically compact
			weight += 1;

			// Create border nodes and link them up
			var children = g.GetChildrenInternal().ToList();
			foreach (var child in children)
			{
				DepthFirstSearch(g, root.Id, nodeSep, weight, height, depths, child);
			}

			// Save the multiplier for node layers for later removal of empty border
			// layers.
			g.OptionsInternal.NodeRankFactor = nodeSep;
		}

		private static void DepthFirstSearch(Graph g, string root, int nodeSep, int weight, int height, Dictionary<string, int> depths, Node node)
		{
			var children = g.GetChildrenInternal(node.Id).ToList();
			if (children.Count == 0)
			{
				if (node.Id != root)
				{
					g.SetEdge(root, node.Id, null, e =>
					{
						e.Weight = 0;
						e.MinLength = nodeSep;
					});
				}

				return;
			}

			var top = Util.AddBorderNode(g, "_bt");
			var bottom = Util.AddBorderNode(g, "_bb");

			g.SetParent(top.Id, node.Id);
			node.BorderTop = top.Id;
			g.SetParent(bottom.Id, node.Id);
			node.BorderBottom = bottom.Id;

			foreach (var child in children)
			{
				DepthFirstSearch(g, root, nodeSep, weight, height, depths, child);

				var childTop = !string.IsNullOrEmpty(child.BorderTop) ? child.BorderTop : child.Id;
				var childBottom = !string.IsNullOrEmpty(child.BorderBottom) ? child.BorderBottom : child.Id;
				var thisWeight = !string.IsNullOrEmpty(child.BorderTop) ? weight : 2 * weight;
				var minlen = childTop != childBottom ? 1 : height - depths[node.Id] + 1;

				g.SetEdgeInternal(top.Id, childTop, null, e =>
				{
					e.Weight = thisWeight;
					e.MinLength = minlen;
					e.IsNestingEdge = true;
				});

				g.SetEdgeInternal(childBottom, bottom.Id, null, e =>
				{
					e.Weight = thisWeight;
					e.MinLength = minlen;
					e.IsNestingEdge = true;
				});
			}

			if (!g.HasParent(node.Id))
			{
				g.SetEdge(root, top.Id, null, e =>
				{
					e.Weight = 0;
					e.MinLength = height + depths[node.Id];
				});
			}
		}

		private static Dictionary<string, int> TreeDepths(Graph g)
		{
			var depths = new Dictionary<string, int>();

			void DepthFirstSearch(string nodeId, int depth)
			{
				var children = g.GetChildrenInternal(nodeId);

				foreach (var child in children)
				{
					DepthFirstSearch(child.Id, depth + 1);
				}

				depths.Add(nodeId, depth);
			}

			foreach (var child in g.GetChildrenInternal())
			{
				DepthFirstSearch(child.Id, 1);
			}

			return depths;
		}

		public static void Cleanup(Graph g)
		{
			var options = g.OptionsInternal;
			if (!string.IsNullOrEmpty(options.NestingRoot))
			{
				g.RemoveNode(options.NestingRoot);
				options.NestingRoot = null;
			}
			
			var nestingEdges = g.GetEdges().Where(e => e.IsNestingEdge).ToArray();

			foreach (var e in nestingEdges)
			{
				g.RemoveEdge(e);
			}
		}
	}
}
