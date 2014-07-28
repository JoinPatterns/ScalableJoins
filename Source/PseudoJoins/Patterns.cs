//------------------------------------------------------------------------------
// <copyright file="Patterns.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.BitMasks;
using System.Diagnostics;

namespace Microsoft.Research.Joins.Patterns {

  internal abstract partial class Pattern {
    internal abstract IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) where IntSet : IIntSet<IntSet>;
  }

  internal abstract partial class Pattern<P> : Pattern {

  }
#warning "TODO: inline me, I no longer add value"
  internal static class AtomFactory {
    internal static Atom<Unit> FromChannel(Synchronous.Channel ch) { return new Atom<Unit>(Synchronous.ToChannelTarget(ch)); }
    internal static Atom<P> FromChannel<P>(Synchronous.Channel<P> ch) { return new Atom<P>(Synchronous.ToChannelTarget(ch)); }
    internal static Atom<Unit> FromChannel<R>(Synchronous<R>.Channel ch) { return new Atom<Unit>(Synchronous<R>.ToChannelTarget(ch)); }
    internal static Atom<P> FromChannel<R, P>(Synchronous<R>.Channel<P> ch) { return new Atom<P>(Synchronous<R>.ToChannelTarget(ch)); }
    internal static Atom<Unit> FromChannel(Asynchronous.Channel ch) { return new Atom<Unit>(Asynchronous.ToChannelTarget(ch)); }
    internal static Atom<P> FromChannel<P>(Asynchronous.Channel<P> ch) { return new Atom<P>(Asynchronous.ToChannelTarget(ch)); }
  }

  /// <summary>
  /// Pattern for a single channel.
  /// </summary>
  /// <typeparam name="P">The message type for the channel (Unit for void)</typeparam>
  internal partial class Atom<P> : Pattern<P> {
    internal IChannelTarget<P> mCT;

    

    internal override IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) {
      if (mCT == null) JoinException.ForeignJoinException();
      mCT.CheckOwner(join);

      if (currentSet.Contains(mCT.id)) {
        JoinException.RepeatedChannelException();
      } else {
        currentSet.Add(mCT.id);
      }
      return currentSet;
    }

    internal Atom(IChannelTarget<P> ct) {
      if (ct == null) JoinException.UnInitializedChannelException();
      mCT = ct;
    }
  }
#warning "TODO: inline me, I no longer add value"
  internal static class VectorFactory {

    internal static Vector<P> FromChannels<P>(Asynchronous.Channel<P>[] chs) { return new Vector<P>(Asynchronous.ToChannelTarget(chs)); }
    internal static Vector FromChannels(Asynchronous.Channel[] chs) { return new Vector(Asynchronous.ToChannelTarget(chs)); }

    internal static Vector<P> FromChannels<P>(Synchronous.Channel<P>[] chs) { return new Vector<P>(Synchronous.ToChannelTarget(chs)); }
    internal static Vector FromChannels(Synchronous.Channel[] chs) { return new Vector(Synchronous.ToChannelTarget(chs)); }

    internal static Vector<P> FromChannels<R,P>(Synchronous<R>.Channel<P>[] chs) { return new Vector<P>(Synchronous<R>.ToChannelTarget(chs)); }
    internal static Vector FromChannels<R>(Synchronous<R>.Channel[] chs) { return new Vector(Synchronous<R>.ToChannelTarget(chs)); }

    

  }

  /// <summary>
  /// Pattern for a (dynamic) vector of channels.
  /// </summary>
  /// <typeparam name="P">The message type, common to all of the channels.</typeparam>
  internal partial class Vector<P> : Pattern<P[]> {
    internal IChannelTarget<P>[] mCTs;

 
    internal override IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) {
      foreach (IChannelTarget<P> ct in mCTs) {
        if (ct == null) JoinException.ForeignJoinException();
        ct.CheckOwner(join);

        if (currentSet.Contains(ct.id)) {
          JoinException.RepeatedChannelException();
        } else {
          currentSet.Add(ct.id);
        }
      }
      return currentSet;
    }

    internal Vector(IChannelTarget<P>[] cts) {
      if (cts == null) JoinException.UnInitializedChannelsException();
      mCTs = new IChannelTarget<P>[cts.Length];
      for (int i = 0; i < cts.Length; i++) {
        if (cts[i] == null) JoinException.UnInitializedChannelException();
        mCTs[i] = cts[i];
      }
    }

  }

  /// <summary>
  /// Special-case vector of units, treated as a single unit-returning pattern.
  /// </summary>
  internal partial class Vector : Pattern<Unit> {
  
    internal IChannelTarget<Unit>[] mCTs;


    internal override IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) {
      foreach (IChannelTarget<Unit> ct in mCTs) {
        if (ct == null) JoinException.ForeignJoinException();
        ct.CheckOwner(join);

        if (currentSet.Contains(ct.id)) {
          JoinException.RepeatedChannelException();
        } else {
        currentSet.Add(ct.id);
        }
      }
      return currentSet;
    }

    internal Vector(IChannelTarget<Unit>[] cts) {
      if (cts == null) JoinException.UnInitializedChannelsException();
      mCTs = new IChannelTarget<Unit>[cts.Length];
      for (int i = 0; i < cts.Length; i++) {
        if (cts[i] == null) JoinException.UnInitializedChannelException();
        mCTs[i] = cts[i];
      }

    }
  }

    internal partial class And<P, Q> : Pattern<Pair<P, Q>> {
      internal readonly Pattern<P> mFst;
      internal readonly Pattern<Q> mSnd;

      internal And(Pattern<P> Fst, Pattern<Q> Snd) {
        mFst = Fst;
        mSnd = Snd;
      }

      internal override IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) {
        return mSnd.GetIntSet(join, mFst.GetIntSet(join, currentSet));
      }

    }

    internal partial class And<P> : Pattern<P> {
      internal readonly Pattern<P> mFst;
      internal readonly Pattern<Unit> mSnd;

      
      internal override IntSet GetIntSet<IntSet>(Join join, IntSet currentSet) {
        return mSnd.GetIntSet(join, mFst.GetIntSet(join, currentSet));
      }

      internal And(Pattern<P> Fst, Pattern<Unit> Snd) {
        mFst = Fst;
        mSnd = Snd;
      }
    }
}

