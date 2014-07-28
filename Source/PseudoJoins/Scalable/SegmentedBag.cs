//------------------------------------------------------------------------------
// <copyright file="SegmentedBag.cs" company="Microsoft">
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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Research.Joins.Paper;
#if true

namespace Microsoft.Research.Joins.Paper.Bag {

 
  enum Stat : int { NULLED = 0, PENDING = 1, CLAIMED = 2, CONSUMED = 3, WOKEN = 4 };

  internal abstract class Segment {
    internal const int SEGSIZEEXP = 5; 
    internal const int CACHELINEEXP = 3;

    internal const int SEGSIZE = 1 << SEGSIZEEXP;
    internal const int ALLOCSIZE = SEGSIZE;
    internal const int CACHELINE = 1 << CACHELINEEXP; // words
    internal readonly object mChan;
  
    internal abstract Segment GetNext();
    
    /// <summary>
    /// Lower bound on index of first unconsumed item (if any).
    /// </summary>
    internal int mLow;

    /// <summary>
    /// Upper bound on index of last item.
    /// </summary>
    internal int mHigh;
    internal int High {
      get { return Math.Min(mHigh, SEGSIZE - 1);  }
    }

    internal bool IsFull() {
      return mHigh >= SEGSIZE - 1;
    }

    protected int IdxOf(int i) {
      //Swap high and low parts of address to decrease cache locality...
      return ((i & (CACHELINE - 1)) << (SEGSIZEEXP - CACHELINEEXP))
        | ((i & (~(CACHELINE - 1))) >> CACHELINEEXP);
    }

    abstract internal Signal GetWakeUpSignal(int i);

    abstract internal Stat GetStatus(int i);
    abstract internal void SetStatus(int i, Stat s);

    internal abstract R GetResult<R>(int i);
    internal abstract void SetResult<R>(int i, R result);
    internal abstract void SetException(int i, Exception exception);

    internal abstract Msg GetWaker(int i);
    internal abstract void SetWaker(int i, Msg waker);
    
    /// <summary>
    /// Attempt to move status from PENDING to newStatus.
    /// </summary>
    /// <returns>The previous status; move has succeeded only when the previous status was PENDING.</returns>
    abstract internal Stat TryMoveFromPending(int i, Stat newStatus);

    protected Segment(object chan) {
      mChan = chan;
    }
  }

  interface INodeFields<A,R> {
     void GetSignalForWakeUp();
     A payload { get; set; }

     // remaining properties are for synchronous channels only:
   
     Signal wakeUp { get; }
     R result { get; set;}
     Exception exception { get; set; }
     Msg asyncWaker { get; set; }

  }

  internal class Segment<A, R, NodeFields > : Segment where NodeFields : struct, INodeFields<A,R> {

    private struct Node {
      public volatile int status;
      public NodeFields fields;
    }
    internal Segment<A,R,NodeFields> mNext;
    
    private Node[] mNodes;

    internal override Signal GetWakeUpSignal(int i) {
      return mNodes[IdxOf(i)].fields.wakeUp;
    }

    internal A GetPayload(int i) {
      return mNodes[IdxOf(i)].fields.payload;
    }

#warning "this is a GVM"
    internal override R1 GetResult<R1>(int i) {
      var This = (Segment<A,R1,SyncNodeFields<A,R1>>) (object) this;
      var node = (This.mNodes)[IdxOf(i)];
      if (node.fields.exception == null)
        return node.fields.result;
      else
        throw node.fields.exception;
    }
#warning "this is a GVM"
    internal override void SetResult<R1>(int i, R1 result) {
      var This = (Segment<A, R1,SyncNodeFields<A,R1>>)(object)this;
     (This.mNodes)[IdxOf(i)].fields.result = result;
    }

    internal override void SetException(int i, Exception exception) {
      (this.mNodes)[IdxOf(i)].fields.exception = exception;
    }

    internal override Msg GetWaker(int i) {
      return mNodes[IdxOf(i)].fields.asyncWaker;
    }

    internal override void SetWaker(int i, Msg waker) {
      mNodes[IdxOf(i)].fields.asyncWaker = waker;
    }

    internal override Stat GetStatus(int i) {
      return (Stat)mNodes[IdxOf(i)].status;
    }

    internal override void SetStatus(int i, Stat s) {
      mNodes[IdxOf(i)].status = (int)s;
    }

    internal override Stat TryMoveFromPending(int i, Stat newStat) {
      // before issuing CAS, check the status  ---- ACTUALLY HURTS PERFORMANCE
#warning "TODO: investigate - does this optimization actually hurt performance or not?"
      var s = (Stat)mNodes[IdxOf(i)].status;
      if (s != Stat.PENDING) return s;
      return (Stat)Interlocked.CompareExchange(ref mNodes[IdxOf(i)].status, (int)newStat, (int)Stat.PENDING);
    }

    internal Segment(object chan) : base(chan) {
      mNodes = new Node[ALLOCSIZE];
      mLow = 0;
      mHigh = -1;
    }

    /// <summary>
    /// Creates and links in a new tail segment.
    /// </summary>
    /// <returns>The new tail segment.</returns>
    internal Segment<A,R,NodeFields> Grow() {
      var seg = new Segment<A,R, NodeFields>(mChan);
      mNext = seg;
      return seg;
    }

    /// <summary>
    /// Attempt to add a new item to the segment; will start in PENDING status.
    /// </summary>
    /// <param name="payload">The item to add.</param>
    /// <param name="tail">The tail pointer for the Bag (updated if growing).</param>
    /// <param name="initialStatus">The initial status of the node.</param>
    /// <returns>A MessageRef, or default(MessageRef) if unsuccessful.</returns>
    internal Msg TryAdd(A payload, ref Segment<A,R, NodeFields> tail, Stat initialStatus) {
      if (mHigh >= SEGSIZE - 1) {
        return default(Msg); // another thread is growing the Bag; spin
      }

      var index = Interlocked.Increment(ref this.mHigh); // NOTE: mHigh can overrun SEGSIZE
      if (index < SEGSIZE) {
        int ixOf = IdxOf(index);
        mNodes[ixOf].fields.payload = payload;
#warning "Why do this for async channels?"
        // mNodes[ixOf].wakeUp = Signal.GetTLSignal();
        mNodes[ixOf].fields.GetSignalForWakeUp();
        // status is volatile, so ordering is guaranteed
        mNodes[ixOf].status = (int)initialStatus;  
      }

      // time to grow?
      if (index == SEGSIZE - 1) tail = Grow();
      if (index < SEGSIZE)
        return new Msg(this, index);
      else
        return default(Msg); // another thread is growing the Bag, must retry
    }

    internal override Segment GetNext() {
      var wait = new SpinWait();
      while (mNext == null) wait.SpinOnce();
      return mNext;
    }
  }

  internal struct Msg {
    internal Segment mSeg;
    internal int mIdx;

    public static readonly Msg Null;

    public object Chan() {
      return /* (mSeg == null) ? null : */ mSeg.mChan;
    }

    public bool Is(Msg m) {
      return mSeg == m.mSeg && mIdx == m.mIdx;
    }

    public override string ToString () {
      if (mSeg != null)
        return mIdx.ToString();
      return "-";
    }

    internal Msg(Segment seg) { 
      mSeg = seg;
      mIdx = seg.mLow;
      Stabilize();
    }

    internal Msg(Segment seg, int idx) {
      mSeg = seg;
      mIdx = idx;
      Stabilize();
    }

    private void Stabilize() {
      // while Grow in progress
      while (mIdx >= Segment.SEGSIZE && mSeg.mHigh >= Segment.SEGSIZE - 1) {
        mSeg = mSeg.GetNext();
        mIdx = mSeg.mLow;
      }
      if (mIdx > mSeg.High) mSeg = null;
    }

    internal bool MoveNext() {
      mIdx++;
      Stabilize();
      return !IsNull;
    }

    internal Msg Next(out bool success) {
      Msg ret = this;
      success = ret.MoveNext();
      return ret;
    }


    internal R GetResult<R>() {
      return mSeg.GetResult<R>(mIdx);
    }

    internal void SetResult<R>(R result) {
      mSeg.SetResult(mIdx, result);
    }

    internal void SetException(Exception exception) {
      mSeg.SetException(mIdx, exception);
    }

    internal void SetAsyncWaker(Msg waker) {
      mSeg.SetWaker(mIdx, waker);
      mSeg.SetStatus(mIdx, Stat.WOKEN);
    }

    internal bool HasAsyncWaker { get { return !(mSeg.GetWaker(mIdx).IsNull); } }
   

    internal Msg AsyncWaker {
      get { return mSeg.GetWaker(mIdx); }
      set { mSeg.SetWaker(mIdx, value); }

    }

    internal Signal Signal {
      get { return mSeg.GetWakeUpSignal(mIdx); }
    }

    internal Stat Status {
      get {
        Stat s = mSeg.GetStatus(mIdx);
        if (s == Stat.CONSUMED && mSeg.mLow == mIdx) mSeg.mLow = mIdx + 1; // catch up low water mark when possible.
        return s;
      }

      set {
        if (IsNull) return;
        mSeg.SetStatus(mIdx, value);
      }
    }

    internal bool IsConsumed {
      get {
        if (IsNull) return false;

        if (mSeg.GetStatus(mIdx) == Stat.CONSUMED) {
          if (mSeg.mLow == mIdx) mSeg.mLow = mIdx + 1; // catch up low water mark when possible.
          return true;
        } 
        return false;
      }
    }

    internal bool IsWoken {
      get {
        if (IsNull) return false;
        return mSeg.GetStatus(mIdx) == Stat.WOKEN;
      }
    }

    internal bool IsNull {
      get { return mSeg == null; }
    }

    /// <summary>
    /// Move the status from CLAIMED to PENDING.
    /// </summary>
    internal void Rollback() {
      if (IsNull) return;

      mSeg.SetStatus(mIdx, Stat.PENDING);
      //mSeg = null;
    }
    /// <summary>
    /// Move the status from CLAIMED to CONSUMED.
    /// </summary>
    internal void Consume() {
      if (IsNull) return;

      if (mSeg.mLow == mIdx) 
        mSeg.mLow = mIdx + 1;
      mSeg.SetStatus(mIdx, Stat.CONSUMED);
      //mSeg = null;
    }

    /// <summary>
    /// Attempt to move status from PENDING to newStatus.
    /// </summary>
    /// <returns>The previous status; move has succeeded only when the previous status was PENDING.</returns>
    internal Stat TryMoveFromPending(Stat newStatus) {
      var s = mSeg.TryMoveFromPending(mIdx, newStatus);
      if (s == Stat.CONSUMED && mSeg.mLow == mIdx) mSeg.mLow = mIdx + 1; // catch up low water mark when possible.
      return s;
    }

    internal bool TryClaim() {
      if (IsNull) return true;

      //while (!IsNull) {
        Stat s = TryMoveFromPending(Stat.CLAIMED);
        if (s == Stat.PENDING)
          return true;
//        else if (s == Status.CLAIMED)
//          retry = true;
//        MoveNext();
//      }

      return false;
    }
#warning "this method is dead"
    internal bool TryClaimAnyPending(ref bool retry) {
      if (IsNull) return true;

      while (!IsNull) {
        Stat s = TryMoveFromPending(Stat.CLAIMED);
        if (s == Stat.PENDING)
          return true;
        else if (s == Stat.CLAIMED)
          retry = true;
        MoveNext();
      }

      return false;
    }

#warning "this method is dead"
    internal void Clear() {
      mSeg = null;
      mIdx = -1;
    }
  } 

  internal abstract class Bag {
    internal abstract Msg Head();
  }

  internal struct AsyncNodeFields<A> : INodeFields<A,Unit> {
    private A _payload;
    public A payload {
      get {
        return _payload;
      }
      set {
        _payload = value; ;
      }
    }
    
    public Signal wakeUp {
      get {
        return null;
      }
      set {
        ;
      }
    }

    public Unit result {
      get {
        return  Unit.Null;
      }
      set {
       ;
      }
    }

    public Exception exception {
      get {
        return null;
      }
      set {
        ;
      }
    }

    public Msg asyncWaker {
      get {
        return Msg.Null;
      }
      set {
        ;
      }
    }

    public void GetSignalForWakeUp() { // do nothing
    }
  }

  internal struct SyncNodeFields<A,R> : INodeFields<A,R> {

    private A _payload;
    public A payload {
      get {
        return _payload;
      }
      set {
        _payload = value; 
      }
    }

    private Signal _wakeUp;
    public Signal wakeUp {
      get {
        return _wakeUp ;
      }
      set {
        _wakeUp = value; 
      }
    }
    private R _result;
    public R result {
      get {
        return _result;
      }
      set {
        _result = value ;
      }
    }

    private Exception _exception;
    public Exception exception {
      get {
        return _exception;
      }
      set {
        _exception = value;
      }
    }

    private Msg _asyncWaker;
    public Msg asyncWaker {
      get {
        return _asyncWaker;
      }
      set {
        _asyncWaker = value;
      }
    }
 

    public void GetSignalForWakeUp() {
      _wakeUp = Signal.GetTLSignal();
    }

  }


  internal class Bag<A,R,NodeFields> : Bag where NodeFields :struct, INodeFields<A,R> {
    private Segment<A,R,NodeFields> mHead;
    private Segment<A,R,NodeFields> mTail;

    internal Bag(object chan) {
      mHead = mTail = new Segment<A,R,NodeFields>(chan);
    }

    internal A GetPayload(Msg msg) {
      var seg =  (Segment<A, R, NodeFields>) msg.mSeg;
      return seg.GetPayload(msg.mIdx);
    }
    /// <summary>
    /// Add a message to the queue; will typically start in PENDING status.
    /// </summary>
    /// <param name="a">The payload</param>
    /// <param name="initialStatus">The intial status of the message (typically PENDING) </param>
    /// <returns>A reference to the enqueued message</returns>
    internal Msg Add(A a, Stat initialStatus) {
      var backoff = new Backoff();
      while (true) {
        var ret = mTail.TryAdd(a, ref mTail, initialStatus);
        if (!ret.IsNull) return ret;
        backoff.Once();
      }
    }

    protected void CatchUpHead() {
      for (var head = mHead; head.mLow >= Segment.SEGSIZE && head.mNext != null; )
        head = (mHead = head.mNext);
    }

    internal override Msg Head() {
      CatchUpHead();
      return new Msg(mHead);
    }

    internal Msg FindPendingMsg(out bool sawClaimed) {
      sawClaimed = false;
      Msg m = Head();
      while (!m.IsNull) {
        switch (m.Status) {
          case Stat.PENDING: {
            return m;
          }
          case Stat.CLAIMED:
            sawClaimed = true;
            break;
          default:
            break;
        }
        m.MoveNext();
      }
      return Msg.Null;
    }
  }
}
#endif