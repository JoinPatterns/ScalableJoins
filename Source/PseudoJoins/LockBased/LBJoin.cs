//------------------------------------------------------------------------------
// <copyright file="LBJoin.cs" company="Microsoft">
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
using System.Threading;

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;
using System.Threading.Tasks;
namespace Microsoft.Research.Joins {


  public abstract partial class Join {
    public struct LockBased : IJoinFactory {
      public Join New<IntSet>(int size) where IntSet : struct, IIntSet<IntSet> {
        return new Joins.LockBased.LBJoin<IntSet>(size);
      }
    }
  }
}

namespace Microsoft.Research.Joins.LockBased {

  

  // Unfortunately, extensions methods need to be declared in outermost classes...
  internal static class Invokers {

    [DebuggerNonUserCode]
    internal static void Invoke<IntSet>(this LBJoin<IntSet>.AsyncTarget<Unit, UnitQueue> This) where IntSet : struct, IIntSet<IntSet> {
      This.Invoke(Unit.Null);
    }
    
    [DebuggerNonUserCode]
    internal static void Invoke<IntSet>(this LBJoin<IntSet>.SyncTarget<Unit, Unit> This) where IntSet : struct, IIntSet<IntSet> {
      This.Invoke(Unit.Null);
    }

    [DebuggerNonUserCode]
    internal static R Invoke<IntSet, R>(this LBJoin<IntSet>.SyncTarget<R, Unit> This) where IntSet : struct, IIntSet<IntSet> {
      return This.Invoke(Unit.Null);
    }

    [DebuggerNonUserCode]
    internal static void Invoke<IntSet, A>(this LBJoin<IntSet>.SyncTarget<Unit, A> This, A a) where IntSet : struct, IIntSet<IntSet> {
      This.Invoke(a);
    }

  }

  internal partial class LBJoin<IntSet> : Join<IntSet> where IntSet : struct, IIntSet<IntSet> {

    interface IRegister {
      void Register(Chord cho);
    }

    const string AsynchronousChannelDisplayString = "ID={mID}, Queue={mQ}";
    const string SynchronousChannelDisplayString = "ID={mID}, Queue={mThreadQ}";

    internal IntSet mState;

   
    internal override void Register(Joins.Chord cho) {
      var mask = cho.GetPattern().GetIntSet(this, default(IntSet));

      if (mask.IsEmpty()) JoinException.EmptyPatternException();


      // OVERLAP IS SOMETIMES USEFUL...

      // reject  this pattern if it overlaps an existing one
      //      var chords = mChords;
      //      if (chords != null) {
      //        do {
      //          chords = chords.mTail;
      //          if (chords.mKey.ContainsAll(mask) || mask.ContainsAll(chords.mKey)) {
      //            JoinException.RedundantPatternException();
      //          }
      //        } while (chords != mChords);
      //      }

      var chord = cho.Compile<IntSet>(this);
      
      new Dictionary<Chord>(ref mChords, mask, chord);

      for (int i = 0; i < Count; i++) {
        var chan = Chans[i];
        if (mask.Contains(chan.id) && !chan.IsAsync) {
          ((IRegister)chan).Register(chord);
        }
      }

    }
    internal Dictionary<Chord> mChords = null;
    internal void Scan() {
      IntSet state = mState;
      Dictionary<Chord> chords = mChords;
      if (chords == null) return;
      do {
        chords = chords.mTail;
        if (state.ContainsAll(chords.mKey)) {
          if (chords.mValue.IsAsync) {
            chords.mValue.FireAsync();
          } else {
            ((IWakeable)chords.mValue.syncChan).Wake();
          }
          return;
        }
      } while (chords != mChords);
    }

    

    [DebuggerDisplay(AsynchronousChannelDisplayString)]
    internal class AsyncTarget<A, Q> : Chan<A> where Q : struct, IQueue<A> {
      internal Q mQ;

      [System.Runtime.Remoting.Messaging.OneWay]
      [DebuggerNonUserCode]
      public void Invoke(A a) {
        lock (mOwner) {
          mQ.Add(a);
          if (!mOwner.mState.ContainsAll(mSetID)) {
            mOwner.mState.AddAll(mSetID);
            mOwner.Scan();
          }
        }
      }

      internal AsyncTarget(LBJoin<IntSet> join, Q q) : base(join) {
        mQ = q;
        mQ.Init();
      }
      

      internal override A Get(ref Waiter waiters) {
        A a = mQ.Get();
        if (mQ.Empty) mOwner.mState.RemoveAll(mSetID);
        return a;
      }

      public override bool IsAsync {
        get { return true; }
      }
    }

    public sealed override void Initialize<A>(out Asynchronous.Channel<A> channel) {
      channel = new Asynchronous.Channel<A>(new AsyncTarget<A, LibQueue<A>>(this, new LibQueue<A>()).Invoke);
    }

    public sealed override void Initialize(out Asynchronous.Channel channel) {
      var target = new AsyncTarget<Unit, UnitQueue>(this, new UnitQueue());
      channel = new Asynchronous.Channel(target.Invoke);
    }

    private interface IWakeable {
      void Wake();
    }



    public class Send<R,T> : AbstractSend<R> //huh?
    {
        readonly T t;
        private R result;
        readonly TaskCompletionSource<R> tcs;
        readonly SyncTarget<R,T> chan;
     
        public override bool IsCompleted
        {
            get
            {
                Monitor.Enter(chan.mOwner);
                chan.mValueInQueue = false;
                if (chan.mOwner.mState.ContainsAll(chan.mSetID))
                {
                    return false;
                }
                else
                {
                    Dictionary<Chord<R>> chords = chan.Chords;
                    if (chords != null)
                    {
                        do
                        {
                            chords = chords.mTail;
                            if (chan.mOwner.mState.ContainsAll(chords.mKey))
                            {
                                chan.pValue = t;
                                result = chords.mValue.FireSync();
                                return true;
                            }
                        } while (chords != chan.Chords);
                    }
                    chan.mOwner.mState.AddAll(chan.mSetID);
                    return false;
                };
            }
        }
        /*
         *   public override void OnCompleted(Action Resume)
        {
            tcs.Task.ContinueWith(_ => { Resume(); });

            chan.BeginInvoke(this.t, ar =>
            {
                try
                {
                    while (true)
                    {
                        var r = default(R);
                        chan.mValueInQueue = true;
                        if (chan.mWaitQ.Yield(t, chan.mOwner, ref r))
                        {
                            result = r;
                            tcs.SetResult(r);
                            return;
                        }
                        Dictionary<Chord<R>> chords = chan.Chords;
                        if (chords != null)
                        {
                            do
                            {
                                chords = chords.mTail;
                                if (chan.mOwner.mState.ContainsAll(chords.mKey))
                                {
                                    chan.pValue = t;
                                    result = chords.mValue.FireSync();
                                    tcs.SetResult(r);
                                    return;
                                }
                            } while (chords != chan.Chords);
                        }
                        chan.mOwner.mState.AddAll(chan.mSetID);
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                };
            },
            null);
        }
         * */
        public override void OnCompleted(Action Resume)
        {

            var r = default(R);
            chan.mValueInQueue = true;
            Action<bool> k = null;
            k = done =>
            {
                if (done)
                {
                    result = r;
                    Resume();
                }
                else
                {
                    Monitor.Enter(chan.mOwner);
                    Dictionary<Chord<R>> chords = chan.Chords;
                    if (chords != null)
                    {
                        do
                        {
                            chords = chords.mTail;
                            if (chan.mOwner.mState.ContainsAll(chords.mKey))
                            {
                                chan.pValue = t;
                                result = chords.mValue.FireSync();
                                ;
                                Resume();
                                return;
                            }
                        } while (chords != chan.Chords);
                    }
                    chan.mOwner.mState.AddAll(chan.mSetID);
                    chan.mWaitQ.AsyncYield(t,chan.mOwner, k);
                };
               
            };
            chan.mWaitQ.AsyncYield(t, chan.mOwner, k);
            //Monitor.Exit(chan.mOwner);
          
        }
        public Send(SyncTarget<R,T> chan, T t)
        {
            this.t = t;
            this.tcs = new TaskCompletionSource<R>();
            this.chan = chan;

        }
        public override R GetResult() { return this.result; }

        public override AbstractSend<R> GetAwaiter()
        {
            return this;
        }
    }



       


    [DebuggerDisplay(SynchronousChannelDisplayString)]
    internal class SyncTarget<R, A> : Chan<A>, IWakeable, IRegister, IGetAwaiter<R,A> {

      
      private A mValue;
      public bool mValueInQueue = true;
      protected internal A pValue {
        get {
          A value = mValue;
          mValue = default(A); // clear the register after read, to avoid leaks.
          mValueInQueue = true;
          return value;
        }
        set {
          mValueInQueue = false; 
          mValue = value; }
      }
      internal ThreadQueue<R,A> mWaitQ;
      internal Dictionary<Chord<R>> Chords;

      void IRegister.Register(Chord chord) {
        new Dictionary<Chord<R>>(ref Chords, chord.mask.Difference(mSetID), (Chord<R>) chord);
      }

      internal SyncTarget(LBJoin<IntSet> join) : base(join) {
        mWaitQ = new ThreadQueue<R,A>();
        commit = delegate {
        this.mOwner.Scan();
        Monitor.Exit(this.mOwner);
      };
      }

      private readonly Action commit;

      [DebuggerNonUserCode]
      public R Invoke(A a) {
        Monitor.Enter(mOwner);
        mValueInQueue = false;
        if (!mOwner.mState.ContainsAll(mSetID)) 
         goto now;
      later: {
          var r = default(R);
          mValueInQueue = true;
          if (mWaitQ.Yield(a, mOwner, ref r)) {
            return r;
          }
        }
      now:
        Dictionary<Chord<R>> chords = Chords;
        if (chords != null) {
          do {
            chords = chords.mTail;
            if (mOwner.mState.ContainsAll(chords.mKey)) {
              this.pValue = a;
              return chords.mValue.FireSync();
            }
          } while (chords != Chords);
        }
        mOwner.mState.AddAll(mSetID); goto later;
      }
     
     

      internal override A Get(ref Waiter waiters) {
        A a = default(A);
        if (mValueInQueue) {
           var waiter = mWaitQ.mQ.Dequeue();
           // TODO: Unfortunate cast.
           waiter.AddTo(ref waiters);
           a = waiter.m_arg;
        }
        else {
          a = mValue;
          // fastpath, waiters unchanged;
        }
        if (mWaitQ.Empty) mOwner.mState.RemoveAll(mSetID);
        return a;
      }

      public override string ToString() {
        string s = "#Waiting = " + mWaitQ.ToString();
        if (mWaitQ.Empty) return s;
        s += ", Awaiting = {";
        Dictionary<Chord<R>> chords = Chords;
        if (chords != null) {
          do {
            chords = chords.mTail;
            s += mOwner.IntSetToString(chords.mKey.Difference(mOwner.mState));
            if (chords != Chords) s += "|";
          } while (chords != Chords);
        }
        return s + "}";
      }

      public void Wake() {
        mWaitQ.WakeUp();
#warning "DRAGONS"
        if (mWaitQ.Empty) mOwner.mState.RemoveAll(mSetID); 
      }

      public override bool IsAsync {
        get { return false; }
      }

      public AbstractSend<R> GetAwaiter(A t)
      {
          return new Send<R,A>(this,t);
      }
    }

    public sealed override void Initialize<R, A>(out Synchronous<R>.Channel<A> sync) {
      sync = new Synchronous<R>.Channel<A>(new SyncTarget<R, A>(this).Invoke);
    }

    public sealed override void Initialize<R>(out Synchronous<R>.Channel sync) {
      var target = new SyncTarget<R, Unit>(this);
      sync = new Synchronous<R>.Channel(target.Invoke<IntSet, R>);
    }

    public sealed override void Initialize<A>(out Synchronous.Channel<A> sync) {
      var target = new SyncTarget<Unit, A>(this);
      sync = new Synchronous.Channel<A>(target.Invoke<IntSet, A>);
    }

    public sealed override void Initialize(out Synchronous.Channel sync) {
      var target = new SyncTarget<Unit, Unit>(this);
      sync = new Synchronous.Channel(target.Invoke<IntSet>);
    }

    public LBJoin(int size)
      : base(size) {
      mState = mState.Empty();
      Debug.Assert(mSize <= mState.Capacity());
    }
  }
}