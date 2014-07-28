
using System;
using Microsoft.Research.Joins.ArgCounts;


namespace Microsoft.Research.Joins {
  public static class ChordExtensions {


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do(this ActionChord<Z, Unit> chord, Action continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Unit,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Unit a) { continuation(); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0>(this ActionChord<S<Z>, P0> chord, Action<P0> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<P0,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (P0 a) { continuation(a); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1>(this ActionChord<S<S<Z>>, Pair<P0, P1>> chord, Action<P0, P1> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<P0, P1>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<P0, P1> a) { continuation(a.Fst,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2>(this ActionChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>> chord, Action<P0, P1, P2> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<P0, P1>, P2>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<P0, P1>, P2> a) { continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3>(this ActionChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>> chord, Action<P0, P1, P2, P3> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<P0, P1>, P2>, P3> a) { continuation(a.Fst.Fst.Fst,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4>(this ActionChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>> chord, Action<P0, P1, P2, P3, P4> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { continuation(a.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5>(this ActionChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>> chord, Action<P0, P1, P2, P3, P4, P5> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { continuation(a.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6>(this ActionChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>> chord, Action<P0, P1, P2, P3, P4, P5, P6> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7>(this ActionChord<S<S<S<S<S<S<S<S<Z>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8>(this ActionChord<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this ActionChord<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this ActionChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15>> chord, Action<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15>,Unit>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15> a) { continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); return Unit.Null; }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<R>(this FuncChord<Z, Unit,R> chord, Func<R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Unit,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Unit a) { return continuation(); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, R>(this FuncChord<S<Z>, P0,R> chord, Func<P0, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<P0,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (P0 a) { return continuation(a); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, R>(this FuncChord<S<S<Z>>, Pair<P0, P1>,R> chord, Func<P0, P1, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<P0, P1>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<P0, P1> a) { return continuation(a.Fst,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, R>(this FuncChord<S<S<S<Z>>>, Pair<Pair<P0, P1>, P2>,R> chord, Func<P0, P1, P2, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<P0, P1>, P2>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<P0, P1>, P2> a) { return continuation(a.Fst.Fst,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, R>(this FuncChord<S<S<S<S<Z>>>>, Pair<Pair<Pair<P0, P1>, P2>, P3>,R> chord, Func<P0, P1, P2, P3, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<P0, P1>, P2>, P3>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<P0, P1>, P2>, P3> a) { return continuation(a.Fst.Fst.Fst,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, R>(this FuncChord<S<S<S<S<S<Z>>>>>, Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>,R> chord, Func<P0, P1, P2, P3, P4, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4> a) { return continuation(a.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, R>(this FuncChord<S<S<S<S<S<S<Z>>>>>>, Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>,R> chord, Func<P0, P1, P2, P3, P4, P5, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, R>(this FuncChord<S<S<S<S<S<S<S<Z>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, R>(this FuncChord<S<S<S<S<S<S<S<S<Z>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, R>(this FuncChord<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }


   /// <summary>
   /// Completes this join pattern with body <paramref name="continuation"/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name="chord"> the join pattern to complete.</param>
   /// <param name="continuation"> the code to execute when the join pattern is enabled.</param>
   /// <exception cref="JoinException"> thrown if <paramref name="continuation"/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, R>(this FuncChord<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<S<Z>>>>>>>>>>>>>>>>, Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15>,R> chord, Func<P0, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, R> continuation) {
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15>,R>(
        chord.mJoin, chord.mPattern, continuation,
        delegate (Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<Pair<P0, P1>, P2>, P3>, P4>, P5>, P6>, P7>, P8>, P9>, P10>, P11>, P12>, P13>, P14>, P15> a) { return continuation(a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Fst.Snd,a.Fst.Fst.Fst.Snd,a.Fst.Fst.Snd,a.Fst.Snd,a.Snd); }
      );
      chord.mJoin.Register(copy);
   }

  } // class ChordExtensions
} // namespace Microsoft.Research.Joins
