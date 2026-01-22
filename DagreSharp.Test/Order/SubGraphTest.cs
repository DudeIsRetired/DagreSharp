using DagreSharp.GraphLibrary;
using DagreSharp.Order;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DagreSharp.Test.Order
{
	public class SubGraphTest
	{
		private readonly Graph g = new(true, false, true) { ConfigureDefaultEdge = e => e.Weight = 1 };
		private readonly Graph cg = new();

		public SubGraphTest()
		{
			foreach (var v in new[] { 0, 1, 2, 3, 4 })
			{
				g.SetNode(v.ToString(), n => { n.Order = v; });
			}
		}

		[Fact]
		public void SortsFlatSubgraphBasedOnBarycenter()
		{
			g.SetEdge("3", "x");
			g.SetEdge("1", "y", null, e => { e.Weight = 2; });
			g.SetEdge("4", "y");

			foreach (var v in new[] { "x", "y" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["y", "x"], result.Vs);
		}

		[Fact]
		public void PreservesThePosOfANodeYWithoutNeighborsInAFlatSubgraph()
		{
			g.SetEdge("3", "x");
			g.SetNode("y");
			g.SetEdge("1", "z", null, e => { e.Weight = 2; });
			g.SetEdge("4", "z");

			foreach (var v in new[] { "x", "y", "z" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["z", "y", "x"], result.Vs);
		}

		[Fact]
		public void BiasesToTheLeftWithoutReverseBias()
		{
			g.SetEdge("1", "x");
			g.SetEdge("1", "y");

			foreach (var v in new[] { "x", "y" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["x", "y"], result.Vs);
		}

		[Fact]
		public void BiasesToTheRightWithReverseBias()
		{
			g.SetEdge("1", "x");
			g.SetEdge("1", "y");

			foreach (var v in new[] { "x", "y" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg, true);

			Assert.Equal(["y", "x"], result.Vs);
		}

		[Fact]
		public void AggregatesStatsAboutTheSubgraph()
		{
			g.SetEdge("3", "x");
			g.SetEdge("1", "y", null, e => { e.Weight = 2; });
			g.SetEdge("4", "y");

			foreach (var v in new[] { "x", "y" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(2.25, result.BaryCenter);
			Assert.Equal(4, result.Weight);
		}

		[Fact]
		public void CanSortANestedSubgraphWithNoBarycenter()
		{
			g.SetNodes(["a", "b", "c"]);
			g.SetParent("a", "y");
			g.SetParent("b", "y");
			g.SetParent("c", "y");
			g.SetEdge("0", "x");
			g.SetEdge("1", "z");
			g.SetEdge("2", "y");

			foreach (var v in new[] { "x", "y", "z" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["x", "z", "a", "b", "c"], result.Vs);
		}

		[Fact]
		public void CanSortANestedSubgraphWithABarycenter()
		{
			g.SetNodes(["a", "b", "c"]);
			g.SetParent("a", "y");
			g.SetParent("b", "y");
			g.SetParent("c", "y");
			g.SetEdge("0", "a", null, e => { e.Weight = 3; });
			g.SetEdge("0", "x");
			g.SetEdge("1", "z");
			g.SetEdge("2", "y");

			foreach (var v in new[] { "x", "y", "z" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["x", "a", "b", "c", "z"], result.Vs);
		}

		[Fact]
		public void CanSortANestedSubgraphWithNoInEdges()
		{
			g.SetNodes(["a", "b", "c"]);
			g.SetParent("a", "y");
			g.SetParent("b", "y");
			g.SetParent("c", "y");
			g.SetEdge("0", "a");
			g.SetEdge("1", "b");
			g.SetEdge("0", "x");
			g.SetEdge("1", "z");

			foreach (var v in new[] { "x", "y", "z" })
			{
				g.SetParent(v, "movable");
			}

			var result = SubGraph.Sort(g, "movable", cg);

			Assert.Equal(["x", "a", "b", "c", "z"], result.Vs);
		}

		[Fact]
		public void SortsBorderNodesToTheExtremesOfTheSubgraph()
		{
			g.SetEdge("0", "x");
			g.SetEdge("1", "y");
			g.SetEdge("2", "z");
			var sg1Node = g.SetNode("sg1");
			sg1Node.BorderLeft.Add(0, "bl");
			sg1Node.BorderRight.Add(0, "br");

			foreach (var v in new[] { "x", "y", "z", "bl", "br" })
			{
				g.SetParent(v, "sg1");
			}

			var result = SubGraph.Sort(g, "sg1", cg);

			Assert.Equal(["bl", "x", "y", "z", "br"], result.Vs);
		}

		[Fact]
		public void AssignsABarycenterToASubgraphBasedOnPreviousBorderNodes()
		{
			g.SetNode("bl1", n => { n.Order = 0; });
			g.SetNode("br1", n => { n.Order = 1; });
			g.SetEdge("bl1", "bl2");
			g.SetEdge("br1", "br2");

			foreach (var v in new[] { "bl2", "br2" })
			{
				g.SetParent(v, "sg");
			}

			var sgNode = g.SetNode("sg");
			sgNode.BorderLeft.Add(0, "bl2");
			sgNode.BorderRight.Add(0, "br2");

			var result = SubGraph.Sort(g, "sg", cg);

			Assert.Equal(2, result.Vs.Count);
			Assert.Equal(["bl2", "br2"], result.Vs);
			Assert.Equal(0.5, result.BaryCenter);
			Assert.Equal(2, result.Weight);
		}

		[Fact]
		public void SortsNodesByBarycenter()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0, BaryCenter = 2, Weight = 3 },
				new(["b"]) { Index = 1, BaryCenter = 1, Weight = 2 },
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["b", "a"], result.Vs);
			Assert.Equal((double)(2 * 3 + 1 * 2) / (3 + 2), result.BaryCenter);
			Assert.Equal(3+2, result.Weight);
		}

		[Fact]
		public void CanSortSuperNodes()
		{
			var input = new List<MappedEntry>
			{
				new(["a", "c", "d"]) { Index = 0, BaryCenter = 2, Weight = 3 },
				new(["b"]) { Index = 1, BaryCenter = 1, Weight = 2 },
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["b", "a", "c", "d"], result.Vs);
			Assert.Equal((double)(2 * 3 + 1 * 2) / (3 + 2), result.BaryCenter);
			Assert.Equal(3 + 2, result.Weight);
		}

		[Fact]
		public void BiasesToTheLeftByDefault()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0, BaryCenter = 1, Weight = 1 },
				new(["b"]) { Index = 1, BaryCenter = 1, Weight = 1 },
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["a", "b"], result.Vs);
			Assert.Equal(1, result.BaryCenter);
			Assert.Equal(2, result.Weight);
		}

		[Fact]
		public void BiasesToTheRightIfBiasRightTrue()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0, BaryCenter = 1, Weight = 1 },
				new(["b"]) { Index = 1, BaryCenter = 1, Weight = 1 },
			};

			var result = SubGraph.Sort(input, true);

			Assert.Equal(["b", "a"], result.Vs);
			Assert.Equal(1, result.BaryCenter);
			Assert.Equal(2, result.Weight);
		}

		[Fact]
		public void CanSortNodesWithoutABarycenter()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0, BaryCenter = 2, Weight = 1 },
				new(["b"]) { Index = 1, BaryCenter = 6, Weight = 1 },
				new(["c"]) { Index = 2 },
				new(["d"]) { Index = 3, BaryCenter = 3, Weight = 1 }
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["a", "d", "c", "b"], result.Vs);
			Assert.Equal((double)(2 + 6 + 3) / 3, result.BaryCenter);
			Assert.Equal(3, result.Weight);
		}

		[Fact]
		public void CanHandleNoBarycentersForAnyNodes()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0 },
				new(["b"]) { Index = 3 },
				new(["c"]) { Index = 2 },
				new(["d"]) { Index = 1 }
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["a", "d", "c", "b"], result.Vs);
		}

		[Fact]
		public void CanHandleABarycenterOf0()
		{
			var input = new List<MappedEntry>
			{
				new(["a"]) { Index = 0, BaryCenter = 0, Weight = 1 },
				new(["b"]) { Index = 3 },
				new(["c"]) { Index = 2 },
				new(["d"]) { Index = 1 }
			};

			var result = SubGraph.Sort(input);

			Assert.Equal(["a", "d", "c", "b"], result.Vs);
			Assert.Equal(0, result.BaryCenter);
			Assert.Equal(1, result.Weight);
		}

	}
}
