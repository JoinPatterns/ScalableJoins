//------------------------------------------------------------------------------
// <copyright file="SJoin.cs" company="Microsoft">
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
using System.Diagnostics;
using System.Threading;

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;

using Microsoft.Research.Joins.Paper.Bag;

namespace Microsoft.Research.Joins {
  public abstract partial class Join {
    // basic flavour with fast path optimization OFF
    public struct ScalableNonOpt : IJoinFactory {
      const bool FASTPATH = false;
      public Join New<IntSet>(int size) where IntSet : struct, IIntSet<IntSet> {
        return Create<IntSet>(size);
      }

      public static Join Create<IntSet>(int size) where IntSet : struct, IIntSet<IntSet> {
        if (size <= 4)
          return new Paper.Join<IntSet, StackArray<Msg>.StackArray4>(size, FASTPATH);
        else if (size <= 8)
            return new Paper.Join<IntSet, StackArray<Msg>.StackArray8>(size, FASTPATH);
        else if (size <= 16)
            return new Paper.Join<IntSet, StackArray<Msg>.StackArray16>(size, FASTPATH);
        else if (size <= 32)
          return new Paper.Join<IntSet, StackArray<Msg>.StackArray32>(size, FASTPATH);
        else if (size % 32 == 0) {
          return Create<IntSet, StackArray<Msg>.StackArray32>(size, (size / 32) - 1);
        }
        else {
          return Create<IntSet, StackArray<Msg>.StackArray32>(size, (size / 32));
        }
      }

      private static Join Create<IntSet, StackArray>(int size, int n)
        where IntSet : struct, IIntSet<IntSet>
        where StackArray : struct, IStackArray<Msg> {
        // Debug.Assert(default(IntSet).Capacity() == default(StackArray).Length);
        if (n == 0) return new Paper.Join<IntSet, StackArray>(size, FASTPATH);
        else return Create<IntSet, StackArray<Msg>.PairArray<StackArray>>(size, n >> 1);
      }
    }
    // Another flavour with fast path optimization ON
    public struct Scalable : IJoinFactory {
      const bool FASTPATH = true;
      public Join New<IntSet>(int size) where IntSet : struct, IIntSet<IntSet> {
        return Create<IntSet>(size);
      }

      public static Join Create<IntSet>(int size) where IntSet : struct, IIntSet<IntSet> {
        if (size <= 4)
          return new Paper.Join<IntSet, StackArray<Msg>.StackArray4>(size, FASTPATH);
        else if (size <= 8)
            return new Paper.Join<IntSet, StackArray<Msg>.StackArray8>(size, FASTPATH);
        else if (size <= 16)
            return new Paper.Join<IntSet, StackArray<Msg>.StackArray16>(size, FASTPATH);
        else if (size <= 32)
          return new Paper.Join<IntSet, StackArray<Msg>.StackArray32>(size, FASTPATH);
        else if (size % 32 == 0) {
          return Create<IntSet, StackArray<Msg>.StackArray32>(size, (size / 32) - 1);
        }
        else {
          return Create<IntSet, StackArray<Msg>.StackArray32>(size, (size / 32));
        }
      }

      private static Join Create<IntSet, StackArray>(int size, int n)
        where IntSet : struct, IIntSet<IntSet>
        where StackArray : struct, IStackArray<Msg> {
        // Debug.Assert(default(IntSet).Capacity() == default(StackArray).Length);
        if (n == 0) return new Paper.Join<IntSet, StackArray>(size, FASTPATH);
        else return Create<IntSet, StackArray<Msg>.PairArray<StackArray>>(size, n >> 1);
      }


    }
  }
}

namespace Microsoft.Research.Joins.Paper {
  // Unfortunately, extensions methods need to be declared in outermost classes...
  static internal class Invokers {

    [DebuggerNonUserCode]
    internal static void Invoke<IntSet, StackArray>(this Join<IntSet, StackArray>.AsyncTarget<Unit> This)
      where IntSet : struct, IIntSet<IntSet>
      where StackArray : struct, IStackArray<Msg> {
      This.Invoke(Unit.Null);
    }

    [DebuggerNonUserCode]
    internal static void Invoke<IntSet, StackArray>(this Join<IntSet, StackArray>.SyncTarget<Unit, Unit> This)
      where IntSet : struct, IIntSet<IntSet>
      where StackArray : struct, IStackArray<Msg> {
      This.Invoke(Unit.Null);
    }

    [DebuggerNonUserCode]
    internal static R Invoke<StackArray, IntSet, R>(this Join<IntSet, StackArray>.SyncTarget<R, Unit> This)
      where IntSet : struct, IIntSet<IntSet>
      where StackArray : struct, IStackArray<Msg> {
      return This.Invoke(Unit.Null);
    }

    [DebuggerNonUserCode]
    internal static void Invoke<StackArray, IntSet, A>(this Join<IntSet, StackArray>.SyncTarget<Unit, A> This, A a)
      where IntSet : struct, IIntSet<IntSet>
      where StackArray : struct, IStackArray<Msg> {
      This.Invoke(a);
    }
  }


  internal partial class Join<IntSet, MsgArray> : Join<IntSet> 
    where IntSet : struct, IIntSet<IntSet>
    where MsgArray : struct, IStackArray<Msg> {

    /*
    internal interface IChord {
      bool TryClaim(ref MessageArray claims, MessageRef myMsg, out bool sawClaimed);
      Chan[] Chans { get; }
      bool IsAsync { get; }
      void FireAsync(ref MessageArray claims);
      R FireSync<R>(ref MessageArray claims);
    }
    */

    internal abstract class Chord {
      internal abstract bool  TryClaim(ref MsgArray claims, Chan chan, Msg myMsg, out bool sawClaimed);

      internal readonly Chan[] Chans;
      
      //internal abstract bool IsAsync { get; }
      //NB: this is not a GVM!
      public R1 FireSync<R1>(ref MsgArray claims)
      {
#warning "TODO: remove this cast!"
        Chord<R1> This = (Chord<R1>)(object)this;
        return This.FireSync(ref claims);
      }
     
      internal abstract void FireAsync(ref MsgArray claims);
      
      internal readonly bool IsAsync;
   
      internal Chord(bool isAsync, Chan[] chans)
      {
         IsAsync = isAsync;
         Chans = chans;
      }
    }


    static internal void ConsumeAll(ref MsgArray claims) {
      for (int i = 0; i < claims.Length; i++) {
        //if (!claims[i].IsNull)
          claims[i].Consume();
      }
    }

    internal class AsyncTarget<A> : Chan<A> {
      internal Bag<A,Unit,AsyncNodeFields<A>> mQ;

      public override Msg FindPendingMsg(out bool sawClaimed) {
        return mQ.FindPendingMsg(out sawClaimed);
      }
      //TODO: rename me
      public override Msg AddPendingMsg(A a) {
        return mQ.Add(a, Stat.PENDING);
      }

      
      

      [System.Runtime.Remoting.Messaging.OneWay]
      [DebuggerNonUserCode]
      public void Invoke(A a) {
        AsyncSend(this, a);
      }



      internal AsyncTarget(Join<IntSet, MsgArray> join)
        : base(join,false) {
        mQ = new Bag<A,Unit,AsyncNodeFields<A>>(this);
      }

  

      public override A GetPayload(Msg msg) {
#warning "Improve me"  
        return mQ.GetPayload(msg);
      }

    }

    public override void Initialize<A>(out Asynchronous.Channel<A> channel) {
      channel = new Asynchronous.Channel<A>(new AsyncTarget<A>(this).Invoke);
    }

    public override void Initialize(out Asynchronous.Channel channel) {
      var target = new AsyncTarget<Unit>(this);
      channel = new Asynchronous.Channel(target.Invoke);
    }

    internal partial class SyncTarget<R, A> : Chan<A> {
      internal Bag<A,R,SyncNodeFields<A,R>> mQ;

      public override Msg AddPendingMsg(A a) {
        return mQ.Add(a, Stat.PENDING);
      }

      public override Msg FindPendingMsg(out bool sawClaimed) {
        return mQ.FindPendingMsg(out sawClaimed);
      }


      internal SyncTarget(Join<IntSet, MsgArray> join)
        : base(join,true) {
        mQ = new Bag<A,R,SyncNodeFields<A,R>>(this);
      }
     
      public R Invoke(A a) {
        return SyncSend<R,A>(this, a, mOwner.fastpath); 
      }

     

      public override A GetPayload(Msg msg) {
#warning "Improve me"
        if (msg.IsNull) { 
          var a = ThreadStatic<A>.Value;
          ThreadStatic<A>.Value = default(A);
          return a; 
        };
        return mQ.GetPayload(msg);
      }

    
    }

    public override void Initialize<R, A>(out Synchronous<R>.Channel<A> sync) {
      sync = new Synchronous<R>.Channel<A>(new SyncTarget<R, A>(this).Invoke);
    }

    public override void Initialize<R>(out Synchronous<R>.Channel sync) {
      var target = new SyncTarget<R, Unit>(this);
      sync = new Synchronous<R>.Channel(target.Invoke<MsgArray, IntSet, R>);
    }

    public override void Initialize<A>(out Synchronous.Channel<A> sync) {
      var target = new SyncTarget<Unit, A>(this);
      sync = new Synchronous.Channel<A>(target.Invoke<MsgArray, IntSet, A>);
    }

    public override void Initialize(out Synchronous.Channel sync) {
      var target = new SyncTarget<Unit, Unit>(this);
      sync = new Synchronous.Channel(target.Invoke<IntSet, MsgArray>);
    }

    private readonly bool fastpath = false;
    
    internal override void Register(Joins.Chord cho) {
      
      var schord = cho.Compile<IntSet,MsgArray>(this);
      foreach (var chan in schord.Chans) {
        chan.RegisterChord(schord);
        
      }
    }

    public Join(int size, bool fastpath)
      : base(size) {
      this.fastpath = fastpath;
    }
    public Join(int size) : this(size, false) { }  
  }
}
