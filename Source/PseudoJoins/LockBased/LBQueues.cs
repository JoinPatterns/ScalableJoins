//------------------------------------------------------------------------------
// <copyright file="LBQueues.cs" company="Microsoft">
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
using System.Diagnostics;

namespace Microsoft.Research.Joins.LockBased {

  internal interface IQueue<A> {
    void Init();
    void Add(A a);
    A Get();
    bool Empty { get; }
  }

  internal struct LibQueue<A> : IQueue<A> {
    internal System.Collections.Generic.Queue<A> mQ;

    public void Init() {
      mQ = new System.Collections.Generic.Queue<A>();
    }

    public void Add(A a) {
      mQ.Enqueue(a);
    }

    public A Get() {
      return mQ.Dequeue();
    }

    public bool Empty {
      get { return mQ.Count == 0; }
    }
  }

  internal struct Queue<A> : IQueue<A> {
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

    void IQueue<A>.Init() { mLast = null; }
    void IQueue<A>.Add(A a) {
      if (mLast == null) mLast = new CyclicList(a);
      else {
        new CyclicList(a, mLast);
        mLast = mLast.mTail;
      }
    }
    A IQueue<A>.Get() {
      A a = mLast.mTail.mHead;
      if ((mLast.mTail) == mLast)
        mLast = null;
      else mLast.mTail = mLast.mTail.mTail;
      return a;
    }

    bool IQueue<A>.Empty { get { return mLast == null; } }

    public override string ToString() {
      return (mLast == null) ? "empty" :
        (mLast.mTail == mLast) ? mLast.mTail.mHead.ToString() :
        mLast.mTail.mHead + ",...," + mLast.mHead;
    }
  }



  internal struct UnitQueue : IQueue<Unit> {
    private int mCount;
    void IQueue<Unit>.Init() { this.mCount = 0; }
    void IQueue<Unit>.Add(Unit u) {
      if (this.mCount == System.Int32.MaxValue)
        JoinException.AsynchronousChannelOverflow();
      else mCount++;
    }
    Unit IQueue<Unit>.Get() { mCount--; return Unit.Null; }
    bool IQueue<Unit>.Empty { get { return mCount == 0; } }
    public override string ToString() {
      return mCount.ToString();
    }

  }

  internal class ThreadQueue {
    private bool mSignalled = false;
    private int mCount = 0;
    internal bool Empty { get { return (mCount == 0); } }
    [DebuggerNonUserCode]
    internal void Yield(object myCurrentLock) {
      mCount++;
      Monitor.Exit(myCurrentLock);
      lock (this) {
        while (!mSignalled) {
          Monitor.Wait(this);
        }
        mSignalled = false;
      }
      Monitor.Enter(myCurrentLock);
      mCount--;
    }
    internal void WakeUp() {
      lock (this) {
        if (!mSignalled) {
          mSignalled = true;
          Monitor.Pulse(this);
        }
      }
    }
    public override string ToString() {
      return mCount.ToString();
    }
  }

  /*
  This version tries to cache a single lock in a thread-local, but may not buy us anything...
  internal class ThreadLocals {
    [ThreadStatic]
    static internal object Lock;

  }
  internal class Waiter<R> {
    enum Status { Pending, Done, Failed, Continue }
    private Status mStatus = Status.Pending;

    R m_res;
    Exception m_exn;
    object m_lock;

    internal Waiter() {
     var l = ThreadLocals.Lock;
     if (l == null) {
       l = new object();
       ThreadLocals.Lock = l;
     } 
     m_lock = l;
    }

    internal bool Wait(ref R res) {
      lock (m_lock) {
        while (mStatus == Status.Pending) {
          Monitor.Wait(m_lock);
        };
        switch (mStatus) {
          case Status.Done: {
              res = m_res;
              return true;
            }
          case Status.Failed: {
              throw m_exn;
            }
          case Status.Continue:
          default: {
              return false;
            }
        }
      }
    }
    internal void WakeUp() {
      lock (m_lock) {
        if (this.mStatus == Status.Pending) {
          this.mStatus = Status.Continue;
          Monitor.Pulse(m_lock);
        }
      }
    }
    internal void Fail(Exception e) {
      lock (m_lock) {
        if (this.mStatus == Status.Pending) {
          this.m_exn = e;
          this.mStatus = Status.Failed;
          Monitor.Pulse(m_lock);
        }
      }
      if (mNext != null) mNext.Fail(e);
    }
    internal void Succeed(R res) {
      lock (m_lock) {
        if (this.mStatus == Status.Pending) {
          this.m_res = res;
          this.mStatus = Status.Done;
          Monitor.Pulse(m_lock);
        }
      }
      if (mNext != null) mNext.Succeed(res);
    }

    internal Waiter<R> mNext;

    public void AddTo(ref Waiter<R> waiters) {
      this.mNext = waiters;
      waiters = this;
    }

  }
  */

  internal abstract class Waiter {
    internal enum Status { Pending, Done, Failed, Continue }
    internal Status mStatus = Status.Pending;
    internal Exception m_exn;

    protected abstract void Schedule();
    protected abstract void WaitWhilePending();
    internal void WakeUp() {
      lock (this) {
        if (this.mStatus == Status.Pending) {
          this.mStatus = Status.Continue;
          Schedule();
        }
      }
    }
    internal void Fail(Exception e) {
      lock (this) {
        if (this.mStatus == Status.Pending) {
          this.m_exn = e;
          this.mStatus = Status.Failed;
          Schedule();
        }
      }
      if (mNext != null) mNext.Fail(e);
    }

    internal Waiter mNext;

    public void AddTo(ref Waiter waiters) {
      this.mNext = waiters;
      waiters = this;
    }
  }
  internal abstract class Waiter<R> : Waiter  {
      internal R m_res;
      
      internal bool Wait(ref R res) {
        lock (this) {
         /* 
          while (mStatus == Status.Pending) {
            Monitor.Wait(this);
          };
          */
          WaitWhilePending();
          switch (mStatus) {
            case Status.Pending:
                  {
                      Debug.Assert(false);
                      throw new System.Exception();
                  }
            case Status.Done: {
                res = m_res;
                return true;
              }
            case Status.Failed: {
                throw m_exn;
              }
            case Status.Continue: 
            default: {
                return false;
              }
          }
        }
      }
    
      internal void Succeed(R res) {
        lock (this) {
            if (this.mStatus == Status.Pending)
            {
                this.m_res = res;
                this.mStatus = Status.Done;
                Schedule();
            }
        }
        if (mNext != null) ((Waiter<R>) mNext).Succeed(res);
      }

      

    }
   
  internal abstract class AbstractWaiter<A,R> : Waiter<R> {

     public readonly A m_arg;

     internal AbstractWaiter(A arg) {
       this.m_arg = arg;
     }

     
   }
    internal  class Waiter<A,R> : AbstractWaiter<A,R> {

    
     protected override void WaitWhilePending()
     {
         while (mStatus == Status.Pending)
         {
             Monitor.Wait(this);
         };
     }
     protected override void Schedule()
     {
         Monitor.Pulse(this);
     }

     internal Waiter(A arg): base(arg) {
     }

     
   }
    internal  class AsyncWaiter<A,R> : AbstractWaiter<A,R> {
        Action<bool, Waiter<R>> k;
    
     protected override void WaitWhilePending()
        {
#warning: "Async:check me"
            Debug.Assert(this.mStatus != Status.Pending);
            return;
     }
     protected override void Schedule()
     {
#warning: "Async:optimize me"
         Debug.Assert(this.mStatus != Status.Pending);
         System.Threading.Tasks.Task.Factory.StartNew(() => k(mStatus != Status.Continue,this));
         //Run()
     }

     internal AsyncWaiter(A arg,Action<bool,Waiter<R>> k): base(arg) {
         this.k = k;
        
     }

     
   }

  internal class ThreadQueue<R,A> {

    // todo: add Node<R> superclass with appropriate methods and 
    // links to other nodes...


    internal System.Collections.Generic.Queue<AbstractWaiter<A, R>> mQ
      = new System.Collections.Generic.Queue<AbstractWaiter<A, R>>();
   
    internal bool Empty { get { return (mQ.Count == 0); } }
    [DebuggerNonUserCode]
    internal bool Yield(A a, object myCurrentLock, ref R r) {
      var n = new Waiter<A,R>(a);
      mQ.Enqueue(n);
      Monitor.Exit(myCurrentLock);
      var s = n.Wait(ref r);
      if (s) return true;
      else {
        Monitor.Enter(myCurrentLock);
        return false;
      }
     
    }
    internal void WakeUp() {
      var n = mQ.Dequeue();
      n.WakeUp();
    }


    internal void AsyncYield( A t, object myCurrentLock, Action<bool,Waiter<R>> k)
    {
        var n = new AsyncWaiter<A, R>(t,k);
        mQ.Enqueue(n);
        Monitor.Exit(myCurrentLock);
        /*
        var s = n.Wait(ref a.result);
        if (s) return true;
        else
        {
            Monitor.Enter(myCurrentLock);
            return false;
        }
         * */
    }
  }

}