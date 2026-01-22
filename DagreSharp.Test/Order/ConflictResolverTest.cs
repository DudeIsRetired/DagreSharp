using DagreSharp.GraphLibrary;
using DagreSharp.Order;

namespace DagreSharp.Test.Order
{
	public class ConflictResolverTest
	{
		private readonly Graph cg = new();

		[Fact]
		public void ReturnsBackNodesUnchangedWhenNoConstraintsExist()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 2, Weight = 3 },
				new("b") { BaryCenter = 1, Weight = 2 },
			};

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Equal(2, result.Count);

			var me1 = result[0];
			Assert.Equal(["a"], me1.Vs);
			Assert.Equal(0, me1.Index);
			Assert.Equal(2, me1.BaryCenter);
			Assert.Equal(3, me1.Weight);

			var me2 = result[1];
			Assert.Equal(["b"], me2.Vs);
			Assert.Equal(1, me2.Index);
			Assert.Equal(1, me2.BaryCenter);
			Assert.Equal(2, me2.Weight);
		}

		[Fact]
		public void ReturnsBackNodesUnchangedWhenNoConflictsExist()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 2, Weight = 3 },
				new("b") { BaryCenter = 1, Weight = 2 },
			};

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Equal(2, result.Count);

			var me1 = result[0];
			Assert.Equal(["a"], me1.Vs);
			Assert.Equal(0, me1.Index);
			Assert.Equal(2, me1.BaryCenter);
			Assert.Equal(3, me1.Weight);

			var me2 = result[1];
			Assert.Equal(["b"], me2.Vs);
			Assert.Equal(1, me2.Index);
			Assert.Equal(1, me2.BaryCenter);
			Assert.Equal(2, me2.Weight);
		}

		[Fact]
		public void CoalescesNodesWhenThereIsAConflict()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 2, Weight = 3 },
				new("b") { BaryCenter = 1, Weight = 2 },
			};
			cg.SetEdge("a", "b");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me = result[0];
			Assert.Equal(["a", "b"], me.Vs);
			Assert.Equal(0, me.Index);
			Assert.Equal((double)(3 * 2 + 2 * 1) / (3 + 2), me.BaryCenter);
			Assert.Equal(3 + 2, me.Weight);
		}

		[Fact]
		public void CoalescesNodesWhenThereIsAConflict2()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 4, Weight = 1 },
				new("b") { BaryCenter = 3, Weight = 1 },
				new("c") { BaryCenter = 2, Weight = 1 },
				new("d") { BaryCenter = 1, Weight = 1 }
			};
			cg.SetPath(["a", "b", "c", "d"]);

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me = result[0];
			Assert.Equal(["a", "b", "c", "d"], me.Vs);
			Assert.Equal(0, me.Index);
			Assert.Equal((double)(4 + 3 + 2 + 1) / 4, me.BaryCenter);
			Assert.Equal(4, me.Weight);
		}

		[Fact]
		public void WorksWithMultipleConstraintsForTheSameTarget1()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 4, Weight = 1 },
				new("b") { BaryCenter = 3, Weight = 1 },
				new("c") { BaryCenter = 2, Weight = 1 }
			};
			cg.SetEdge("a", "c");
			cg.SetEdge("b", "c");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me = result[0];
			Assert.True(me.Vs.IndexOf("c") > me.Vs.IndexOf("a"));
			Assert.True(me.Vs.IndexOf("c") > me.Vs.IndexOf("b"));
			Assert.Equal(0, me.Index);
			Assert.Equal((4 + 3 + 2) / 3, me.BaryCenter);
			Assert.Equal(3, me.Weight);
		}

		[Fact]
		public void WorksWithMultipleConstraintsForTheSameTarget2()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 4, Weight = 1 },
				new("b") { BaryCenter = 3, Weight = 1 },
				new("c") { BaryCenter = 2, Weight = 1 },
				new("d") { BaryCenter = 1, Weight = 1 }
			};
			cg.SetEdge("a", "c");
			cg.SetEdge("a", "d");
			cg.SetEdge("b", "c");
			cg.SetEdge("c", "d");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me = result[0];
			Assert.True(me.Vs.IndexOf("c") > me.Vs.IndexOf("a"));
			Assert.True(me.Vs.IndexOf("c") > me.Vs.IndexOf("b"));
			Assert.True(me.Vs.IndexOf("d") > me.Vs.IndexOf("c"));
			Assert.Equal(0, me.Index);
			Assert.Equal((double)(4 + 3 + 2 + 1) / 4, me.BaryCenter);
			Assert.Equal(4, me.Weight);
		}

		[Fact]
		public void DoesNothingToANodeLackingBothABarycenterAndAConstraint()
		{
			var input = new List<BaryCenterResult>
			{
				new("a"),
				new("b") { BaryCenter = 1, Weight = 2 },
			};

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Equal(2, result.Count);

			var me1 = result[0];
			Assert.Equal(["a"], me1.Vs);
			Assert.Equal(0, me1.Index);

			var me2 = result[1];
			Assert.Equal(["b"], me2.Vs);
			Assert.Equal(1, me2.Index);
			Assert.Equal(1, me2.BaryCenter);
			Assert.Equal(2, me2.Weight);
		}

		[Fact]
		public void TreatsANodeWithoutABarycenterAsAlwaysViolatingConstraints1()
		{
			var input = new List<BaryCenterResult>
			{
				new("a"),
				new("b") { BaryCenter = 1, Weight = 2 },
			};
			cg.SetEdge("a", "b");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me1 = result[0];
			Assert.Equal(["a", "b"], me1.Vs);
			Assert.Equal(0, me1.Index);
			Assert.Equal(1, me1.BaryCenter);
			Assert.Equal(2, me1.Weight);
		}

		[Fact]
		public void TreatsANodeWithoutABarycenterAsAlwaysViolatingConstraints2()
		{
			var input = new List<BaryCenterResult>
			{
				new("a"),
				new("b") { BaryCenter = 1, Weight = 2 },
			};
			cg.SetEdge("b", "a");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Single(result);

			var me1 = result[0];
			Assert.Equal(["b", "a"], me1.Vs);
			Assert.Equal(0, me1.Index);
			Assert.Equal(1, me1.BaryCenter);
			Assert.Equal(2, me1.Weight);
		}

		[Fact]
		public void IgnoresEdgesNotRelatedToEntries()
		{
			var input = new List<BaryCenterResult>
			{
				new("a") { BaryCenter = 2, Weight = 3 },
				new("b") { BaryCenter = 1, Weight = 2 },
			};
			cg.SetEdge("c", "d");

			var result = ConflictResolver.ResolveConflicts(input, cg).OrderBy(me => me.Vs[0]).ToList();

			Assert.Equal(2, result.Count);

			var me1 = result[0];
			Assert.Equal(["a"], me1.Vs);
			Assert.Equal(0, me1.Index);
			Assert.Equal(2, me1.BaryCenter);
			Assert.Equal(3, me1.Weight);

			var me2 = result[1];
			Assert.Equal(["b"], me2.Vs);
			Assert.Equal(1, me2.Index);
			Assert.Equal(1, me2.BaryCenter);
			Assert.Equal(2, me2.Weight);
		}

	}
}
