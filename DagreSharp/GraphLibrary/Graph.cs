using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DagreSharp.GraphLibrary
{
	public class Graph
	{
		private const string DEFAULT_EDGE_NAME = "\x00";
		private const string GRAPH_NODE = "\x00";
		private const string EDGE_KEY_DELIM = "\x01";

		private readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>();

		public readonly Dictionary<string, Edge> _edges = new Dictionary<string, Edge>();

		private readonly Dictionary<string, List<Edge>> _inEdges = new Dictionary<string, List<Edge>>();

		private readonly Dictionary<string, List<Edge>> _outEdges = new Dictionary<string, List<Edge>>();

		private readonly Dictionary<string, List<Node>> _predecessors = new Dictionary<string, List<Node>>();

		private readonly Dictionary<string, List<Node>> _successors = new Dictionary<string, List<Node>>();

		private readonly Dictionary<string, string> _parents = new Dictionary<string, string>();

		private readonly Dictionary<string, List<Node>> _children = new Dictionary<string, List<Node>>();

		public IReadOnlyCollection<Node> Nodes { get => _nodes.Values; }

		public IReadOnlyCollection<Edge> Edges { get => _edges.Values; }

		public Action<Node> ConfigureDefaultNode { get; set; }

		public Action<Edge> ConfigureDefaultEdge { get; set; }

		public bool IsDirected { get; }

		public bool IsMultigraph { get; }

		public bool IsCompound { get; }

		public GraphOptions Options { get; }

		public Graph(bool isDirected = true, bool isMultigraph = false, bool isCompound = false)
		{
			IsDirected = isDirected;
			IsMultigraph = isMultigraph;
			IsCompound = isCompound;
			Options = new GraphOptions();

			if (IsCompound)
			{
				_children.Add(GRAPH_NODE, new List<Node>());
			}
		}

		//			/* === Node functions ========== */

		/**
		 * Gets list of nodes without in-edges.
		 * Complexity: O(|V|).
		 */
		public IReadOnlyCollection<Node> GetSources()
		{
			return _nodes.Values.Where(v => _inEdges[v.Id].Count == 0).ToList();
		}

		/**
		 * Gets list of nodes without out-edges.
		 * Complexity: O(|V|).
		 */
		public IReadOnlyCollection<Node> GetSinks()
		{
			return Nodes.Where(v => _outEdges[v.Id].Count == 0).ToList();
		}

		/**
		 * Invokes setNode method for each node in names list.
		 * Complexity: O(|names|).
		 */
		public Graph SetNodes(IEnumerable<string> names, Action<Node> configure = null)
		{
			foreach (var v in names)
			{
				SetNode(v, configure);
			}

			return this;
		}

		/**
		 * Creates or updates the value for the node v in the graph. If label is supplied
		 * it is set as the value for the node. If label is not supplied and the node was
		 * created by this call then the default node label will be assigned.
		 * Complexity: O(1).
		 */
		public Node SetNode(string id, Action<Node> configure = null)
		{
			return SetNode(new Node(id), configure);
		}

		internal Node SetNode(Node node, Action<Node> configure = null)
		{
			if (_nodes.TryGetValue(node.Id, out Node value))
			{
				configure?.Invoke(value);
				return value;
			}

			ConfigureDefaultNode?.Invoke(node);
			configure?.Invoke(node);
			_nodes.Add(node.Id, node);

			if (IsCompound)
			{
				_parents[node.Id] = GRAPH_NODE;
				_children[node.Id] = new List<Node>();
				_children[GRAPH_NODE].Add(node);
			}

			_inEdges[node.Id] = new List<Edge>();
			_predecessors[node.Id] = new List<Node>();
			_outEdges[node.Id] = new List<Edge>();
			_successors[node.Id] = new List<Node>();

			return node;
		}

		/**
		 * Gets the label of node with specified name.
		 * Complexity: O(|V|).
		 */
		public Node GetNode(string id)
		{
			return _nodes[id];
		}

		public Node FindNode(string id)
		{
			if (_nodes.ContainsKey(id))
			{
				return _nodes[id];
			}

			return null;
		}

		/**
		 * Detects whether graph has a node with specified name or not.
		 */
		public bool HasNode(string id)
		{
			return _nodes.ContainsKey(id);
		}

		/**
		 * Remove the node with the name from the graph or do nothing if the node is not in
		 * the graph. If the node was removed this function also removes any incident
		 * edges.
		 * Complexity: O(1).
		 */
		public Graph RemoveNode(string id)
		{
			_nodes.Remove(id);

			if (IsCompound)
			{
				RemoveFromParentsChildList(id);
				_parents.Remove(id);
				foreach (var child in GetChildren(id).ToList())
				{
					SetParent(child.Id);
				}
				_children.Remove(id);
			}

			_inEdges.RemoveAll(id, RemoveEdge);
			_predecessors.Remove(id);
			_outEdges.RemoveAll(id, RemoveEdge);
			_successors.Remove(id);

			return this;
		}

		/**
		 * Sets node p as a parent for node v if it is defined, or removes the
		 * parent for v if p is undefined. Method throws an exception in case of
		 * invoking it in context of noncompound graph.
		 * Average-case complexity: O(1).
		 */
		public Graph SetParent(string id, string parent = null)
		{
			if (!IsCompound)
			{
				throw new InvalidOperationException("Cannot set parent in a non-compound graph");
			}

			if (parent == null)
			{
				parent = GRAPH_NODE;
			}
			else
			{
				for (var ancestor = parent; !string.IsNullOrEmpty(ancestor); ancestor = FindParent(ancestor))
				{
					if (ancestor == id)
					{
						throw new InvalidOperationException("Setting " + parent + " as parent of " + id + " would create a cycle");
					}
				}

				SetNode(parent);
			}

			var node = SetNode(new Node(id));
			RemoveFromParentsChildList(id);
			_parents[id] = parent;
			_children[parent].Add(node);

			return this;
		}

		private void RemoveFromParentsChildList(string id)
		{
			var parentChildren = _children[_parents[id]];
			var node = parentChildren.FirstOrDefault(x => x.Id == id);

			if (node != null)
			{
				_children[_parents[id]].Remove(node);
			}
		}

		/**
		 * Gets parent node for node v.
		 * Complexity: O(1).
		 */
		public string FindParent(string id)
		{
			if (IsCompound)
			{
				if (_parents.TryGetValue(id, out string parent))
				{
					if (parent != GRAPH_NODE)
					{
						return parent;
					}
				}
			}

			return null;
		}

		public bool HasParent(string id)
		{
			if (IsCompound)
			{
				if (_parents.TryGetValue(id, out string parent))
				{
					return parent != GRAPH_NODE;
				}
			}

			return false;
		}

		/**
		 * Gets list of direct children of node v.
		 * Complexity: O(1).
		 */
		public IReadOnlyCollection<Node> GetChildren(string id = GRAPH_NODE)
		{
			if (IsCompound)
			{
				if (!_children.ContainsKey(id))
				{
					return new List<Node>();
				}

				return _children[id];
			}
			else if (id == GRAPH_NODE)
			{
				return _nodes.Values;
			}

			return new List<Node>();
		}

		/**
		 * Return all nodes that are predecessors of the specified node or undefined if node v is not in
		 * the graph. Behavior is undefined for undirected graphs - use neighbors instead.
		 * Complexity: O(|V|).
		 */
		public List<Node> GetPredecessors(string id)
		{
			return _predecessors.TryGetValue(id, out List<Node> value) ? value : new List<Node>();
		}

		/**
		 * Return all nodes that are successors of the specified node or undefined if node v is not in
		 * the graph. Behavior is undefined for undirected graphs - use neighbors instead.
		 * Complexity: O(|V|).
		 */
		public List<Node> GetSuccessors(string id)
		{
			return _successors.TryGetValue(id, out List<Node> values) ? values : new List<Node>();
		}

		/**
		 * Return all nodes that are predecessors or successors of the specified node or undefined if
		 * node v is not in the graph.
		 * Complexity: O(|V|).
		 */
		public List<Node> GetNeighbors(string id)
		{
			var preds = GetPredecessors(id);
			var sucs = GetSuccessors(id);

			return preds.Union(sucs).ToList();
		}

		public bool IsLeaf(string id)
		{
			var neighbors = IsDirected ? GetSuccessors(id) : GetNeighbors(id);
			return neighbors.Count == 0;
		}

		/**
		 * Creates new graph with nodes filtered via filter. Edges incident to rejected node
		 * are also removed. In case of compound graph, if parent is rejected by filter,
		 * than all its children are rejected too.
		 * Average-case complexity: O(|E|+|V|).
		 */
		public Graph FilterNodes(Func<string, bool> filter)
		{
			var copy = new Graph(IsDirected, IsMultigraph, IsCompound);
			copy.Options.CopyFrom(Options);

			foreach (var v in _nodes.Keys)
			{
				if (filter(v))
				{
					copy.SetNode(v);
				}
			}

			foreach (var e in _edges.Values)
			{
				if (copy.HasNode(e.From) && copy.HasNode(e.To))
				{
					copy.SetEdge(e);
				}
			}

			foreach (var e in _edges.Values)
			{
				if (copy.HasNode(e.From) && copy.HasNode(e.To))
				{
					copy.SetEdge(e);
				}
			}

			var parents = new Dictionary<string, string>();
			string findParent(string v)
			{
				var parent = FindParent(v);
				if (string.IsNullOrEmpty(parent) || copy.HasNode(parent))
				{
					parents[v] = parent;
					return parent;
				}
				else if (parents.TryGetValue(parent, out string value))
				{
					return value;
				}
				else
				{
					return findParent(parent);
				}
			}

			if (IsCompound)
			{
				foreach (var node in copy.Nodes)
				{
					copy.SetParent(node.Id, findParent(node.Id));
				}
			}

			return copy;
		}

		//			/* === Edge functions ========== */

		/**
		 * Establish an edges path over the nodes in nodes list. If some edge is already
		 * exists, it will update its label, otherwise it will create an edge between pair
		 * of nodes with label provided or default label if no label provided.
		 * Complexity: O(|nodes|).
		 */
		public Graph SetPath(string[] nodes, Action<Edge> configure = null)
		{
			if (nodes.Length < 2)
			{
				return this;
			}

			string v = nodes[0];
			for (int i = 1; i < nodes.Length; i++)
			{
				var w = nodes[i];
				if (configure != null)
				{
					SetEdge(v, w, null, configure);
				}
				else
				{
					SetEdge(v, w, null);
				}

				v = w;
			}

			return this;
		}

		public Edge SetEdge(string from, string to, string name = null, Action<Edge> configure = null)
		{
			if (!IsDirected && string.Compare(from, to, StringComparison.OrdinalIgnoreCase) > 0)
			{
				(to, from) = (from, to);
			}

			var edge = new Edge(from, to) { Name = name };
			return SetEdge(edge, configure);
		}

		/// <summary>
		/// Creates or updates the label for the edge(v, w) with the optionally supplied
		/// name.If label is supplied it is set as the value for the edge.If label is not
		/// supplied and the edge was created by this call then the default edge label will
		/// be assigned.The name parameter is only useful with multigraphs.
		/// </summary>
		internal Edge SetEdge(Edge edge, Action<Edge> configure = null)
		{
			if (!string.IsNullOrEmpty(edge.Name) && !IsMultigraph)
			{
				throw new InvalidOperationException("Cannot set a named edge when isMultigraph = false");
			}

			edge.Id = EdgeArgsToId(edge.From, edge.To, edge.Name);

			if (_edges.ContainsKey(edge.Id))
			{
				var targetEdge = _edges[edge.Id];
				configure?.Invoke(targetEdge);
				return targetEdge;
			}

			// It didn't exist, so we need to create it. First ensure the nodes exist.
			var fromNode = SetNode(new Node(edge.From));
			var toNode = SetNode(new Node(edge.To));

			ConfigureDefaultEdge?.Invoke(edge);
			configure?.Invoke(edge);

			_edges[edge.Id] = edge;
			_predecessors.InitOrAdd(edge.To, fromNode);
			_successors.InitOrAdd(edge.From, toNode);
			_inEdges[edge.To].Add(edge);
			_outEdges[edge.From].Add(edge);

			return edge;
		}

		/**
		 * Gets the label for the specified edge.
		 * Complexity: O(1).
		 */
		public Edge GetEdge(string from, string to, string name = null)
		{
			var e = EdgeArgsToId(from, to, name);
			return _edges[e];
		}

		internal Edge GetEdge(Edge edge)
		{
			return _edges[edge.Id];
		}

		public Edge FindEdge(string from, string to, string name = null)
		{
			var e = EdgeArgsToId(from, to, name);
			return _edges.TryGetValue(e, out Edge value) ? value : null;
		}

		/**
		 * Detects whether the graph contains specified edge or not. No subgraphs are considered.
		 * Complexity: O(1).
		 */
		public bool HasEdge(string from, string to, string name = null)
		{
			var e = EdgeArgsToId(from, to, name);
			return _edges.ContainsKey(e);
		}

		/**
		 * Removes the specified edge from the graph. No subgraphs are considered.
		 * Complexity: O(1).
		 */
		public Graph RemoveEdge(string from, string to, string name = null)
		{
			var e = EdgeArgsToId(from, to, name);

			if (_edges.ContainsKey(e))
			{
				var edge = _edges[e];
				return RemoveEdge(edge);
			}

			return this;
		}

		internal Graph RemoveEdge(Edge edge)
		{
			if (edge != null)
			{
				//var edgeId = EdgeArgsToId(edge.From, edge.To, edge.Name);
				_edges.Remove(edge.Id);
				_predecessors.RemoveOrClear(edge.To, edge.From);
				_successors.RemoveOrClear(edge.From, edge.To);
				_inEdges[edge.To].Remove(edge);
				_outEdges[edge.From].Remove(edge);
			}

			return this;
		}

		/**
		 * Return all edges that point to the node v. Optionally filters those edges down to just those
		 * coming from node u. Behavior is undefined for undirected graphs - use nodeEdges instead.
		 * Complexity: O(|E|).
		 */
		public IReadOnlyCollection<Edge> GetInEdges(string nodeId, string filterFromNodeId = null)
		{
			var edges = new List<Edge>();

			if (_inEdges.TryGetValue(nodeId, out List<Edge> value))
			{
				edges.AddRange(value);
				if (filterFromNodeId != null)
				{
					edges = edges.Where(e => e.From == filterFromNodeId).ToList();
				}
			}

			return edges;
		}

		/**
		 * Return all edges that are pointed at by node v. Optionally filters those edges down to just
		 * those point to w. Behavior is undefined for undirected graphs - use nodeEdges instead.
		 * Complexity: O(|E|).
		 */
		public IReadOnlyCollection<Edge> GetOutEdges(string nodeId, string filterToNodeId = null)
		{
			var edges = new List<Edge>();

			if (_outEdges.TryGetValue(nodeId, out List<Edge> value))
			{
				edges.AddRange(value);
				if (filterToNodeId != null)
				{
					edges = edges.Where(e => e.To == filterToNodeId).ToList();
				}
			}

			return edges;
		}

		/// <summary>
		/// Returns all edges to or from node from regardless of direction. Optionally filters those edges
		/// down to just those between nodes from and to regardless of direction.
		/// Complexity: O(|E|).
		/// </summary>
		public IReadOnlyCollection<Edge> GetAllEdges(string nodeId, string filterNodeId = null)
		{
			var edges = new List<Edge>(GetInEdges(nodeId, filterNodeId));
			edges.AddRange(GetOutEdges(nodeId, filterNodeId));

			return edges;
		}

		private string EdgeArgsToId(string from, string to, string name = null)
		{
			if (!IsDirected && string.Compare(from, to, StringComparison.OrdinalIgnoreCase) > 0)
			{
				(to, from) = (from, to);
			}

			return from + EDGE_KEY_DELIM + to + EDGE_KEY_DELIM + (name ?? DEFAULT_EDGE_NAME);
		}

		public IReadOnlyCollection<Edge> GetNodeEdges(string from, string to = null)
		{
			var inEdges = GetInEdges(from, to);
			var outEdges = GetOutEdges(from, to);

			return inEdges.Union(outEdges).ToList();
		}

	}
}