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

// Test performance using joins as a Semaphore
namespace Semaphore {
  interface ISemaphore {
    void Acquire();
    void Release();
    void Init(int n);
  }

  namespace Classic
  {
    using Microsoft.Research.Joins.Classic;
 
    class JoinSemaphore : ISemaphore {
      Asynchronous.Channel Token;
      Synchronous.Channel Acq;
      Join j;

      public void Acquire() { Acq(); }
      public void Release() { Token(); }
      public void Init(int size) {
        for (int i = 0; i < size; i++) { Token(); }
      }

      public JoinSemaphore() {
        j = Join.Create(2);
        j.Initialize(out Token);
        j.Initialize(out Acq);
        j.When(Acq).And(Token).Do(delegate { });
      }
    }
  }

  

  class JoinSemaphore<DICT> : ISemaphore where DICT : struct, Join.IJoinFactory {
    Asynchronous.Channel Token;
    Synchronous.Channel Acq;
    Join j;

    public void Acquire() { Acq(); }
    public void Release() { Token(); }
    public void Init(int size) {
      for (int i = 0; i < size; i++) { Token(); }
    }

    public JoinSemaphore() {
      j = Join.Create<DICT>(2);
      j.Initialize(out Token);
      j.Initialize(out Acq);
      j.When(Acq).And(Token).Do(delegate {  });
    }
  }

  class BuiltIn : ISemaphore {
    System.Threading.Semaphore s;
    public void Acquire() { s.WaitOne(); }
    public void Release() { s.Release(); }
    public void Init(int size) { s = new System.Threading.Semaphore(size, size); }
  }

  class BuiltInSlim : ISemaphore {
    System.Threading.SemaphoreSlim s;
    public void Acquire() { s.Wait(); }
    public void Release() { s.Release(); }
    public void Init(int size) { s = new System.Threading.SemaphoreSlim(size, size); }
  }

  

  class Program {
    const int TotalIterations = 100000;
    static int SpinsInCritical = 0;
    static int ParPerSeq = 0;
    const int Trials = 3;

    static long Trial<T>(int n) where T : ISemaphore,  new() {
      var threads = new Thread[n];
      var l = new T();
      l.Init(n/2);
      for (int i = 0; i < n; i++) 
        threads[i] = new Thread(delegate() {

          var SpinsInCritical = Program.SpinsInCritical;
          var ParPerSeq = Program.ParPerSeq;

          //var rand = new Random(0);
          for (int j = 0; j < TotalIterations / n; j++) {
            l.Acquire();
            Thread.SpinWait(SpinsInCritical); // critical section
            l.Release();
            //Thread.SpinWait(rand.Next(SpinsInCritical * 10));
            Thread.SpinWait(SpinsInCritical * ParPerSeq);
          }
        });

      GC.Collect();

      var watch = Stopwatch.StartNew();
      for (int i = 0; i < n; i++) threads[i].Start();
      for (int i = 0; i < n; i++) threads[i].Join();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }

    static void DoTest<T>(string name) where T : ISemaphore, new() {
      // This test is based on "The performance of spin lock alternatives for shared memory multiprocessors"
      // http://www.cs.washington.edu/homes/tom/pubs/spinlock.pdf

      Console.Error.WriteLine(name);
      var series = new Dictionary<int,long>();
      Reporting.data.Add(name,series);
      for (var n = 2; n <= System.Environment.ProcessorCount * 2; n++) {
        Reporting.AddX(n);
        long sum = 0;
        for (int i = 0; i < Trials; i++) sum += Trial<T>( n);
        Console.Error.WriteLine("  {0:000} {1:00000}", n, sum / Trials);
        series.Add(n, sum /Trials);
        
      }
    }

    static void Main(string[] args) {

      if (!(args.Length == 2 && Int32.TryParse(args[0], out SpinsInCritical) && Int32.TryParse(args[1], out ParPerSeq))) {
        SpinsInCritical = 0;
        ParPerSeq = 0;
      };
 
      
      var parRatio = (double)ParPerSeq / (1 + (double)ParPerSeq);

      Console.Error.WriteLine("Iterations: {0}  SpinsInCrit: {1}  ParPerSeq:  {2}  Trials:  {3}", TotalIterations, SpinsInCritical, ParPerSeq, Trials);
      Console.Error.WriteLine("Ahmdahl says: {0:00.00} max speedup on {1} cores",
        1 / ((1 - parRatio) + (parRatio / Environment.ProcessorCount)), 
        Environment.ProcessorCount);



      //  DoTest<JoinSemaphore<Join.LFJoin>>();

      DoTest<Classic.JoinSemaphore>("LB-Joins-old");

    // DoTest<JoinSemaphore<Join.LBJoin>>("LB-Joins);

    //  DoTest<JoinSemaphore<Join.SJoin>>();

      DoTest<JoinSemaphore<Join.ScalableNonOpt>>("S-Joins");

      DoTest<JoinSemaphore<Join.Scalable>>("S-Joins-opt");


      //DoTest<JoinSemaphore<Join.SLJoin>>();

      DoTest<BuiltIn>(".NET Sema");

      DoTest<BuiltInSlim>(".NET Slim");

     

      Reporting.ReportData(false); // don't report speedup data, absolute times only
    }
  }
}


