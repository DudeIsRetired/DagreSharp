using DagreSharp.GraphLibrary;
using System.Collections.Generic;

namespace DagreSharp.Order
{
	public static class LayerGraph
	{
		public delegate IReadOnlyCollection<Edge> RelationShipFunc(string v, string u);
		/*
		 * Constructs a graph that can be used to sort a layer of nodes. The graph will
		 * contain all base and subgraph nodes from the request layer in their original
		 * hierarchy and any edges that are incident on these nodes and are of the type
		 * requested by the "relationship" parameter.
		 *
		 * Nodes from the requested rank that do not have parents are assigned a root
		 * node in the output graph, which is set in the root graph attribute. This
		 * makes it easy to walk the hierarchy of movable nodes during ordering.
		 *
		 * Pre-conditions:
		 *
		 *    1. Input graph is a DAG
		 *    2. Base nodes in the input graph have a rank attribute
		 *    3. Subgraph nodes in the input graph has minRank and maxRank attributes
		 *    4. Edges have an assigned weight
		 *
		 * Post-conditions:
		 *
		 *    1. Output graph has all nodes in the movable rank with preserved
		 *       hierarchy.
		 *    2. Root nodes in the movable layer are made children of the node
		 *       indicated by the root attribute of the graph.
		 *    3. Non-movable nodes incident on movable nodes, selected by the
		 *       relationship parameter, are included in the graph (without hierarchy).
		 *    4. Edges incident on movable nodes, selected by the relationship
		 *       parameter, are added to the output graph.
		 *    5. The weights for copied edges are aggregated as need, since the output
		 *       graph is not a multi-graph.
		 */
		public static Graph Build(Graph g, int rank, RelationShipFunc relationshipFunc, IEnumerable<Node> nodesWithRank = null)
		{
			if (nodesWithRank == null)
			{
				nodesWithRank = g.GetNodes();
			}

			var root = CreateRootNode(g);
			var result = new Graph(true, false, true);
			result.OptionsInternal.Root = root;
			//result.ConfigureDefaultNode = n => g.GetNode(n.Id);

			foreach (var node in nodesWithRank)
			{
				var parent = g.FindParent(node.Id);

				if (node.Rank == rank || node.MinRank <= rank && rank <= node.MaxRank)
				{
					result.SetNode(node);
					result.SetParent(node.Id, string.IsNullOrEmpty(parent) ? root : parent);

					// This assumes we have only short edges!
					var relationShip = relationshipFunc(node.Id, null);
					foreach (var e in relationShip)
					{
						var u = e.From == node.Id ? e.To : e.From;
						var edge = result.FindEdge(u, node.Id);
						var weight = edge != null ? edge.Weight : 0;

						if (!result.HasNode(u))
						{
							var uNode = g.GetNodeInternal(u);
							result.SetNode(uNode);
						}

						result.SetEdge(u, node.Id, null, x => { x.Weight = e.Weight + weight; });
					}

					if (node.MinRank.HasValue)
					{
						result.SetNode(node, n =>
						{
							n.BorderLeft.Add(rank, node.BorderLeft[rank]);
							n.BorderRight.Add(rank, node.BorderRight[rank]);
						});
					}
				}
			}

			return result;
		}

		private static string CreateRootNode(Graph g)
		{
			string v;
			while (g.HasNode((v = Util.UniqueId("_root")))) ;
			return v;
		}
	}
}
