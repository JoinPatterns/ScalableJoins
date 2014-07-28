

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.ArgCounts;
using System;
using System.Threading;

namespace Microsoft.Research.Joins {
  public static class ChordExtensions {


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do(this ActionChord<Z, Unit> chord, Action continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Unit,Unit>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Unit a) { continuation(); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0>(this ActionChord<S<Z>, P0> chord, Action<P0> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<P0,Unit>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (P0 a) { continuation(a); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0, P1>(this ActionChord<S<S<Z>>, Pair<P0, P1>> chord, Action<P0, P1> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Pair<P0, P1>,Unit>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Pair<P0, P1> a) { continuation(a.Fst,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0, P1, P2>(this ActionChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>> chord, Action<P0, P1, P2> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Pair<Pair<P0, P1>, P2>,Unit>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Pair<Pair<P0, P1>, P2> a) { continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<R>(this FuncChord<Z, Unit,R> chord, Func<R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Unit,R>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Unit a) { return continuation(); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0, R>(this FuncChord<S<Z>, P0,R> chord, Func<P0, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<P0,R>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (P0 a) { return continuation(a); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0, P1, R>(this FuncChord<S<S<Z>>, Pair<P0, P1>,R> chord, Func<P0, P1, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Pair<P0, P1>,R>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Pair<P0, P1> a) { return continuation(a.Fst,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant,
   /// or the join pattern is empty.
   /// </exception>
   public static void Do<P0, P1, P2, R>(this FuncChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>,R> chord, Func<P0, P1, P2, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy =
        new Chord<Pair<Pair<P0, P1>, P2>,R>(chord.mJoin, chord.mPattern,
                           continuation,
                           delegate (Pair<Pair<P0, P1>, P2> a) { return continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }

  } // class ChordExtensions
} // namespace Microsoft.Research.Joins

#if false
#define CONTINUATIONATTRIBUTES
using Microsoft.Research.Joins;

using Microsoft.Research.Joins.BitMasks;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.ArgCounts;
using System;
using System.Threading;

namespace Microsoft.Research.Joins {
  

  public static class ChordExtensions {
   
   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do(this ActionChord<Z,Unit> chord, Action continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0>(this ActionChord<S<Z>, P0> chord, Action<P0> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1>(this ActionChord<S<S<Z>>, Pair<P0, P1>> chord, Action<P0, P1> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2>(this ActionChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>> chord, Action<P0, P1, P2> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3>(this ActionChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>> chord, Action<P0, P1, P2, P3> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4>(this ActionChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>> chord, Action<P0, P1, P2, P3, P4> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5>(this ActionChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>> chord, Action<P0, P1, P2, P3, P4, P5> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6>(this ActionChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>> chord, Action<P0, P1, P2, P3, P4, P5, P6> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<R>(this FuncChord<Z,Unit,R> chord, Func<R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, R>(this FuncChord<S<Z>, P0,R> chord, Func<P0, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, R>(this FuncChord<S<S<Z>>, Pair<P0, P1>,R> chord, Func<P0, P1, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, R>(this FuncChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>,R> chord, Func<P0, P1, P2, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, R>(this FuncChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>,R> chord, Func<P0, P1, P2, P3, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, R>(this FuncChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>,R> chord, Func<P0, P1, P2, P3, P4, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, R>(this FuncChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>,R> chord, Func<P0, P1, P2, P3, P4, P5, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, R>(this FuncChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      chord.mJoin.Register(chord, continuation);
   }

  } // class ChordExtensions
  public abstract partial class Join {
  internal virtual void Register(ActionChord<Z,Unit> chord, Action continuation) {
    var copy = new Chord<Unit, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Unit a) { continuation(); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0>(ActionChord<S<Z>, P0> chord, Action<P0> continuation) {
    var copy = new Chord<P0, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (P0 a) { continuation(a); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1>(ActionChord<S<S<Z>>, Pair<P0, P1>> chord, Action<P0, P1> continuation) {
    var copy = new Chord<Pair<P0, P1>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<P0, P1> a) { continuation(a.Fst,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2>(ActionChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>> chord, Action<P0, P1, P2> continuation) {
    var copy = new Chord<Pair<Pair<P0, P1>, P2>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<P0, P1>, P2> a) { continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3>(ActionChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>> chord, Action<P0, P1, P2, P3> continuation) {
    var copy = new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<P0, P1>, P2>, P3> a) { continuation(a.Fst.Fst.Fst,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4>(ActionChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>> chord, Action<P0, P1, P2, P3, P4> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { continuation(a.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4, P5>(ActionChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>> chord, Action<P0, P1, P2, P3, P4, P5> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { continuation(a.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4, P5, P6>(ActionChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>> chord, Action<P0, P1, P2, P3, P4, P5, P6> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, Unit>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<R>(FuncChord<Z,Unit,R> chord, Func<R> continuation) {
    var copy = new Chord<Unit, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Unit a) { return continuation(); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, R>(FuncChord<S<Z>, P0,R> chord, Func<P0, R> continuation) {
    var copy = new Chord<P0, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (P0 a) { return continuation(a); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, R>(FuncChord<S<S<Z>>, Pair<P0, P1>,R> chord, Func<P0, P1, R> continuation) {
    var copy = new Chord<Pair<P0, P1>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<P0, P1> a) { return continuation(a.Fst,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, R>(FuncChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>,R> chord, Func<P0, P1, P2, R> continuation) {
    var copy = new Chord<Pair<Pair<P0, P1>, P2>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<P0, P1>, P2> a) { return continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, R>(FuncChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>,R> chord, Func<P0, P1, P2, P3, R> continuation) {
    var copy = new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<P0, P1>, P2>, P3> a) { return continuation(a.Fst.Fst.Fst,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4, R>(FuncChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>,R> chord, Func<P0, P1, P2, P3, P4, R> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { return continuation(a.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4, P5, R>(FuncChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>,R> chord, Func<P0, P1, P2, P3, P4, P5, R> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  internal virtual void Register<P0, P1, P2, P3, P4, P5, P6, R>(FuncChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, R> continuation) {
    var copy = new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, R>(chord.mJoin, chord.mPattern);
      copy.mContinuation = delegate (Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
   }

  } // class Join
} // namespace Microsoft.Research.Joins

#if false
namespace Microsoft.Research.Joins.Paper {
  internal partial class Join<IntSet,MsgArray> : Join<IntSet> {

    internal override void Register(ActionChord<Z,Unit> chord, Action continuation) {
      var copy = new Chord<Unit, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Unit a) { continuation(); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0>(VoidRetChord<O, P0> chord, Action<P0> continuation) {
      var copy = new Chord<P0, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(P0 a) { continuation(a); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1>(VoidRetChord<S<O>, Pair<P0, P1>> chord, Action<P0, P1> continuation) {
      var copy = new Chord<Pair<P0, P1>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<P0, P1> a) { continuation(a.Fst, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2>(VoidRetChord<S<S<O>>, Pair<Pair<P0, P1>, P2>> chord, Action<P0, P1, P2> continuation) {
      var copy = new Chord<Pair<Pair<P0, P1>, P2>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<P0, P1>, P2> a) { continuation(a.Fst.Fst, a.Fst.Snd, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3>(VoidRetChord<S<S<S<O>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>> chord, Action<P0, P1, P2, P3> continuation) {
      var copy = new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<P0, P1>, P2>, P3> a) { continuation(a.Fst.Fst.Fst, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4>(VoidRetChord<S<S<S<S<O>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>> chord, Action<P0, P1, P2, P3, P4> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { continuation(a.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4, P5>(VoidRetChord<S<S<S<S<S<O>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>> chord, Action<P0, P1, P2, P3, P4, P5> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { continuation(a.Fst.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4, P5, P6>(VoidRetChord<S<S<S<S<S<S<O>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>> chord, Action<P0, P1, P2, P3, P4, P5, P6> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, Unit>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); return Unit.Null; };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<R>(FuncChord<Z,Unit,R> chord, Func<R> continuation) {
      var copy = new Chord<Unit, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Unit a) { return continuation(); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, R>(NonVoidChord<O, P0, R> chord, Func<P0, R> continuation) {
      var copy = new Chord<P0, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(P0 a) { return continuation(a); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, R>(NonVoidChord<S<O>, Pair<P0, P1>, R> chord, Func<P0, P1, R> continuation) {
      var copy = new Chord<Pair<P0, P1>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<P0, P1> a) { return continuation(a.Fst, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, R>(NonVoidChord<S<S<O>>, Pair<Pair<P0, P1>, P2>, R> chord, Func<P0, P1, P2, R> continuation) {
      var copy = new Chord<Pair<Pair<P0, P1>, P2>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<P0, P1>, P2> a) { return continuation(a.Fst.Fst, a.Fst.Snd, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, R>(NonVoidChord<S<S<S<O>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>, R> chord, Func<P0, P1, P2, P3, R> continuation) {
      var copy = new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<P0, P1>, P2>, P3> a) { return continuation(a.Fst.Fst.Fst, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4, R>(NonVoidChord<S<S<S<S<O>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, R> chord, Func<P0, P1, P2, P3, P4, R> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { return continuation(a.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4, P5, R>(NonVoidChord<S<S<S<S<S<O>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, R> chord, Func<P0, P1, P2, P3, P4, P5, R> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

    internal override void Register<P0, P1, P2, P3, P4, P5, P6, R>(NonVoidChord<S<S<S<S<S<S<O>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, R> chord, Func<P0, P1, P2, P3, P4, P5, P6, R> continuation) {
      var copy = new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, R>(chord.mJoin, chord.mPattern, chord.mAsyncChans, chord.mSyncChans, chord.mArgChans);
      copy.mContinuation = delegate(Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst, a.Fst.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Fst.Snd, a.Fst.Fst.Fst.Snd, a.Fst.Fst.Snd, a.Fst.Snd, a.Snd); };
     copy.SetContinuationAttribute(continuation); Register(copy);
    }

  } // class SJoin
} // namespace Microsoft.Research.Joins.Paper
#endif
#endif

