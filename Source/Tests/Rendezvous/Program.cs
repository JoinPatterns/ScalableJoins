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


namespace Rendezvous {
 


  
    class Program {
      static int attempts = 100000000;
      static int chan = 2;
      static int threads = 1;

      class TestPayload {
        public int id;
        public int ch;
        public int cnt;
        public int tid;

        public TestPayload(int ch, int t, int i) {
          this.ch = ch;
          this.tid = t;
          this.id = i;
        }
        public TestPayload(int t, int i) {
          this.tid = t;
          this.id = i;
        }
        public override string ToString() {
          return ((((ch * threads) + tid) * attempts) + id).ToString();
        }
        public void Received() {
          cnt++;
        }
      }


      public struct Pair<A, B> {
        readonly A Fst;
        readonly B Snd;
        public Pair(A Fst, B Snd) {
          this.Fst = Fst;
          this.Snd = Snd;
        }
        public override string ToString() {
          return "("+Fst.ToString()+","+Snd.ToString()+")";
        }

      }

      static long ExchangerTrial(bool debug) {
        Exchanger<String> exchg = new Exchanger<String>();
        bool killSwitch = false;
        var cnt = new int[threads * 2];
        var ts = new Thread[threads*2];
        DateTime start, finish;
        start = DateTime.Now;

        for (var t = 0; t < threads * 2; t++) {
          var t_ = t;
          ts[t] = new Thread(delegate() {
            var indent = new String('\t', threads * 2 + t_);
            Thread.CurrentThread.Name = t_.ToString();
            for (var i = 0; !killSwitch && i < attempts; i++) {
              var got = exchg.exchange("1234");
              cnt[t_]++;
            }
          });
          ts[t].Start();
        }

        Thread.Sleep(3000);
        killSwitch = true;
        foreach (var t in ts) {
          t.Abort();
          //t.Join();
        }

        finish = DateTime.Now;

        int tot = 0;
        foreach (var i in cnt) tot += i;

        Console.Error.WriteLine("{0:00} threads, {1:00.00} seconds, {2:00000} thousand rendezvous/s",
          threads * 2, finish.Subtract(start).TotalSeconds, (tot / finish.Subtract(start).TotalSeconds) / 1000);

        long ret = (long)((attempts / 1000) / ((tot / finish.Subtract(start).TotalSeconds) / 1000));
        return ret;
      }

      static long Trial<DICT>(bool debug) where DICT : struct, Join.IJoinFactory {
        Synchronous<Pair<String, String>>.Channel<String> Get1, Get2;
        bool killSwitch = false;

        Join j = Join.Create<DICT>();
        j.Initialize(out Get1);
        j.Initialize(out Get2);
        j.When(Get1).And(Get2).Do( (p,q) => {
          if (debug) {
            var indent = System.Threading.Thread.CurrentThread.Name;
            Console.Error.WriteLine("{0}({1})-({2})",
              indent,
              p, q);
          }
          return new Pair<String, String>(p, q);
        });

        var chs = new Synchronous<Pair<String, String>>.Channel<String>[] { Get1, Get2 };
        var cnt = new int[chan, threads];
        var ts = new Thread[chan,threads];
        
        DateTime start, finish;

        start = DateTime.Now;
        for (var c = 0; c < chan; c++) {
          for (var t = 0; t < threads; t++) {
            var t_ = t;
            var c_ = c;
            ts[c, t] = new Thread(delegate() {
              var indent = new String('\t', (c_ * threads) +t_);
              Thread.CurrentThread.Name = indent;
              for (var i = 0; !killSwitch && i < attempts; i++) {
                var got = chs[c_]("1234");
                cnt[c_, t_]++;
              }
            });
            ts[c, t].Start();
          }
        }

        Thread.Sleep(3000);
        killSwitch = true;
        foreach (var t in ts) {
          t.Abort();
          //t.Join();
        }
                
        finish = DateTime.Now;

        int tot = 0;
        foreach (var i in cnt) tot += i;

        Console.Error.WriteLine("{0:00} threads, {1:00.00} seconds, {2:00000} thousand rendezvous/s", 
          chan * threads, finish.Subtract(start).TotalSeconds, (tot / finish.Subtract(start).TotalSeconds) / 1000);
        long ret = (long) ((attempts / 1000) / ((tot / finish.Subtract(start).TotalSeconds) / 1000));
        return ret;

      }

      static void TestRendezvous<DICT>(string name, bool debug) where DICT : struct, Join.IJoinFactory {
        var series = new Dictionary<int, long>();
        Console.Error.WriteLine(name);
        Reporting.data.Add(name, series);
        for (threads = 1; threads <= Environment.ProcessorCount; threads++) {
          Reporting.AddX(chan * threads);
          GC.Collect();
          series.Add(chan*threads,Trial<DICT>(debug));
        }
      }

      static void TestExchanger(string name, bool debug) {
        var series = new Dictionary<int, long>();
        Console.Error.WriteLine(name);
        Reporting.data.Add(name, series);
        for (threads = 1; threads <= Environment.ProcessorCount; threads++) {
          Reporting.AddX(chan * threads);
          GC.Collect();
          series.Add(chan * threads, ExchangerTrial(debug));
        }
      }

      static void Main(string[] args) {
        

        TestRendezvous<Join.LockBased>("LB-Join-mod", false);

        /*
        Console.WriteLine("");
        Console.WriteLine("LFJoin");
        TestRendezvous<Join.LFJoin>(false);
        

        Console.WriteLine("");
        Console.WriteLine("SJoin");
        TestRendezvous<Join.SJoin>(false);
        */
        
        TestRendezvous<Join.ScalableNonOpt>("S-Join",false);

        TestRendezvous<Join.Scalable>("S-Join-opt",false);

        TestExchanger("Exchanger", false);

        Reporting.ReportData(false); // only report (estimated) absolute perf
      }
    }
  }

