using System;
using System.Threading;
using Microsoft.Research.Joins;
using System.Diagnostics;

// Micro-benchmark pitting Comega OnePlaceBuffer (hand-translated) against Joins version.
namespace Comega {

  internal class CallQ<A> {

    internal class CyclicList {

      internal A mHead;
      internal CyclicList mTail;
      internal CyclicList(A head, CyclicList context) {
        mHead = head;
        mTail = context.mTail;
        context.mTail = this;
      }
      internal CyclicList(A head) {
        mHead = head; mTail = this;
      }
    }

    private CyclicList mLast;

    internal void Init() { mLast = null; }
    internal void Add(A a) {
      if (mLast == null) mLast = new CyclicList(a);
      else {
        new CyclicList(a, mLast);
        mLast = mLast.mTail;
      }
    }
    internal A Get() {
      A a = mLast.mTail.mHead;
      if ((mLast.mTail) == mLast)
        mLast = null;
      else mLast.mTail = mLast.mTail.mTail;
      return a;
    }

    internal bool Empty { get { return mLast == null; } }

    public override string ToString() {
      return (mLast == null) ? "empty" :
        (mLast.mTail == mLast) ? mLast.mTail.mHead.ToString() :
        mLast.mTail.mHead + ",...," + mLast.mHead;
    }
  }

  public sealed class VoidQ {
    private int n;
    public VoidQ() { n = 0; }
    public void Add() { n++; }
    public void Get() { n--; }
    public bool Empty() { return (n == 0); }
  }

  public class ThreadQ {
    private bool Signalled = false;
    private int Count = 0;
    public bool Empty() { return (Count == 0); }
    public void Yield(object myCurrentLock) {
      Count++;
      Monitor.Exit(myCurrentLock);
      lock (this) {
        while (!Signalled) {
          Monitor.Wait(this);
        }
        Signalled = false;
      }
      Monitor.Enter(myCurrentLock);
      Count--;
    }
    public void WakeUp() {
      lock (this) {
        if (!Signalled) {
          Signalled = true;
          Monitor.Pulse(this);
        }
      }
    }
  }

  public class OnePlaceBuffer {
    // Methods
    public OnePlaceBuffer() {
      this._state = 0;
      this._lock = new object();
      this._callQ00000001 = new VoidQ();
      this._callQ00000002 = new CallQ<string>();
      this._threadQ00000004 = new ThreadQ();
      this._threadQ00000008 = new ThreadQ();
      this.empty();
    }
    private void _scan() {
      if ((~this._state & 5) == 0) {
        this._threadQ00000004.WakeUp();
      }
      else if ((~this._state & 10) == 0) {
        this._threadQ00000008.WakeUp();
      }
    }
    private void contains(string s) {
      object obj1 = this._lock;
      lock (obj1) {
        this._callQ00000002.Add(s);
        if ((((~this._state & 2) == 0) ? 1 : 0) == 0) {
          this._state |= 2;
          this._scan();
        }
      }
    }
    private void empty() {
      object obj1 = this._lock;
      lock (obj1) {
        this._callQ00000001.Add();
        if ((((~this._state & 1) == 0) ? 1 : 0) == 0) {
          this._state |= 1;
          this._scan();
        }
      }
    }
    public string Get() {
      string text2;
      Monitor.Enter(this._lock);
      if ((((~this._state & 8) == 0) ? 1 : 0) == 0) {
        goto Label_0073;
      }
    Label_003C:
      this._threadQ00000008.Yield(this._lock);
      if (this._threadQ00000008.Empty()) {
        this._state &= 0xfffffff7;
      }
    Label_0073:
      if ((~this._state & 2) == 0) {
        string obj1 = this._callQ00000002.Get();
        if (this._callQ00000002.Empty) {
          this._state &= 0xfffffffd;
        }
        this._scan();
        Monitor.Exit(this._lock);
        string text1 = obj1;
        this.empty();
        text2 = text1;
      }
      else {
        this._state |= 8;
        goto Label_003C;
      }
      string text3 = text2;
      return text2;
    }

    public void Put(string s) {
      Monitor.Enter(this._lock);
      if ((((~this._state & 4) == 0) ? 1 : 0) == 0) {
        goto Label_0073;
      }
    Label_003C:
      this._threadQ00000004.Yield(this._lock);
      if (this._threadQ00000004.Empty()) {
        this._state &= 0xfffffffb;
      }
    Label_0073:
      if ((~this._state & 1) == 0) {
        this._callQ00000001.Get();
        if (this._callQ00000001.Empty()) {
          this._state &= 0xfffffffe;
        }
        this._scan();
        Monitor.Exit(this._lock);
        this.contains(s);
      }
      else {
        this._state |= 4;
        goto Label_003C;
      }
    }

    // Fields
    private VoidQ _callQ00000001;
    private CallQ<string> _callQ00000002;
    private object _lock;
    private uint _state;
    private ThreadQ _threadQ00000004;
    private ThreadQ _threadQ00000008;
  }




}
namespace Joins {

  public class OnePlaceBuffer {

    private readonly Asynchronous.Channel Empty;
    private readonly Asynchronous.Channel<string> Contains;

    public readonly Synchronous.Channel<string> Put;
    public readonly Synchronous<string>.Channel Get;
    public OnePlaceBuffer() {
      Join j = Join.Create();


      j.Initialize(out Empty);
      j.Initialize(out Contains);
      j.Initialize(out Put);
      j.Initialize(out Get);


      /*
      Empty = j.CreateAsynchronousChannel();
      Contains = j.CreateAsynchronousChannel<string>();
      Put = j.CreateVoidSynchronousChannel<string>();
      Get = j.CreateSynchronousChannel<string>();
      */


      j.When(Put).And(Empty).Do(delegate(string s) { Contains(s); });


      j.When(Get).And(Contains).Do(delegate(string s)
        {
          Empty();
          return s;
        });

      Empty();

    }
  }


  class Program {


    static Joins.OnePlaceBuffer jb = new Joins.OnePlaceBuffer();
    static Comega.OnePlaceBuffer cb = new Comega.OnePlaceBuffer();

    public static void Main() {
      int n = 1000;

      Comega.OnePlaceBuffer[] cbs = new Comega.OnePlaceBuffer[n];
      Joins.OnePlaceBuffer[] jbs = new Joins.OnePlaceBuffer[n];

      Stopwatch sw = new Stopwatch();

    loop:
      GC.Collect();

      int gcs = GC.CollectionCount(0);
      sw.Reset();
      sw.Start();
      for (int i = 0; i < n; i++) {
        cbs[i] = new Comega.OnePlaceBuffer();
      }
      sw.Stop();

      long cTicks = sw.ElapsedTicks;
      Console.WriteLine("Comega allocation: {0}", sw.Elapsed);
      gcs = GC.CollectionCount(0) - gcs;
      Console.WriteLine("Collections: {0}", gcs);

      GC.Collect();
      gcs = GC.CollectionCount(0);
      sw.Reset();
      sw.Start();
      for (int i = 0; i < n; i++) {
        jbs[i] = new Joins.OnePlaceBuffer();
      }
      sw.Stop();

      long jTicks = sw.ElapsedTicks;
      Console.WriteLine("Joins  allocation: {0}", sw.Elapsed);
      gcs = GC.CollectionCount(0) - gcs;
      Console.WriteLine("Collections: {0}", gcs);

      Console.WriteLine(" allocation ratio: {0}", Ratio(jTicks, cTicks));



      sw.Reset();
      sw.Start();
      for (int i = 0; i < n; i++) {
        cb.Put("");
        cb.Get();
      }
      sw.Stop();
      cTicks = sw.ElapsedTicks;
      Console.WriteLine("Comega Put/Get: {0}", sw.Elapsed);

      sw.Reset();
      sw.Start();
      for (int i = 0; i < n; i++) {
        jb.Put("");
        jb.Get();
      }
      sw.Stop();
      jTicks = sw.ElapsedTicks;
      Console.WriteLine("Joins  Put/Get: {0}", sw.Elapsed);
      Console.WriteLine("Joins/Comega Ratio: {0}", Ratio(jTicks,cTicks));

      sw.Reset();
      sw.Start();
      Thread producer = new Thread(delegate()
      {
        for (int i = 0; i < n; i++) {
          cb.Put("");
          // Thread.Sleep(0);
        }
      });
      Thread consumer = new Thread(delegate()
      {
        for (int i = 0; i < n; i++) {
          string s = cb.Get();
          // Thread.Sleep(0);
        }
      });
      producer.Start();
      consumer.Start();
      consumer.Join();
      sw.Stop();
      cTicks = sw.ElapsedTicks;
      Console.WriteLine("Comega Threads: {0}", sw.Elapsed);

      sw.Reset();
      sw.Start();
      producer = new Thread(delegate()
      {
        for (int i = 0; i < n; i++) {
          jb.Put("");
          //Thread.Sleep(0);
        }
      });
      consumer = new Thread(delegate()
      {
        for (int i = 0; i < n; i++) {
          string s = jb.Get();
          // Thread.Sleep(0);
        }
      });
      producer.Start();
      consumer.Start();
      consumer.Join();
      sw.Stop();
      jTicks = sw.ElapsedTicks;
      Console.WriteLine("Joins  Threads: {0}", sw.Elapsed);
      Console.WriteLine("Joins/Comega Ratio: {0}",  Ratio(jTicks,cTicks));


      System.Console.WriteLine("Hit any key to try again.");
      Console.ReadLine();

      goto loop;


    }
    static Double Ratio(long num, long div) {
      return (Double)num / (Double)div;
    }
  }

}