using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Research.Joins;
// producerconsumer microbenchmark
namespace ProducerConsumer {



public interface Buffer {
  void put(string s);
  string get();
}

public class PB : Buffer {

  public void put(string s) {
     Put(s);
  }

  public string get() {
     return Get();
  }

  Asynchronous.Channel<string> Put;
  Synchronous<string>.Channel Get;

  public PB() {
    Join j = Join.Create();
    j.Initialize(out Put);
    j.Initialize(out Get);
    j.When(Get).And(Put).Do( delegate(string s) {return  s;});
  }
}

public class LB : Buffer {
  private Queue<string> q = new Queue<string>();

  public void put(string s) {
    Monitor.Enter(this);
    q.Enqueue(s);
    Monitor.Pulse(this);
    Monitor.Exit(this);
  }

  public string get() {
    Monitor.Enter(this);
    while (q.Count == 0) {
      Monitor.Wait(this);
    }
    string s = (q.Dequeue());
    Monitor.Exit(this);
    return s;
  }
}

public class pctest {
  static Buffer b;
  static int total = 60000;
  static int burstsize = 20 ;
  static int n = 30;


  static void Main(string[] args) {
    Asynchronous.Channel producer, consumer, done;
    Synchronous.Channel wait;
    Join j = Join.Create();
    j.Initialize(out producer);
    j.Initialize(out consumer);
    j.Initialize(out done);
    j.Initialize(out wait);
    j.When(producer).Do(delegate () {
    int sent = 0;

    while(sent<total) {
      for(int i=0;i<burstsize;i++) {
        b.put("hello");
      }
      sent += burstsize;
      Thread.Sleep(0);
    }
    done();
  });
  
    j.When(consumer).Do(delegate {
    for(int i=0; i<total;i++) {
      string s = b.get();
    }
    done();
  });
    j.When(wait).And(done).Do(delegate {});


   // burstsize = Int32.Parse(args[0]);

    DateTime start, finish;

    Console.WriteLine("polyphonic buffer");
    b = new PB();
    start=DateTime.Now;

    for(int i = 0; i < n; i++) { 
      producer();
      consumer(); }
    for (int i = 0; i < n; i++) { 
      wait(); 
      wait(); 
    };
  
    finish=DateTime.Now;
    Console.WriteLine(finish.Subtract(start));

    Console.WriteLine("locking buffer");
    b = new LB();
    start=DateTime.Now;
    for (int i = 0; i < n; i++) {
      producer();
      consumer();
    }
    for (int i = 0; i < n; i++) {
      wait();
      wait();
    };
    finish=DateTime.Now;
    Console.WriteLine(finish.Subtract(start));

    Console.WriteLine("Hit any key to exit!");
    Console.ReadLine();
  }
}
}
