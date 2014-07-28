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
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Research.Joins;
using System.Diagnostics;

// producerconsumer microbenchmark


namespace ProducerConsumer {
  using PAYLOAD = String;
  public static class Constants {

    public static PAYLOAD payload = "666";
  }

  public interface Buffer {
    void put(PAYLOAD s);
    PAYLOAD @get();
  }

  namespace Classic {
    using Microsoft.Research.Joins.Classic;
    public class CPB : Buffer {

      public void put(PAYLOAD s) {
        Put(s);
      }

      public PAYLOAD get() {
        return Get();
      }

      Asynchronous.Channel<PAYLOAD> Put;
      Synchronous<PAYLOAD>.Channel Get;

      public CPB() {
        Join j = Join.Create();
        j.Initialize(out Put);
        j.Initialize(out Get);
        j.When(Get).And(Put).Do(delegate(PAYLOAD s) { return s; });
      }
    }
  }

  public class PB : Buffer {

    public void put(PAYLOAD s) {
      Put(s);
    }

    public PAYLOAD @get() {
      return Get();
    }

    Asynchronous.Channel<PAYLOAD> Put;
    Synchronous<PAYLOAD>.Channel Get;

    public PB() {
      Join j = Microsoft.Research.Joins.Join.Create<Join.LockBased>();
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Get).And(Put).Do(delegate(PAYLOAD s) { return s; });
    }
  }

  
  


  

 
  public class PaperB : Buffer {

    public void put(PAYLOAD s) {
      Put(s);
    }

    public PAYLOAD @get() {
      return Get();
    }

    Asynchronous.Channel<PAYLOAD> Put;
    Synchronous<PAYLOAD>.Channel Get;

    public PaperB() {
      Join j = Join.Create<Join.ScalableNonOpt>(2);
      j.Initialize(out Put);
      j.Initialize(out Get);
      //Console.WriteLine(s);
      j.When(Get).And(Put).Do(delegate(PAYLOAD s) { return s; });
    }
  }

  public class PaperFPB : Buffer {

    public void put(PAYLOAD s) {
      Put(s);
    }

    public PAYLOAD @get() {
      return Get();
    }

    Asynchronous.Channel<PAYLOAD> Put;
    Synchronous<PAYLOAD>.Channel Get;

    public PaperFPB() {
      Join j = Join.Create<Join.Scalable>(2);
      j.Initialize(out Put);
      j.Initialize(out Get);
      //Console.WriteLine(s);
      j.When(Get).And(Put).Do(delegate(PAYLOAD s) { return s; });
    }
  }


  // Monitor-based
  public class MB : Buffer {
    private Queue<PAYLOAD> q = new Queue<PAYLOAD>();

    public void put(PAYLOAD s) {
      Monitor.Enter(this);
      q.Enqueue(s);
      Monitor.Pulse(this);
      Monitor.Exit(this);
    }

    public PAYLOAD @get() {
      Monitor.Enter(this);
      while (q.Count == 0)
        Monitor.Wait(this);
      PAYLOAD s = (q.Dequeue());
      Monitor.Exit(this);
      return s;
    }
  }

  // Simple lock-based
  public class LB : Buffer {
    private Queue<PAYLOAD> q = new Queue<PAYLOAD>();

    public void put(PAYLOAD s) {
      lock (this) {
        q.Enqueue(s);
      }
    }

    public PAYLOAD @get() {
      while (true)
        lock (this) {
          if (q.Count > 0) {
            return q.Dequeue();
          }
        
        //Thread.Yield();
        }
    }
  }

  // lock free buffer using MS ConcurrentQueue (probably incorrect).
  public class CQB : Buffer {
    private ConcurrentQueue<PAYLOAD> q = new ConcurrentQueue<PAYLOAD>();

    public void put(PAYLOAD s) {
      q.Enqueue(s);
    }

    public PAYLOAD @get() {
      PAYLOAD s = default(PAYLOAD);
      while (!q.TryDequeue(out s)) {
        //Thread.Sleep(1);
      }
      return s;
    }
  }

  // lock free buffer using MS ConcurrentBag
  public class CBB : Buffer {
    private ConcurrentBag<PAYLOAD> q = new ConcurrentBag<PAYLOAD>();

    public void put(PAYLOAD s) {
      q.Add(s);
    }

    public PAYLOAD @get() {
      PAYLOAD s = default(PAYLOAD);
      while (!q.TryTake(out s)) {
      }
      ;
      return s;
    }
  }

  // lock free buffer using MS ConcurrentStack
  public class CSB : Buffer {
    private ConcurrentStack<PAYLOAD> q = new ConcurrentStack<PAYLOAD>();

    public void put(PAYLOAD s) {
      q.Push(s);
    }

    public PAYLOAD @get() {
      PAYLOAD s = default(PAYLOAD);
      while (!q.TryPop(out s)) {
      }
      ;
      return s;
    }
  }



  // lock free buffer using MS BlockingCollection
  public class CBC : Buffer {
    private BlockingCollection<PAYLOAD> q = new BlockingCollection<PAYLOAD>();

    public void put(PAYLOAD s) {
      q.Add(s);
    }

    public PAYLOAD @get() {
      return q.Take();
    }
  }


  public class pctest {
    static Buffer b;
    static int n = 1;
    static int maxThreadPairs = System.Environment.ProcessorCount;
    static int total = 100000;
    static int burstsize = 1;
    static int trials = 3;
    static int prodSpins = 0;
    static int consSpins = 0;

    static Asynchronous.Channel<int> producer, consumer;
    static Asynchronous.Channel done;
    static Synchronous.Channel wait;

    static long DoTrial(Buffer newbuf) {
      GC.Collect();
      b = newbuf;

      var sw = new Stopwatch();
      sw.Start();
      
      for (int i = 0; i < n; i++) {
        producer(i);
        consumer(i);
      }
      for (int i = 0; i < n; i++) {
        wait();
        wait();
      }

      sw.Stop();
      return sw.ElapsedMilliseconds;
    }

    static void DoTest(string name, Buffer newbuf) {
      Console.Error.WriteLine(name);
      var series = new Dictionary<int, long>();
      Reporting.data.Add(name, series);

      for (n = 1; n <= maxThreadPairs; n++) {
        var threads = n * 2;
        Reporting.AddX(threads);
        long time = 0;
        var trialTimes = new System.Collections.Generic.List<long>();

        for (int j = 0; j < trials; j++) {
          var ttime = DoTrial(newbuf);
          trialTimes.Add(ttime);
          time += ttime;
        }
        
        var mean = time / trials;
        
        Console.Error.Write("  ");
        Console.Error.Write("{0:000}", n * 2);
        Console.Error.Write(" threads MEAN: ");
        Console.Error.Write("{0:00000}", time);
        series.Add(threads, time);

        
        Console.Error.Write("  SDV: ");
        long sqDifSum = 0;
        foreach (var t in trialTimes)
          sqDifSum += (t - mean) * (t - mean);
        Console.Error.WriteLine("{0:00000}", TimeSpan.FromTicks((long)Math.Sqrt(sqDifSum / trials)).TotalMilliseconds);
      }
    }

   

    static void Main(string[] args) {

      if (!(args.Length == 2 && Int32.TryParse(args[0], out prodSpins) && Int32.TryParse(args[1], out consSpins))) {
        prodSpins = 0;
        consSpins = 0;
      };

      Join j = Join.Create<Join.Scalable>();
      j.Initialize(out producer);
      j.Initialize(out consumer);
      j.Initialize(out done);
      j.Initialize(out wait);
      
      j.When(producer).Do(p => {
        var rand = new Random(0);
        Thread.CurrentThread.Name = "Producer " + p;
        int sent = 0;
        var pSpins = pctest.prodSpins;
        while (sent < total / n) {
          for (int i = 0; i < burstsize; i++)
            b.put(Constants.payload);
          sent += burstsize;
          Thread.SpinWait(rand.Next(pSpins));
        }
        done();
      });
      
      j.When(consumer).Do(c => {
        var rand = new Random(0);
        Thread.CurrentThread.Name = "Consumer " + c;
        var cSpins = pctest.consSpins;
        for (int i = 0; i < total / n; i++) {
          PAYLOAD s = b.@get();
          //if (i%100==0) Console.WriteLine(i);
          Thread.SpinWait(rand.Next(cSpins)); 
        }
        done();
      });
      j.When(wait).And(done).Do(delegate { });

      /*
      Console.WriteLine("monitor-based buffer");
      DoTest(new MB());

      Console.WriteLine("simple lock-based buffer");
      DoTest(new LB());
   */

     
      DoTest("LB-Joins",new Classic.CPB());

      DoTest("S-Joins", new PaperB());

      DoTest("S-Joins-opt", new PaperFPB());
     
      DoTest("ConcQ", new CQB());

    
      DoTest("BlockCol", new CBC());

      DoTest("LB-Joins-mod", new PB());
    
      /*
      Console.WriteLine("concurrent stack");
      DoTest(new CSB());

      Console.WriteLine("concurrent bag");
      DoTest(new CBB());

      Console.WriteLine("LFJoin");
      DoTest(new LFB());
      
      Console.WriteLine("SJoin");
      DoTest(new SB());
      */

     
      //Console.WriteLine("LBSJoin");
      //DoTest(new PBS());

      //Console.WriteLine("LBJoin");
      //DoTest(new PB());

      //Console.WriteLine("spinlock joins");
      //DoTest(new SPB());

      //Console.WriteLine("Comparison");
      //Compare(new LFB(), new SB());

      Reporting.ReportData();

    }
  }
}
