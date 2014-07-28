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

// Test performance using joins as a lock
namespace Lock {
  interface ILock {
    void Acquire();
    void Release();
  }

  namespace Classic {
    using Microsoft.Research.Joins.Classic;
    class JoinLock : ILock  {
      Asynchronous.Channel Unlocked;
      Synchronous.Channel Lock;
      Join j;

      public void Acquire() { Lock(); }
      public void Release() { Unlocked(); }

      public JoinLock() {
        j = Join.Create(2);
        j.Initialize(out Unlocked);
        j.Initialize(out Lock);
        j.When(Lock).And(Unlocked).Do(delegate { });
        Unlocked();
      }
    }
  }

  class JoinLock<DICT> : ILock where DICT : struct, Join.IJoinFactory {
    Asynchronous.Channel Unlocked;
    Synchronous.Channel Lock;
    Join j;

    public void Acquire() { Lock(); }
    public void Release() { Unlocked(); }

    public JoinLock() {
      j = Join.Create<DICT>(2);
      j.Initialize(out Unlocked);
      j.Initialize(out Lock);
      j.When(Unlocked).And(Lock).Do(delegate {  });
      Unlocked();
    }
  }

  class BuiltIn : ILock {
    public void Acquire() { Monitor.Enter(this); }
    public void Release() { Monitor.Exit(this); }
  }

  class Spin : ILock {
    SpinLock mLock = new SpinLock();
    public void Acquire() { bool taken = false; while (!taken) mLock.Enter(ref taken); }
    public void Release() { mLock.Exit(); }
  }

  class Program {
    const int TotalIterations = 100000;
    static int SpinsInCritical = 0;
    static int ParPerSeq = 0;
    const int Trials = 3;

    static long Trial(ILock l, int n) {
      var threads = new Thread[n];
      for (int i = 0; i < n; i++)
        threads[i] = new Thread(delegate() {
          var SpinsInCritical = Program.SpinsInCritical;
          var ParPerSeq = Program.ParPerSeq;
          var rand = new Random(0);
          for (int j = 0; j < TotalIterations / n; j++) {
            l.Acquire();
            Thread.SpinWait(SpinsInCritical); // critical section
            l.Release();
            Thread.SpinWait(rand.Next(SpinsInCritical * ParPerSeq));
            //Thread.SpinWait(SpinsInCritical * ParPerSeq);
          }
        });

      GC.Collect();

      var watch = Stopwatch.StartNew();
      for (int i = 0; i < n; i++) threads[i].Start();
      for (int i = 0; i < n; i++) threads[i].Join();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }

    static void DoTest(string name, ILock l) {
      // This test is based on "The performance of spin lock alternatives for shared memory multiprocessors"
      // http://www.cs.washington.edu/homes/tom/pubs/spinlock.pdf
      Console.Error.WriteLine(name);
      var series = new Dictionary<int, long>();
      Reporting.data.Add(name, series);

      for (var n = 1; n <= System.Environment.ProcessorCount * 2; n++) {

        Reporting.AddX(n);
        long sum = 0;
        for (int i = 0; i < Trials; i++) sum += Trial(l, n);
        Console.Error.WriteLine("  {0:000} {1:00000}", n, sum / Trials);
        series.Add(n, sum / Trials);
      }
    }

    
    

   

    static void Main(string[] args) {

      if (!(args.Length == 2 && Int32.TryParse(args[0], out SpinsInCritical) && Int32.TryParse(args[1], out ParPerSeq))) {
        SpinsInCritical = 0;
        ParPerSeq = 0;
      };
 
    /*
      var parRatio = (double)ParPerSeq / (1 + (double)ParPerSeq);

      Console.WriteLine("Iterations: {0}  SpinsInCrit: {1}  ParPerSeq:  {2}  Trials:  {3}", TotalIterations, SpinsInCritical, ParPerSeq, Trials);
      Console.WriteLine("Ahmdahl says: {0:00.00} max speedup on {1} cores",
        1 / ((1 - parRatio) + (parRatio / Environment.ProcessorCount)), 
        Environment.ProcessorCount);
        */
     
    

      DoTest("LB-Joins", new Classic.JoinLock());
       
      DoTest("S-Joins", new JoinLock<Join.ScalableNonOpt>());
     
      DoTest("S-Joins-opt",new JoinLock<Join.Scalable>());

      DoTest(".NET locks", new BuiltIn());

      DoTest(".NET spin Locks", new Spin());

       Reporting.ReportData();
      
    }
  }
}
