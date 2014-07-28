//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Research.Joins;

// Test performance using joins as a barrier
namespace Lock {
  interface IBarrier {
    void Arrive(int id);
  }

  class SymJoinBarrier<DICT> : IBarrier where DICT : struct, Join.IJoinFactory {
    int cnt = 0;

    Synchronous.Channel Sync;
    Join j;

    public void Arrive(int id) { Sync(); }

    public SymJoinBarrier(int n) {
      j = Join.Create<DICT>(n);
      j.Initialize(out Sync);

      var chord = j.When(Sync);
      for (int i = 0; i < n-1; i++) {
        chord = chord.And(Sync);
      }
      chord.Do(delegate {});
    }
  }

  class BuiltIn : IBarrier {
    private Barrier b;
    public void Arrive(int id) { b.SignalAndWait(); }
    public BuiltIn(int n) {
      b = new Barrier(n);
    }
  }

  class JoinBarrier<DICT> : IBarrier where DICT : struct, Join.IJoinFactory {
    Synchronous.Channel[] Sync;
    Join j;

    public void Arrive(int id) { Sync[id](); }

    public JoinBarrier(int n) {
      j = Join.Create<DICT>(n);
      //Sync = new Synchronous.Channel[n];
      j.Initialize(out Sync, n);

      var chord = j.When(Sync[0]);
      for (int i = 1; i < n; i++) {
        chord = chord.And(Sync[i]);
      }
      chord.Do(delegate { });
    }
  }

  class Program {
    const int TotalIterations = 100000;
    const int Trials = 3;

    static long Trial(IBarrier b, int n) {
      var threads = new Thread[n];
      for (int i = 0; i < n; i++) {
        int tid = i;
        threads[i] = new Thread(delegate() {
          for (int j = 0; j < TotalIterations / n; j++) {
            b.Arrive(tid);
          }
        });
      }
	
      GC.Collect();

      var watch = Stopwatch.StartNew();
      for (int i = 0; i < n; i++) threads[i].Start();
      for (int i = 0; i < n; i++) threads[i].Join();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }

    static void DoTest(string name, Func<int, IBarrier> mkB)
    {
        Console.Error.WriteLine(name);
        var series = new Dictionary<int, long>();
        Reporting.data.Add(name, series);
        for (var n = 1; n <= System.Environment.ProcessorCount * 2; n++)
        {
            Reporting.AddX(n);
            //for (var n = 14; n <= 14; n++) {
            // for (var n = System.Environment.ProcessorCount * 2; n <= System.Environment.ProcessorCount * 2; n++) {
            //for (var n = 2; n <= 2; n++) {
            long sum = 0;
            for (int i = 0; i < Trials; i++) sum += Trial(mkB(n), n);
            Console.Error.WriteLine("  {0:000} {1:000000}", n, sum / Trials);
            series.Add(n, sum / Trials);

        }
    }

    static void Main(string[] args) {
      Console.Error.WriteLine("Iterations: {0}  Trials:  {1}", TotalIterations, Trials);

     
      /*
      Console.WriteLine("Scalable joins: symmetric");
      DoTest((int n) => new SymJoinBarrier<Join.SJoin>(n));

      Console.WriteLine("Scalable joins: asymmetric");
      DoTest((int n) => new JoinBarrier<Join.SJoin>(n));
      */

      DoTest("LB-Joins-mod", (int n) => new JoinBarrier<Join.LockBased>(n));

      DoTest("S-Joins", (int n) => new JoinBarrier<Join.ScalableNonOpt>(n));

      DoTest("S-Joins-opt", (int n) => new JoinBarrier<Join.Scalable>(n));

      DoTest(".NET Barrier", (int n) => new BuiltIn(n));

      /*
      Console.WriteLine("LFJoin: asymmetric");
      DoTest((int n) => new JoinBarrier<Join.LFJoin>(n));
      */

      Reporting.ReportData();
    }
  }
}
