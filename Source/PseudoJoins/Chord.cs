//------------------------------------------------------------------------------
// <copyright file="Chord.cs" company="Microsoft">
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
using Microsoft.Research.Joins.Patterns;
using System.Threading;
using System;
using System.Collections.Generic;
using Microsoft.Research.Joins.ArgCounts;
using Microsoft.Research.Joins.BitMasks;





namespace Microsoft.Research.Joins.ArgCounts {
  public abstract class S<n> { }
  public abstract class Z { }
}

namespace Microsoft.Research.Joins {
  
#warning "TODO: inline me, I no longer add value"
  internal static class ChordFactory {
    internal static ActionChord<Z,Unit> FromChannel(Join join, Synchronous.Channel ch) {
      return new ActionChord<Z,Unit>(join, AtomFactory.FromChannel(ch));
    }

    internal static ActionChord<S<Z>, A> FromChannel<A>(Join join, Synchronous.Channel<A> ch) {
      return new ActionChord<S<Z>, A>(join, AtomFactory.FromChannel(ch));
    }

    internal static FuncChord<Z,Unit,R> FromChannel<R>(Join join, Synchronous<R>.Channel ch) {
      return new FuncChord<Z,Unit,R>(join, AtomFactory.FromChannel(ch));
    }

    internal static FuncChord<S<Z>, A, R> FromChannel<A, R>(Join join, Synchronous<R>.Channel<A> ch) {
      return new FuncChord<S<Z>, A, R>(join, AtomFactory.FromChannel(ch));
    }

    internal static ActionChord<Z,Unit> FromChannel(Join join, Asynchronous.Channel ch) {
      return new ActionChord<Z,Unit>(join, AtomFactory.FromChannel(ch));
    }

    internal static ActionChord<S<Z>, A> FromChannel<A>(Join join, Asynchronous.Channel<A> ch) {
      return new ActionChord<S<Z>, A>(join, AtomFactory.FromChannel(ch));
    }

    internal static ActionChord<Z,Unit> FromChannels(Join join, Asynchronous.Channel[] ch) {
      return new ActionChord<Z,Unit>(join, VectorFactory.FromChannels(ch));
    }

    internal static ActionChord<S<Z>, A[]> FromChannels<A>(Join join, Asynchronous.Channel<A>[] ch) {
      return new ActionChord<S<Z>, A[]>(join, VectorFactory.FromChannels(ch));
    }

    internal static ActionChord<S<Z>, A[]> FromChannels<A>(Join join, Synchronous.Channel<A>[] ch) {
      return new ActionChord<S<Z>, A[]>(join, VectorFactory.FromChannels(ch));
    }

    internal static FuncChord<Z,Unit,R> FromChannels<R>(Join join, Synchronous<R>.Channel[] ch) {
      return new FuncChord<Z,Unit,R>(join, VectorFactory.FromChannels(ch));
    }

    internal static FuncChord<S<Z>, A[], R> FromChannels<A,R>(Join join, Synchronous<R>.Channel<A>[] ch) {
      return new FuncChord<S<Z>, A[], R>(join, VectorFactory.FromChannels(ch));
    }

    internal static ActionChord<Z, Unit> FromChannels(Join join, Synchronous.Channel[] ch) {
      return new ActionChord<Z, Unit>(join, VectorFactory.FromChannels(ch));
    }

   
  }

#if MONO
  public static class MonoWorkaround {
    // weird workaround for Mono...
    public static void DoPair<P0, P1, R>(this FuncChord<S<S<Z>>, Pair<P0, P1>,R> chord, Func<P0, P1, R> continuation) {
      chord.mJoin.Register(chord, continuation);
    }
    public static void DoSing<P0, R>(this FuncChord<S<Z>, P0,R> chord, Func<P0, R> continuation) {
      chord.mJoin.Register(chord, continuation);
    }
  }
#endif
  public abstract partial class Chord {
    internal abstract Pattern GetPattern();
    internal readonly Join mJoin; // made public for mono workaround

#if CONTINUATIONATTRIBUTES
    internal ContinuationAttribute mContinuationAttribute;
#endif
    internal void SetContinuationAttribute(Delegate continuation) {
#if CONTINUATIONATTRIBUTES 
       mContinuationAttribute = ContinuationAttribute.GetContinuationAttribute(continuation);
#endif 
    }

    internal Chord(Join join) {
      mJoin = join;
    }
  }

  public  abstract partial class Chord<R> : Chord {


    internal Chord(Join join)
      : base(join) { }
  }

  public partial class Chord<A, R> : Chord<R>
    {

    internal Func<A, R> mContinuation;

    internal readonly Pattern<A> mPattern;

    internal override Pattern GetPattern() { return mPattern; }



    internal Chord(Join join, Pattern<A> pat)
      : base(join) {
      mPattern = pat;
    }

    internal Chord(Join join, Pattern<A> pat, Delegate del, Func<A,R> continuation )
      : base(join) {
      mPattern = pat;
      mContinuation = continuation;
#if CONTINUATIONATTRIBUTES 
       mContinuationAttribute = ContinuationAttribute.GetContinuationAttribute(del);
#endif  
    }
  
  }

  public class FuncChord<n, A, R> : Chord<A, R> {
    internal FuncChord(Join join, Pattern<A> pat)
      : base(join, pat) { }

    /// <summary> 
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public FuncChord<n, A, R> And(Asynchronous.Channel channel) {
      return new FuncChord<n, A, R>(mJoin, new And<A>(mPattern, AtomFactory.FromChannel(channel)));
    }

    
    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public FuncChord<n, A, R> And(Asynchronous.Channel[] channels) {
      return new FuncChord<n, A, R>(mJoin, new And<A>(mPattern, VectorFactory.FromChannels(channels)));
    }

   
    /// <summary> 
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public FuncChord<n, A, R> And(Synchronous<R>.Channel channel) {
      return new FuncChord<n, A, R>(mJoin, new And<A>(mPattern, AtomFactory.FromChannel(channel)));
    }

   
    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public FuncChord<n, A, R> And(Synchronous<R>.Channel[] channels) {
      return new FuncChord<n, A, R>(mJoin, new And<A>(mPattern, VectorFactory.FromChannels(channels)));

    }

  }

  public static class FuncChordExtensions {
     

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <typeparam name="R"> the return type of pattern's continuation.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static FuncChord<S<Z>, B, R> And<B,R>(this FuncChord<Z,Unit,R> chord, Asynchronous.Channel<B> channel) {
      return new FuncChord<S<Z>,B, R>(chord.mJoin, new And<B>(AtomFactory.FromChannel(channel),chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <typeparam name="R"> the return type of the chord and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static FuncChord<S<S<n>>, Pair<A,B>, R> And<n,A,B,R>(this FuncChord<S<n>,A,R> chord, Asynchronous.Channel<B> channel) {
      return new FuncChord<S<S<n>>, Pair<A, B>, R>(chord.mJoin, new And<A, B>(chord.mPattern, AtomFactory.FromChannel(channel)));
    }


    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <typeparam name="R"> the return type of the chord and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static FuncChord<S<Z>, B[], R> And<B,R>(this FuncChord<Z,Unit,R> chord, Asynchronous.Channel<B>[] channels) {
      return new FuncChord<S<Z>, B[], R>(chord.mJoin, new And<B[]>(VectorFactory.FromChannels(channels), chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <typeparam name="R"> the return type of the chord and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static FuncChord<S<S<n>>, Pair<A, B[]>, R> And<n,A,B,R>(this FuncChord<S<n>,A,R> chord, Asynchronous.Channel<B>[] channels) {
      return new FuncChord<S<S<n>>, Pair<A, B[]>, R>(chord.mJoin, new And<A, B[]>(chord.mPattern, VectorFactory.FromChannels(channels)));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <typeparam name="R"> the return type of the chord and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static FuncChord<S<Z>, B[], R> And<B,R>(this FuncChord<Z,Unit,R> chord, Synchronous<R>.Channel<B>[] channels) {
      return new FuncChord<S<Z>, B[], R>(chord.mJoin, new And<B[]>(VectorFactory.FromChannels(channels), chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <typeparam name="R"> the return type of the channels and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static FuncChord<S<S<n>>, Pair<A, B[]>, R> And<n,A,B,R>(this FuncChord<S<n>,A,R> chord, Synchronous<R>.Channel<B>[] channels) {
      return new FuncChord<S<S<n>>, Pair<A, B[]>, R>(chord.mJoin, new And<A, B[]>(chord.mPattern, VectorFactory.FromChannels(channels)));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <typeparam name="R"> the return type of the chord and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static FuncChord<S<Z>, B, R> And<B,R>(this FuncChord<Z,Unit,R> chord, Synchronous<R>.Channel<B> channel) {
      return new FuncChord<S<Z>,B, R>(chord.mJoin, new And<B>(AtomFactory.FromChannel(channel),chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <typeparam name="R"> the return type of the channel and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static FuncChord<S<S<n>>, Pair<A,B>, R> And<n,A,B,R>(this FuncChord<S<n>,A,R> chord, Synchronous<R>.Channel<B> channel) {
      return new FuncChord<S<S<n>>, Pair<A, B>, R>(chord.mJoin, new And<A, B>(chord.mPattern, AtomFactory.FromChannel(channel)));
    }

  }

  public class ActionChord<n, A> : Chord<A, Unit> {
    internal ActionChord(Join join, Pattern<A> pat)
      : base(join, pat) { }

    /// <summary> 
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public ActionChord<n, A> And(Asynchronous.Channel channel) {
      return new ActionChord<n, A>(mJoin, new And<A>(mPattern, AtomFactory.FromChannel(channel)));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public ActionChord<n, A> And(Asynchronous.Channel[] channels) {
      return new ActionChord<n, A>(mJoin, new And<A>(mPattern, VectorFactory.FromChannels(channels)));
    }

    /// <summary> 
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public ActionChord<n, A> And(Synchronous.Channel channel) {
      return new ActionChord<n, A>(mJoin, new And<A>(mPattern, AtomFactory.FromChannel(channel)));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new chord taking a continuation with the same shape.</returns>
    public ActionChord<n, A> And(Synchronous.Channel[] channels) {
      return new ActionChord<n, A>(mJoin, new And<A>(mPattern, VectorFactory.FromChannels(channels)));
    }
  }

  public static class ActionChordExtensions {


    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static ActionChord<S<Z>, B> And<B>(this ActionChord<Z, Unit> chord, Asynchronous.Channel<B> channel) {
      return new ActionChord<S<Z>, B>(chord.mJoin, new And<B>( AtomFactory.FromChannel(channel), chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    ///  <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static ActionChord<S<S<n>>, Pair<A, B>> And<n,A,B>(this ActionChord<S<n>, A> chord, Asynchronous.Channel<B> channel) {
      return new ActionChord<S<S<n>>, Pair<A, B>>(chord.mJoin, new And<A, B>(chord.mPattern, AtomFactory.FromChannel(channel)));
    }


    

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static ActionChord<S<Z>, B[]> And<B>(this ActionChord<Z,Unit> chord, Asynchronous.Channel<B>[] channels) {
      return new ActionChord<S<Z>,B[]>(chord.mJoin, new And<B[]>(VectorFactory.FromChannels(channels), chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static ActionChord<S<S<n>>, Pair<A, B[]>> And<n,A,B>(this ActionChord<S<n>,A> chord, Asynchronous.Channel<B>[] channels) {
      return new ActionChord<S<S<n>>, Pair<A, B[]>>(chord.mJoin, new And<A,B[]>(chord.mPattern, VectorFactory.FromChannels(channels)));
    }



    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static ActionChord<S<Z>, B> And<B>(this ActionChord<Z,Unit> chord,  Synchronous.Channel<B> channel) {
      return new ActionChord<S<Z>, B>(chord.mJoin, new And<B>(AtomFactory.FromChannel(channel), chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on <paramref name="channel"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the type of the channel and additional continuation argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channel"> the additional channel to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channel"/> is null.</exception>
    /// <returns>A new chord taking a continuation with one additional parameter of type <c>B</c>.</returns>
    public static ActionChord<S<S<n>>, Pair<A, B>> And<n,A,B>(this ActionChord<S<n>,A> chord,Synchronous.Channel<B> channel) {
      return new ActionChord<S<S<n>>, Pair<A, B>>(chord.mJoin, new And<A, B>(chord.mPattern, AtomFactory.FromChannel(channel)));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static ActionChord<S<Z>, B[]> And<B>(this ActionChord<Z,Unit> chord, Synchronous.Channel<B>[] channels) {
      return new ActionChord<S<Z>, B[]>(chord.mJoin, new And<B[]>(VectorFactory.FromChannels(channels),chord.mPattern));
    }

    /// <summary>
    /// Extends this pattern to wait for one message on each channel in the array <paramref name="channels"/>.
    /// </summary>
    /// <typeparam name="n"> an auxilliary type counting continuation arguments.</typeparam>
    /// <typeparam name="A"> the current (encoded) argument type of the chord.</typeparam>
    /// <typeparam name="B"> the argument type of each channel in <paramref name="channels"/> and the element type of the new continuation's array argument.</typeparam>
    /// <param name="chord"> the chord to extend.</param>
    /// <param name="channels"> the array of additional channels to wait on.</param>
    /// <exception cref="JoinException"> thrown when <paramref name="channels"/> is null or contains a null channel.</exception>
    /// <returns>A new pattern taking a continuation with one additional argument <c>b</c> of type <c>B[]</c>, with <c>b[i]</c>
    /// containing the value consumed from channel <c>channels[i]</c>.</returns>
    public static ActionChord<S<S<n>>, Pair<A, B[]>> And<n,A,B>(this ActionChord<S<n>,A> chord, Synchronous.Channel<B>[] channels) {
      return new ActionChord<S<S<n>>, Pair<A, B[]>>(chord.mJoin, new And<A, B[]>(chord.mPattern, VectorFactory.FromChannels(channels)));
    }

    
  }
}