//------------------------------------------------------------------------------
// <copyright file="LFTarget.cs" company="Microsoft">
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
using Microsoft.Research.Joins.Paper.Bag;

namespace Microsoft.Research.Joins.Paper {
  internal partial class Join<IntSet, MsgArray> : Join<IntSet>
    where IntSet : struct, IIntSet<IntSet>
    where MsgArray : struct, IStackArray<Msg> {

  /*
    internal interface Chan {
      Msg FindPendingMsg(out bool sawClaimed);
      Chord[] Chords { get; }
      int id { get; }
      ThreadLocal<int> SpinCount { get; }
      bool IsSync { get; }

      void RegisterChord(Chord c);
    }
  
    internal interface Chan<A> : Chan {
      Msg AddPendingMsg(A a);
      A GetPayload(Msg msg);
      A ThreadStaticPayload { set; }
          
    }

    */

    internal abstract class Chan : IChannelTarget {

      internal readonly Join<IntSet, MsgArray> mOwner;

      internal readonly int id;
      internal readonly bool IsSync;
      bool IChannelTarget.IsAsync { get { return !IsSync; } }
      int IChannelTarget.id { get { return id; } }

      void IChannelTarget.CheckOwner(Join join) {
        if (join != mOwner) JoinException.ForeignJoinException();
      }

      public Chord[] Chords;

      public void RegisterChord(Chord c) {
        var oldChords = Chords;
        Chords = new Chord[Chords.Length + 1];
        oldChords.CopyTo(Chords, 0);
        Chords[oldChords.Length] = c;
      }

      internal ThreadLocal<int> mSpinCount = new ThreadLocal<int>(() => -1);

      public ThreadLocal<int> SpinCount {
        get { return mSpinCount; }
      }

      public abstract Msg FindPendingMsg(out bool sawClaimed);
    

      internal Chan(Join<IntSet, MsgArray> join, bool isSync) {
        id = join.NextChannel(this);
        Chords = new Chord[0];
        mOwner = join;
        IsSync = isSync;
      }
    }

    internal abstract class Chan<A> :  Chan, IChannelTarget<A> {
      
      public abstract Msg AddPendingMsg(A a);
      
      public abstract A GetPayload(Msg msg);

      public A ThreadStaticPayload {
        set {
          ThreadStatic<A>.Value = value;
        }
      }

      internal Chan(Join<IntSet, MsgArray> join, bool isSync)
        : base(join,isSync) {
      }
    }
  }
}
