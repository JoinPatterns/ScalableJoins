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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using JC = Microsoft.Research.Joins.Classic;

using Microsoft.Research.Joins;

namespace Philosophers {
  using Fork = Asynchronous.Channel;
  interface ITable<Forks> {
    Forks AcquireForks(int n);
    void ReleaseForks(Forks forks);
    void Init(int n);
  }

  class LockTable : ITable<int> {
    public class Lock {
      public void Acq() {
        Monitor.Enter(this);
      }
      public void Rel() {
        Monitor.Exit(this);
      }
    }

    private Lock[] mLocks;
    private int mSize = -1;

    public int AcquireForks(int n) {
      int i, j;
      if (n + 1 < mSize) {
        i = n; j = n + 1;
      } else if (n == mSize - 1) {
        i = 0; j = n;
      } else {
        throw new Exception();
      }

      mLocks[i].Acq();
      mLocks[j].Acq();

      return n;
    }

    public void ReleaseForks(int n) {
      int i, j;
      if (n + 1 < mSize) {
        i = n; j = n + 1;
      } else if (n == mSize - 1) {
        i = 0; j = n;
      } else {
        throw new Exception();
      }

      mLocks[j].Rel();
      mLocks[i].Rel();
    }

    public void Init(int n) {
      mLocks = new Lock[n];
      for (var i = 0; i < n; i++) mLocks[i] = new Lock();
      mSize = n;
    }
  }

  // uses one channel per fork
  namespace Classic
  {
    using Microsoft.Research.Joins.Classic;
    using Fork = Microsoft.Research.Joins.Classic.Asynchronous.Channel;
  
    class Table : ITable<int> 
    {
        private Fork[] Forks;
        Synchronous<int>.Channel[] Hungry;
        Join j;

        public int AcquireForks(int n)
        {
            return Hungry[n]();
        }

        public void ReleaseForks(int i)
        {
            var left = Forks[i]; var right = Forks[(i + 1) % Forks.Length];
            if ((i + 1) < Forks.Length)
            {   right(); left(); }
            else
            {
                left(); right();
            }
        }

        public void Init(int numPhil)
        {
            var numForks = numPhil;
            j = Join.Create(numPhil + numForks);
            Forks = new Fork[numForks];
            Hungry = new Synchronous<int>.Channel[numPhil];
            for (int i = 0; i < numForks; i++)
            {
                j.Initialize(out Forks[i]);

            }
            for (int i = 0; i < numPhil; i++)
            {
                j.Initialize(out Hungry[i]);
            }

            for (int i = 0; i < numPhil; i++)
            {
                var phil = i;
                var left = Forks[i]; var right = Forks[(i + 1) % numForks];
                j.When(Hungry[i]).And(left).And(right).Do(() =>
                {
                    return phil;
                });
            }

            for (int i = 0; i < numForks; i++)
            {

                Forks[i]();
            }
        }
    }
    // like Table<DICT> but uses just two channels for all the forks. One channel would do, but this requires non-linear patterns.
    class Table2 : ITable<Pair<Fork, Fork>> { 
        private Fork LeftFork;
        private Fork RightFork;
        Synchronous<Pair<Fork, Fork>>.Channel[] Hungry;
        Join j;

        public Pair<Fork, Fork> AcquireForks(int n)
        {
            return Hungry[n]();
        }

        public void ReleaseForks(Pair<Fork, Fork> forks)
        {
            forks.Fst(); forks.Snd();
        }

        public void Init(int numPhil)
        {
            var numForks = numPhil;
            j = Join.Create(numPhil + numForks);

            Hungry = new Synchronous<Pair<Fork, Fork>>.Channel[numPhil];
            j.Initialize(out LeftFork);
            j.Initialize(out RightFork);
            for (int i = 0; i < numForks; i++)
            {
                if (i % 2 == 0) { LeftFork(); }
                else { RightFork(); };
            }
            for (int i = 0; i < numPhil; i++)
            {
                j.Initialize(out Hungry[i]);
            }

            for (int i = 0; i < numPhil; i++)
            {
                var phil = i;
                j.When(Hungry[i]).And(LeftFork).And(RightFork).Do(() =>
                {
                    return new Pair<Fork, Fork>(LeftFork, RightFork);
                });
            }
        }
    }

  }
 
  // uses one channel per fork
  class Table<DICT> : ITable<int> where DICT : struct, Join.IJoinFactory
  {
      private Fork[] Forks;
      Synchronous<int>.Channel[] Hungry;
      Join j;

      public int AcquireForks(int n)
      {
          return Hungry[n]();
      }

      public void ReleaseForks(int i )
      {
          var left = Forks[i]; var right = Forks[(i + 1) % Forks.Length];
          if ((i + 1) < Forks.Length)
          {   right(); left(); }
          else
          {
              left(); right();
          }
      }

      public void Init(int numPhil)
      {
          var numForks = numPhil;
          j = Join.Create<DICT>(numPhil + numForks);
          Forks = new Fork[numForks];
          Hungry = new Synchronous<int>.Channel[numPhil];
          for (int i = 0; i < numForks; i++)
          {
              j.Initialize(out Forks[i]);
             
          }
          for (int i = 0; i < numPhil; i++)
          {
              j.Initialize(out Hungry[i]);
          }

          for (int i = 0; i < numPhil; i++)
          {
              var phil = i;
              var left = Forks[i]; var right = Forks[(i + 1) % numForks];
              j.When(Hungry[i]).And(left).And(right).Do(() =>
              {
                  return phil;
              });
          }

          for (int i = 0; i < numForks; i++)
          {
              
              Forks[i]();
          }
      }
  }
  // like Table<DICT> but uses just two channels for all the forks. One channel would do, but this requires non-linear patterns.
  class Table2<DICT> : ITable<Pair<Fork, Fork>> where DICT : struct, Join.IJoinFactory
  {
      private Fork LeftFork;
      private Fork RightFork;
      Synchronous<Pair<Fork, Fork>>.Channel[] Hungry;
      Join j;

      public Pair<Fork, Fork> AcquireForks(int n)
      {
          return Hungry[n]();
      }

      public void ReleaseForks(Pair<Fork, Fork> forks)
      {
          forks.Fst(); forks.Snd();
      }

      public void Init(int numPhil)
      {
          var numForks = numPhil;
          j = Join.Create<DICT>(numPhil + 2);
          
          Hungry = new Synchronous<Pair<Fork, Fork>>.Channel[numPhil];
          j.Initialize(out LeftFork);
          j.Initialize(out RightFork);
          for (int i = 0; i < numForks; i++)
          {
              if (i % 2 == 0) { LeftFork();}
              else { RightFork(); };
          }
          for (int i = 0; i < numPhil; i++)
          {
              j.Initialize(out Hungry[i]);
          }

          for (int i = 0; i < numPhil; i++)
          {
              var phil = i;
              j.When(Hungry[i]).And(LeftFork).And(RightFork).Do(() =>
              {
                  return new Pair<Fork, Fork>(LeftFork, RightFork);
              });
          }
      }
  }
 

  class Program {


    const int TotalIterations = 100000;
    static int SpinsInCritical = 0; 
    static int ParPerSeq = 0;
    const int Trials = 5;

    static long SeqTrial<Forks, T>() where T : ITable<Forks>, new() {
      var l = new T();
      l.Init(2);
      var thread = new Thread(delegate() {
        for (int j = 0; j < TotalIterations; j++) {
          var forks = l.AcquireForks(0);
          // EAT
          Thread.SpinWait(SpinsInCritical);
          l.ReleaseForks(forks);
          // THINK
          Thread.SpinWait(SpinsInCritical * ParPerSeq);
        }
      });

      GC.Collect();

      var watch = Stopwatch.StartNew();
      thread.Start();
      thread.Join();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }

    static long DupTrial<Forks, T>() where T : ITable<Forks>, new() {
      var threads = new Thread[4];
      var l = new T();
      l.Init(4);
      for (int i = 0; i < 4; i++) {
        var k = i;
        threads[i] = new Thread(delegate() {
          for (int j = 0; j < TotalIterations / 4; j++) {
            var forks = l.AcquireForks(k);
            // EAT
            Thread.SpinWait(SpinsInCritical);
            l.ReleaseForks(forks);
            // THINK
            Thread.SpinWait(SpinsInCritical * ParPerSeq);
          }
        });
      }

      GC.Collect();

      var watch = Stopwatch.StartNew();
      threads[0].Start();
      threads[2].Start();
      threads[0].Join();
      threads[2].Join();
      threads[1].Start();
      threads[3].Start();
      threads[1].Join();
      threads[3].Join();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }

    static long Trial<Forks,T>(int n) where T : ITable<Forks>,  new() {
      var threads = new Thread[n];
      var l = new T();
      l.Init(n);
      for (int i = 0; i < n; i++) {
        var k = i;
        threads[i] = new Thread(delegate() {
          var SpinsInCritical = Program.SpinsInCritical;
          var ParPerSeq = Program.ParPerSeq;
          for (int j = 0; j < TotalIterations / n ; j++) {
            var forks = l.AcquireForks(k); 
            // EAT
            Thread.SpinWait(SpinsInCritical);  
            l.ReleaseForks(forks); 
            // THINK
            Thread.SpinWait(SpinsInCritical * ParPerSeq);
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


 

    static void DoTest<Forks, T>(string name = null) where T : ITable<Forks>, new() {
      name = (name == null) ? typeof(T).FullName : name;
      var series = new Dictionary<int, long>();
      Reporting.data.Add(name, series);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(name);

      long sum = 0;
      Reporting.AddX(1);
      SeqTrial<Forks, T>(); // jit
      for (int i = 0; i < Trials; i++) sum += SeqTrial<Forks, T>();
      long time = sum / Trials;
      series.Add(1, time);
      Console.Error.WriteLine(time);

      //long sum = 0;
      //long time;

      for (var n = 2; n <= 2 * Environment.ProcessorCount  ; n++) {
      //for (var n = 2; n <= 2; n++) {
        Reporting.AddX(n);
        sum = 0;
        Trial<Forks, T>(n); // ensure jitted
        for (int i = 0; i < Trials; i++) sum += Trial<Forks, T>(n);
        time = sum / Trials;
        series.Add(n, time);
        Console.Error.WriteLine(time);
      }
    }

    static void PerfTesting() {
      long sum;

      //sum = 0;
      //for (int i = 0; i < Trials; i++)
      //  sum += SeqTrial<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table<Join.SJoin>>();
      //Console.WriteLine("Sequential   {0}", sum / Trials);

      //sum = 0;
      //for (int i = 0; i < Trials; i++)
      //  sum += DupTrial<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table<Join.SJoin>>();
      //Console.WriteLine("Optimal quad {0}", sum / Trials);

      for (int n = 4; n <= 4; n++) {
        sum = 0;
        for (int i = 0; i < Trials; i++)
          sum += Trial<int, Table<Join.Scalable>>(n);
        Console.WriteLine("{0} {1}", n, sum / Trials);
      }
    }


   
    static void Main(string[] args) {

      // allow control-c to report data before exiting
      Console.CancelKeyPress += (s,a) => Reporting.ReportData();


      if (!(args.Length == 2 && Int32.TryParse(args[0], out SpinsInCritical) && Int32.TryParse(args[1], out ParPerSeq))) {
        SpinsInCritical = 0;
        ParPerSeq = 0;
      };


      //PerfTesting();

  
   
      DoTest<int, Classic.Table>("LB-Joins");
    // DoTest<int, Table<Join.LBJoin>>("LB-Joins-mod");
      DoTest<int, Table<Join.ScalableNonOpt>>("S-Joins");
      DoTest<int, Table<Join.Scalable>>("S-Joins-opt");
      DoTest<int, LockTable>("Direct-locks");

/*
      DoTest<JC.Pair<JC.Asynchronous.Channel, JC.Asynchronous.Channel>, Classic.Table2>("Classic.Table2");
      DoTest<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table2<Join.LBJoin>>("LockBased.Table2");
     // DoTest<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table2<Join.SJoin>>("Scalable.Table2");
      DoTest<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table2<Join.SPaper>>("Paper.Table2");
      DoTest<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table2<Join.SPaperFP>>("PaperFP.Table2");
 */
  /*
     if (System.Environment.ProcessorCount <= 8)
      {
          DoTest<Pair<Asynchronous.Channel, Asynchronous.Channel>, Table2<Join.LFJoin>>("LockFree.Table2");
      }
      else { } // takes too long to measure
    */

      Reporting.ReportData();
    }
  }
}
