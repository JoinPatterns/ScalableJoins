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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Joins;
using System.Threading;

namespace Regression {
  class Program {
    class TestPayload {
      public int cnt;

      public TestPayload() {
        cnt = 0;
      }

      public void Received() {
        //Interlocked.Increment(ref cnt);
        cnt++;
      }

      public override string ToString() {
        return cnt.ToString();
      }
    }

    static void TestSequentialQueue<DICT>() where DICT : struct, Join.IJoinFactory {
      GC.Collect();
      Console.WriteLine("Testing sequential message queueing");

      Asynchronous.Channel<TestPayload> Put;
      Synchronous.Channel Get;

      Join j = Join.Create<DICT>(4);
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Get).And(Put).Do(delegate(TestPayload m) {
        m.Received();
      });

      int attempts = 1000000;
      var ms = new TestPayload[attempts];

      for (var i = 0; i < attempts; i++) {
        ms[i] = new TestPayload();
      }

      DateTime start, mid, finish;

      start = DateTime.Now;
      for (var i = 0; i < attempts; i++) {
        Put(ms[i]);
      }

      mid = DateTime.Now;
      for (var i = 0; i < attempts; i++) {
        Get();
      }

      finish = DateTime.Now;
      for (var i = 0; i < attempts; i++) {
        if (ms[i].cnt != 1) {
          Console.Write("FAULT: message received ");
          Console.Write(ms[i].cnt);
          Console.WriteLine(" times.");
          return;
        }
      }

      Console.WriteLine("... successful:");
      Console.WriteLine(mid.Subtract(start));
      Console.WriteLine(finish.Subtract(mid));
    }

    static void TestConcurrentQueue<DICT>() where DICT : struct, Join.IJoinFactory {
      GC.Collect();
      Console.WriteLine("Testing concurrent sending, sequential receiving");

      Asynchronous.Channel<TestPayload> Put;
      Synchronous.Channel Get;

      Join j = Join.Create<DICT>(4);
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Get).And(Put).Do(delegate(TestPayload m) {
        m.Received();
      });

      int threads = 4;
      int attempts = 1000000;
      var ms = new TestPayload[attempts];

      for (var i = 0; i < attempts; i++) {
        ms[i] = new TestPayload();
      }

      var ts = new Thread[threads];

      DateTime start, mid, finish;

      start = DateTime.Now;
      for (var t = 0; t < threads; t++) {
        ts[t] = new Thread(delegate() {
          for (var i = 0; i < attempts; i++) {
            Put(ms[i]);
          }
        });
        ts[t].Start();
      }

      for (var t = 0; t < threads; t++) {
        ts[t].Join();
      }
      mid = DateTime.Now;
      
      for (var i = 0; i < attempts * threads; i++) {
        Get();
      }
      finish = DateTime.Now;

      for (var i = 0; i < attempts; i++) {
        if (ms[i].cnt != threads) {
          Console.Write("FAULT: message received ");
          Console.Write(ms[i].cnt);
          Console.WriteLine(" times.");
          return;
        }
      }

      Console.WriteLine("... successful:");
      Console.WriteLine(mid.Subtract(start));
      Console.WriteLine(finish.Subtract(mid));
    }
#warning "not currently used"
    static void TestConcurrentQueueB<DICT>() where DICT : struct, Join.IJoinFactory {
      GC.Collect();
      Console.WriteLine("Testing concurrent sending, following by concurrent receiving");

      Asynchronous.Channel<TestPayload> Put;
      Synchronous.Channel Get;

      Join j = Join.Create<DICT>(4);
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Get).And(Put).Do(delegate(TestPayload m) {
        m.Received();
      });

      int threads = 4;
      int attempts = 100000;
      var ms = new TestPayload[attempts];

      for (var i = 0; i < attempts; i++) {
        ms[i] = new TestPayload();
      }

      var ts = new Thread[threads];

      DateTime start, mid, finish;

      start = DateTime.Now;
      for (var t = 0; t < threads; t++) {
        ts[t] = new Thread(delegate() {
          for (var i = 0; i < attempts; i++) {
            Put(ms[i]);
          }
        });
        ts[t].Start();
      }

      for (var t = 0; t < threads; t++) {
        ts[t].Join();
      }

      mid = DateTime.Now;

      for (var t = 0; t < threads; t++) {
        ts[t] = new Thread(delegate() {
          for (var i = 0; i < attempts; i++) {
            Get();
          }
        });
        ts[t].Start();
      }

      for (var t = 0; t < threads; t++) {
        ts[t].Join();
      }

      finish = DateTime.Now;

      for (var i = 0; i < attempts; i++) {
        if (ms[i].cnt != threads) {
          Console.Write("FAULT: message received ");
          Console.Write(ms[i].cnt);
          Console.WriteLine(" times.");
          return;
        }
      }

      Console.WriteLine("... successful:");
      Console.WriteLine(mid.Subtract(start));
      Console.WriteLine(finish.Subtract(mid));
    }

    static void Main(string[] args) {
     
 
      
      Console.WriteLine("ScalabeNonOpt");
      TestSequentialQueue<Join.ScalableNonOpt>();
      TestConcurrentQueue<Join.ScalableNonOpt>();


      Console.WriteLine("Scalable");
      TestSequentialQueue<Join.Scalable>();
      TestConcurrentQueue<Join.Scalable>();
     


      Console.WriteLine("LockBased");
      TestSequentialQueue<Join.LockBased>();
      TestConcurrentQueue<Join.LockBased>();
     
    }
  }
}
