using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IntervalTreeLib;
using System.Diagnostics;

namespace IntervalTreeTest {
  [TestClass]
  public class IntervalTreeOfIntUnitTest {
    [TestMethod]
    public void CreateEmptyIntervalTree() {
      IntervalTree<int,int> emptyTree = new IntervalTree<int,int>();
      Assert.IsNotNull(emptyTree);
    }

    [TestMethod]
    public void BuildEmptyIntervalTree() {
      IntervalTree<int,int> emptyTree = new IntervalTree<int,int>();
      emptyTree.Build();
    }

    [TestMethod]
    public void TestSeparateIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(20, 30, 200);

      var result = tree.Get(5);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(100, result[0]);
    }

    [TestMethod]
    public void TestSeparateIntervalsIntersectionsList() {
      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(20, 30, 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(0, result.Count, "Expect zero intersection because the interval do not overlaps");
    }

    [TestMethod]
    public void TwoIntersectingIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(3, 30, 200);

      var result = tree.Get(5);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(100, result[0]);
      Assert.AreEqual(200, result[1]);
    }

    [TestMethod]
    public void TestIntersectingIntervalsIntersectionsList() {
      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(0, 10, 100);
      tree.AddInterval(3, 30, 200);

      var result = tree.GetIntersections().ToList();
      Assert.AreEqual(1, result.Count, "Expect one intersection because the intervals overlaps");

      int totalIntervals = result.Select(x => x.Count).Sum();
      Assert.AreEqual(2, totalIntervals, "Expect two intervals");
    }

    
    [TestMethod]
    public void TwoNonIntersectingIntervalsWithCommonIntervalThatIntersecBoth() {
      IntervalTree<int, int> tree = new IntervalTree<int, int>();
      tree.AddInterval(8, 9, 100);
      tree.AddInterval(8, 11, 200);
      tree.AddInterval(9, 10, 300);

      var intersections = tree.GetIntersections().ToList();
      Assert.AreEqual(2, intersections.Count);
    }

    [TestMethod]
    public void SpeedTestIntersectingIntervals_GetPoint() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100*1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(50*1000);
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [TestMethod]
    public void SpeedTestIntersectingIntervals_GetRange() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }
      tree.Build();

      Stopwatch stopWatch = Stopwatch.StartNew();
      var result = tree.Get(50 * 1000, 52 * 1000);
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 100);
    }

    [TestMethod]
    public void SpeedTestBuild100kIntervals() {
      IntervalTree<int,int> tree = new IntervalTree<int,int>();
      for (int i = 0; i < 100 * 1000; i++) {
        tree.AddInterval(i, i + 200, i);
      }


      Stopwatch stopWatch = Stopwatch.StartNew();
      tree.Build();
      stopWatch.Stop();

      Assert.IsTrue(stopWatch.ElapsedMilliseconds < 3 * 1000, "Build took more then 4s - it took " + stopWatch.Elapsed);
    }
  }
}
