using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Research.Joins;
// bounded producer-consumer microbenchmark
namespace BoundedProducerConsumer {

  public interface Buffer {
    void put(string s);
    string get();
  }

  public class PBB : Buffer {

    public void put(string s) {
      Put(s);
    }

    public string get() {
      return Get();
    }

    Synchronous.Channel<string> Put;
    Synchronous<string>.Channel Get;
    Asynchronous.Channel<int> free;
    Asynchronous.Channel full;
    Asynchronous.Channel<string> p;

    public PBB(int capacity) {
      Join j = Join.Create();
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.Initialize(out free);
      j.Initialize(out full);
      j.Initialize(out p);
    

      j.When(Put).And(free).Do(delegate(string s, int c)
      {
        if (c == 1) full(); else free(c - 1);
        p(s);
      });

      j.When(Get).And(p).And(full).Do(delegate(string s)
      {
        free(1);
        return s;
      });

      j.When(Get).And(p).And(free).Do(delegate(string s, int c)
      {
        free(c + 1);
        return s;
      });

      free(capacity);
    }
  }

  public class LBB : Buffer {
    private Queue<string> q = new Queue<string>();
    private int capacity;

    public void put(string s) {
      lock (this) {
        while (q.Count == capacity) { Monitor.Wait(this); };
        q.Enqueue(s);
        Monitor.PulseAll(this);
      }
    }

    public string get() {
      string s;
      lock (this) {
        while (q.Count == 0) { Monitor.Wait(this); };
        s = q.Dequeue();
        Monitor.PulseAll(this);
      }
      return s;
    }

    public LBB(int capacity) { this.capacity = capacity; }
  }

  public class pctest {
    static Buffer b;
    static int iterations = 1000;
    static int capacity = 100;
    static int n = 10;


    static void Main(string[] args) {
    
      Asynchronous.Channel producer, consumer, done;
      Synchronous.Channel wait;
      Join j = Join.Create();
      j.Initialize(out producer);
      j.Initialize(out consumer);
      j.Initialize(out done);
      j.Initialize(out wait);
      j.When(producer).Do(delegate()
      {
      
        for (int n = 0; n < iterations; n++) {
          for (int i = 0; i < capacity; i++) {
            b.put("hello");
          }
          Thread.Sleep(0);
        }
        done();
      });

      j.When(consumer).Do(delegate
      {
        for (int n = 0; n < iterations; n++) {
          for (int i = 0; i < capacity; i++) {
            b.get();
          }
          Thread.Sleep(0);
        }
        done();
      });
      j.When(wait).And(done).Do(delegate { });

      DateTime start, finish;

      Console.WriteLine("polyphonic bounded buffer");
      b = new PBB(capacity);
      start = DateTime.Now;

      for (int i = 0; i < n; i++) {
        producer();
        consumer();
      }
      for (int i = 0; i < n; i++) {
        wait();
        wait();
      };

      finish = DateTime.Now;
      Console.WriteLine(finish.Subtract(start));

      Console.WriteLine("locking bounded buffer");
      b = new LBB(capacity);
      start = DateTime.Now;
      for (int i = 0; i < n; i++) {
        producer();
        consumer();
      }
      for (int i = 0; i < n; i++) {
        wait();
        wait();
      };
      finish = DateTime.Now;
      Console.WriteLine(finish.Subtract(start));

      Console.WriteLine("Hit any key to exit!");
      Console.ReadLine();
    }
  }
}
