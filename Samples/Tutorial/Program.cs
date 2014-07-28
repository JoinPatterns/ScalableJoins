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
using System.Text;


namespace SpawningTutorial {
  using System.Threading;
  using System;
  using Microsoft.Research.Joins;

  public class SpawningTutorial {
    static readonly Asynchronous.Channel<string> m;
    static SpawningTutorial() {
      Join join = Join.Create();
      join.Initialize(out m);
      join.When(m).Do(delegate(string s)
      {
        while (true) {
          Console.WriteLine(s);
          Thread.Sleep(1000);
        };
      });
    }
    static void Main_() {
      m("thread one");
      m("thread two");
    }
  }
}

namespace Counter.Counter1 {

  public class Counter {
    long i;
    public Counter() {
      i = 0;
    }
    public void Inc() {
      lock (this) {        // acquire the lock on this object
        i = i + 1;         // update the state
      }                    // release the lock
    }
    public long Value() {
      lock (this) {
        return i;          // release the lock & return state
      }
    }
  }
}

namespace Counter.Counter2 {
  using Microsoft.Research.Joins;

  public class Counter {
    long i;
    Asynchronous.Channel mylock;
    public Synchronous.Channel Inc;
    public Synchronous<long>.Channel Value;

    public Counter() {
      Join join = Join.Create();
      join.Initialize(out mylock);
      join.Initialize(out Inc);
      join.Initialize(out Value);
      join.When(Inc).And(mylock).Do(delegate
      { // acquire the lock
        i = i + 1;   // update the state
        mylock();
      });
      join.When(Value).And(mylock).Do(delegate
      { // acquire the lock
        long r = i;  // read the state
        mylock();    // release the lock
        return r;    // return state;
      });
      i = 0;
      mylock();
    }
  }
}

namespace Counter.Counter3 {
  using Microsoft.Research.Joins;

  public class Counter {
    Asynchronous.Channel<long> mystate;
    public Synchronous.Channel Inc;
    public Synchronous<long>.Channel Value;
    public Counter() {
      Join join = Join.Create();
      join.Initialize(out mystate);
      join.Initialize(out Inc);
      join.Initialize(out Value);
      join.When(Inc).And(mystate).Do(delegate(long i)
      {  // acquire the state
        mystate(i + 1);// reissue the updated the state
      });
      join.When(Value).And(mystate).Do(delegate(long i)
      { // acquire the state
        mystate(i);    // reissue the state
        return i;    // return state
      });
      mystate(0);
    }
  }
}

namespace ReaderWriter {
  using Microsoft.Research.Joins;

  public class ReaderWriter {

    private readonly Asynchronous.Channel Idle;
    private readonly Asynchronous.Channel<int> Sharing;
    public readonly Synchronous.Channel Shared, ReleaseShared;
    public readonly Synchronous.Channel Exclusive, ReleaseExclusive;

    public ReaderWriter() {
      Join j = Join.Create();
      j.Initialize(out Idle);
      j.Initialize(out Sharing);
      j.Initialize(out Shared);
      j.Initialize(out ReleaseShared);
      j.Initialize(out Exclusive);
      j.Initialize(out ReleaseExclusive);

      j.When(Shared).And(Idle).Do(delegate { Sharing(1); });
      j.When(Shared).And(Sharing).Do(delegate(int n)
      {
        Sharing(n + 1);
      });
      j.When(ReleaseShared).And(Sharing).Do(delegate(int n)
      {
        if (n == 1) Idle(); else Sharing(n - 1);
      });
      j.When(Exclusive).And(Idle).Do(delegate { });
      j.When(ReleaseExclusive).Do(delegate { Idle(); });

      Idle();
    }
  }
}

namespace BoundedBuffer {
  using Microsoft.Research.Joins;

  public class BufferN<T> {
    private readonly Asynchronous.Channel Token;
    private readonly Asynchronous.Channel<T> Value;
    public readonly Synchronous.Channel<T> Put;
    public readonly Synchronous<T>.Channel Get;

    public BufferN(int size) {
      Join join = Join.Create();
      join.Initialize(out Token);
      join.Initialize(out Value);
      join.Initialize(out Put);
      join.Initialize(out Get);
      join.When(Put).And(Token).Do(delegate(T t)
      {
        Value(t);
      });
      join.When(Get).And(Value).Do(delegate(T t)
      {
        Token();
        return t;
      });
      for (int i = 0; i < size; i++) {
        Token();
      }
    }
  }


}

namespace Semaphore {
  using Microsoft.Research.Joins;

  public class Semaphore {
    public readonly Asynchronous.Channel Signal;
    public readonly Synchronous.Channel Wait;
    public Semaphore() {
      Join join = Join.Create();
      join.Initialize(out Signal);
      join.Initialize(out Wait);
      join.When(Wait).And(Signal).Do(delegate { });
    }
  }

  public struct Pair<A, B> {
    public readonly A Fst; public readonly B Snd;
    public Pair(A Fst, B Snd) { this.Fst = Fst; this.Snd = Snd; }
  }
  public class MyApp {
    static Asynchronous.Channel<Pair<int, Semaphore>> Task1, Task2;

    public static void Main2() {
      Semaphore s = new Semaphore();
      Task1(new Pair<int, Semaphore>(1, s));
      Task2(new Pair<int, Semaphore>(2, s));
      // do something in main thread
      s.Wait(); // wait for one to finish
      s.Wait(); // wait for another to finish
      Console.WriteLine("both tasks finished");
    }

    static MyApp() {
      Join join = Join.Create();
      join.Initialize(out Task1);
      join.Initialize(out Task2);
      join.When(Task1).Do(delegate(Pair<int, Semaphore> p)
      {
        // do something with p.Fst
        p.Snd.Signal();
      });
      join.When(Task2).Do(delegate(Pair<int, Semaphore> p)
      {
        // do something else with p.Fst
        p.Snd.Signal();
      });
    }
  }
}

namespace Asynchronous.ChannelService {
  using System.Threading;
  using Microsoft.Research.Joins;

  interface IService<R, A> {
    Asynchronous.Channel<Pair<A, Asynchronous.Channel<R>>> Service { get;} 
  }

  public class SimpleService : IService<string, int> {
    readonly Asynchronous.Channel<Pair<int, Asynchronous.Channel<string>>> Service;

    Asynchronous.Channel<Pair<int, Asynchronous.Channel<string>>> IService<string,int>.Service { get { return Service; } }

    public SimpleService() {
      Join j = Join.Create();
      j.Initialize(out Service);
      j.When(Service).Do(delegate(Pair<int, Asynchronous.Channel<string>> p)
      {
        string result = p.Fst.ToString(); // do some work with p.Fst
        p.Snd(result); // send the result on p.Snd
      });
    }
  }

  public class JoinTwo<R1, R2> {
    public readonly Asynchronous.Channel<R1> First; 
    public readonly Asynchronous.Channel<R2> Second;
    public readonly Synchronous<Pair<R1, R2>>.Channel Wait;
    public JoinTwo() {
      Join j = Join.Create();
      j.Initialize(out First);
      j.Initialize(out Second);
      j.Initialize(out Wait);
      j.When(Wait).And(First).And(Second).Do(
        delegate(R1 r1, R2 r2)
        {
          return new Pair<R1, R2>(r1, r2);
        });
    }
  }

  namespace Client {
    using Request = Pair<int, Asynchronous.Channel<string>>;
    class Client {
      public static void Main_() {
        IService<string, int> s1, s2;
        s1 = new SimpleService();
        s2 = new SimpleService();
        JoinTwo<string, string> j = new JoinTwo<string, string>();
        s1.Service(new Request(10, j.First));
        s2.Service(new Request(30, j.Second));
        for (int i = 0; i < 7; i++) {
          Thread.Sleep(i);
          Console.WriteLine("Main {0}", i);
        }
        Pair<string, string> result = j.Wait(); // wait for both results to come back
        Console.WriteLine("first result={0}, second result={1}", result.Fst, result.Snd);
        Console.ReadLine();
      }
    }
  }
}
namespace ActiveObjects {
  using System.Collections;
  using Microsoft.Research.Joins;

  public abstract class ActiveObject {
    protected bool done = false;
    private readonly Asynchronous.Channel Start;
    protected Synchronous.Channel ProcessMessage;
    protected Join join;

    public ActiveObject() {
      join = Join.Create();
      join.Initialize(out ProcessMessage);
      join.Initialize(out Start);
      join.When(Start).Do(delegate
      {
        while (!done) ProcessMessage();
      });
      Start();
    }
  }

  public interface EventSink {
    Asynchronous.Channel<string> Post { get; }
  }

  public class Distributor : ActiveObject, EventSink {
    private ArrayList subscribers = new ArrayList();
    private string myname;

    public readonly Asynchronous.Channel<EventSink> Subscribe;
    public readonly Asynchronous.Channel<string> Post;

    Asynchronous.Channel<string> EventSink.Post { get { return Post; } }

    public Distributor(string name) {
      join.Initialize(out Post);
      join.Initialize(out Subscribe);
      join.When(ProcessMessage).And(Subscribe).Do(
      delegate(EventSink sink)
      {
        subscribers.Add(sink);
      });
      join.When(ProcessMessage).And(Post).Do(
      delegate(string message)
      {
        foreach (EventSink sink in subscribers) {
          sink.Post(myname + ":" + message);
        }
      });
      myname = name;
    }
  }
}

namespace RemotingActiveObjects {
  using System.Collections;
  using System.Runtime.Remoting.Messaging;
  using Microsoft.Research.Joins;

  public abstract class ActiveObject : MarshalByRefObject {
    protected bool done = false;
    private readonly Asynchronous.Channel Start;
    protected Synchronous.Channel ProcessMessage;
    protected Join join;

    public ActiveObject() {
      join = Join.Create();
      join.Initialize(out ProcessMessage);
      join.Initialize(out Start);
      join.When(Start).Do(delegate
      {
        while (!done) ProcessMessage();
      });
      Start();
    }
  }


  public interface EventSink {
    [OneWay]
    void Post(string message);
  }

  public class Distributor : ActiveObject, EventSink {
    private ArrayList subscribers = new ArrayList();
    private string myname;

    private Asynchronous.Channel<EventSink> subscribe;
    private Asynchronous.Channel<string> post;

    [OneWay]
    public void Post(string s) {
      post(s);
    }

    [OneWay]
    public void Subscribe(EventSink s) {
      subscribe(s);
    }

    public Distributor(string name) {
      join.Initialize(out post);
      join.Initialize(out subscribe);
      join.When(ProcessMessage).And(subscribe).Do(
      delegate(EventSink sink)
      {
        subscribers.Add(sink);
      });
      join.When(ProcessMessage).And(post).Do(
      delegate(string message)
      {
        foreach (EventSink sink in subscribers) {
          sink.Post(myname + ":" + message);
        }
      });
      myname = name;
    }
  }



}

namespace JoinMany {

  using Microsoft.Research.Joins;

  public class JoinMany<R> {
    private readonly Asynchronous.Channel<R>[] Responses;
    public readonly Synchronous<R[]>.Channel Wait;
    public Asynchronous.Channel<R> Response(int i) {
      return Responses[i];
    }
    public JoinMany(int n) {
      Join j = Join.Create(n + 1);
      j.Initialize(out Responses, n);
      j.Initialize(out Wait);
      j.When(Wait).And(Responses).Do(delegate(R[] results)
      {
        return results;
      });
    }
  }


  public class JoinMany {
    private readonly Asynchronous.Channel[] Responses;
    public readonly Synchronous.Channel Wait;
    public Asynchronous.Channel Response(int i) {
      return Responses[i];
    }
    public JoinMany(int n) {
      Join j = Join.Create(n + 1);
      j.Initialize(out Responses, n);
      j.Initialize(out Wait);
      j.When(Wait).And(Responses).Do(delegate () { });
    }
  }


}








