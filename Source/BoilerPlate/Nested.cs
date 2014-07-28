//------------------------------------------------------------------------------
// <copyright file="Nested.cs" company="Microsoft">
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
using System.Text;

namespace BoilerPlate {
  class Program
  {

    static string Tuple(int i)
    {
      if (i <= 0) return "Unit";
      if (i == 1) return "P0";
      else return "Pair<" + Tuple(i - 1) + ", P" + (i - 1) + ">";
    }

    static string mkmPatternType(int i)
    {
      if (i <= 0) return "Pattern<Unit>";
      if (i == 1) return "Pattern<P0>";
      return "And<" + Tuple(i - 1) + ",P" + (i - 1) + ">";
    }

    static string mkProjections(string sep, int i, string p)
    {
      if (i <= 0) return "";
      if (i == 1) return sep + p;
      else return mkProjections(sep, i - 1, p + ".Fst") + "," + p + ".Snd";
    }

    

    static string mkParams(int i)
    {
      if (i <= 0) return "";
      else return "<P"+(i-1)+">";
     
    }

    static string mkParamsNoSep(int i)
    {
      if (i <= 0) return "";
      if (i == 1) return "P0";
      else return mkParamsNoSep(i - 1) + ", " + "P" + (i - 1);
    }


    static string JoinPatternClass(int i, int max)
    { if (!(i<max)) return "";
      string mPatternType = mkmPatternType(i);
      string mPatternDotGetType = Tuple(i);
      string TuplePred = Tuple(i - 1);
      string Projections = mkProjections("", i, "p");
      string Params = mkParams(i);
      string ParamsSucc = mkParams(i + 1);
      string PredParams = (i <= 1) ? "" : "<P"+(i-1)+">";
      string PSucc = "P" + i;
      string pSucc = "p" + i;
      string Pi = "P" + (i - 1);

      object[] args =
        new object[] {
             Params,//0
             mPatternType, //1
             mPatternDotGetType,//2
             Projections,// 3
             i,// 4
             Pi, //5
             PSucc, //6
             TuplePred, //7
             i+1,//8
           };

      string PairMethod = (i < 1) ? "" : 
        (i == 1) ?
 String.Format(
@"   public OpenPattern{4}<Pair<{5}, {6}>> AndPair<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern{4}<Pair<{5}, {6}>>(mRegistrar, new And<{5}, {6}>(mPattern, new Atom<{6}>(channel)), null);
   }}
", args) :  
String.Format(
@"   public OpenPattern{4}<Pair<{5}, {6}>> AndPair<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern{4}<Pair<{5}, {6}>>(mRegistrar, new And<{7}, Pair<{5}, {6}>>(mPattern.mFst,
                                             new And<{5}, {6}>(mPattern.mSnd, new Atom<{6}>(channel))), null);
   }}
", args);

      string AndMethod = (i + 1 == max) ? "" :
@"
      /// <summary>
      /// Extends this pattern to wait for one message on <paramref name=""channel""/>.
      /// </summary>
      /// <typeparam name=""" + PSucc + @"""> the type of the channel and additional continuation argument.</typeparam>
      /// <param name=""channel""> the channel to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channel""/> is null.</exception>
      /// <returns>A new pattern taking a continuation with one additional parameter of type <c>" + PSucc + @"</c> arguments.</returns>
" + 
(
      (i < 1) ? String.Format(
@"   public OpenPattern{8}<{6}> And<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern{8}<{6}>(mRegistrar, new And<{6}>(new Atom<{6}>(channel),mPattern), null);
   }}
"
, args) :
      String.Format(     
@"   public OpenPattern{8}<{6}> And<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern8<{6}>(mRegistrar, new And<{2},{6}>(mPattern, new Atom<{6}>(channel)), null);
   }}
"
, args));

      string AndNullaryMethod =
@"
        /// <summary>
        /// Extends this pattern to wait for one message on <paramref name=""channel""/>.
        /// </summary>
        /// <param name=""channel""> the channel to wait on.</param>
        /// <exception cref=""JoinException""> thrown when <paramref name=""channel""/> is null.</exception>
        /// <returns>A new pattern taking a continuation with " + i + @" arguments.</returns>
" +
(
        (i <= 1) ?  String.Format(
@"   public OpenPattern{4}{0} And(Asynchronous.Channel channel) {{
      return new OpenPattern{4}{0}(mRegistrar, new And<{2}>(mPattern, new Atom(channel)), null);
   }}
", args)
      : String.Format(
@"   public OpenPattern{4}{0} And(Asynchronous.Channel channel) {{
      return new OpenPattern{4}{0}(mRegistrar, new And<{7},{5}>(mPattern.mFst, new And<{5}>(mPattern.mSnd,new Atom(channel))), null);
   }}
", args));

    string AndVecNullaryMethod = 
      @"
      /// <summary>
      /// Extends this pattern to wait for one message on each channel in the array <paramref name=""channels""/>.
      /// </summary>
      /// <param name=""channels""> the array of channels to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channels""/> is null or contains a null channel.</exception>
      /// <returns>A new pattern taking a continuation with "+ i + @" arguments.</returns>
" +
( 
   (i <= 1) ? String.Format(
@"  public OpenPattern{4}{0} And(Asynchronous.Channel[] channels) {{
      return new OpenPattern{4}{0}(mRegistrar, new And<{2}>(mPattern, new Vector(channels)), null);
   }}
", args)
  : String.Format(
@"  public OpenPattern{4}{0} And(Asynchronous.Channel[] channels) {{
      return new OpenPattern{4}{0}(mRegistrar, new And<{7},{5}>(mPattern.mFst, new And<{5}>(mPattern.mSnd,new Vector(channels))), null);
   }}
", args));

    string AndVecMethod = (i + 1 == max) ? "" :
      @"
      /// <summary>
      /// Extends this pattern to wait for one message on each channel in the array <paramref name=""channels""/>.
      /// </summary>
      /// <param name=""channels""> the array of channels to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channels""/> is null or contains a null channel.</exception>
      /// <returns>A new pattern taking a continuation with one additional argument <c>" + pSucc + @"</c> of type <c>" + PSucc+ @"[]</c>, with <c>" + pSucc + @"[i]</c>
      /// containing the value consumed from channel <c>channels[i]</c>.</returns>
" +
(                                                                                     
    
      (i < 1) ? String.Format(
@"
   public OpenPattern{8}<{6}[]> And<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern{8}<{6}[]>(mRegistrar, new And<{6}[]>(new Vector<{6}>(channels),mPattern), null);
   }}
"
, args) :
String.Format(
  @"
   public OpenPattern{8}<{6}[]> And<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern{8}<{6}[]>(mRegistrar, new And<{2},{6}[]>(mPattern, new Vector<{6}>(channels)), null);
   }}
"
, args));

    string PairVecMethod = 
      (i == 0) ? "" :
      (i == 1) ?
String.Format(
@"
   public OpenPattern{4}<Pair<{5}, {6}[]>> AndPair<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern{4}<Pair<{5}, {6}[]>>(mRegistrar, new And<{5}, {6}[]>(mPattern, new Vector<{6}>(channels)), null);
   }}
", args) :   
String.Format(
@"
   public OpenPattern{4}<Pair<{5}, {6}[]>> AndPair<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern{4}<Pair<{5}, {6}[]>>(mRegistrar, new And<{7}, Pair<{5}, {6}[]>>(mPattern.mFst,
                                             new And<{5}, {6}[]>(mPattern.mSnd, new Vector<{6}>(channels))), null);
   }}
", args);

      // TODO <typeparamref name=""R""></typeparamref>
      return String.Format(
@"
/// <summary>
/// The type of an incomplete join pattern.
/// <para>
/// The open  pattern may be refined by an invoking instance method <c>And(ai)</c>, where 
/// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
/// or completed by invoking <c>Do(d)</c>, where <c>d</c> is the body of the pattern. 
/// The body <c>d</c> is a delegate of type <c>continuation&lt;R{0}&gt;</c> that 
/// returns a value of type <c>R</c>,
/// and receives the values of the currently joined channels
/// as delegate parameters.
/// </para>
/// </summary>
///  <typeparam name=""R""> The return type of this pattern's synchronous channel and continuation.</typeparam>
public class OpenPattern{4}{0} : JoinPattern<R> {{

   internal {1} mPattern;

   public delegate R continuation(" + mkArgs(i) + @");

   internal continuation mContinuation;

   internal override Pattern GetPattern() {{
      return mPattern;
   }}

   internal override R Fire(Join join) {{
      {2} p = mPattern.Get();
      join.Scan();
      Monitor.Exit(join);
      return mContinuation({3});
   }}

   internal OpenPattern{4}(Registrar<R> registrar, {1} Pattern, continuation continuation)
      : base(registrar) {{
      mPattern = Pattern;
      mContinuation = continuation;
    }}
 
   {9}

   {10}

   {11}

   {12}

   {13}

   {14}
   /// <summary>
   /// Completes this join pattern with body <paramref name=""continuation""/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name=""continuation""> the code to execute when the join pattern is enabled.</param>
   /// <exception cref=""JoinException""> thrown if <paramref name=""continuation""/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>
   public void Do(continuation continuation) {{
      if (continuation == null) JoinException.NullContinuationException();
      mRegistrar(new OpenPattern{4}{0}(null, mPattern, continuation));
   }}
{15}
}}
"
,
  args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], AndNullaryMethod, AndMethod, PairMethod, AndVecNullaryMethod, AndVecMethod, PairVecMethod, JoinPatternClass(i + 1, max));
   }

    static string JoinPatternClass(int i, int max)
    {
      string mPatternType = mkmPatternType(i);
      string mPatternDotGetType = Tuple(i);
      string TuplePred = Tuple(i - 1);
      string Projections = mkProjections("", i, "p");
      string Params = (i==0) ? "" : "<"+mkParamsNoSep(i)+">";
      string docParams = (i == 0) ? "" : "&lt;" + mkParamsNoSep(i) + "&gt;";
      string ParamsSucc = mkParamsNoSep(i + 1);
      string PredParams = (i <= 1) ? "" : mkParamsNoSep(i - 1);
      string PSucc = "P" + i;
      string Pi = "P" + (i - 1);
      string pSucc = "p" + i;
      

      object[] args =
        new object[] {
             Params,//0
             mPatternType, //1
             mPatternDotGetType,//2
             Projections,// 3
             PredParams,// 4
             Pi, //5
             PSucc, //6
             TuplePred, //7
             ParamsSucc,//8
           };

      string PairMethod = (i < 1) ? "" : 
        (i == 1) ? 
String.Format(
@"   public OpenPattern<Pair<{5}, {6}>> AndPair<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern<Pair<{5}, {6}>>(mRegistrar, new And<{5}, {6}>(mPattern, new Atom<{6}>(channel)), null);
   }}
", args) 
 : String.Format(
@"   public OpenPattern<{4}, Pair<{5}, {6}>> AndPair<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern<{4}, Pair<{5}, {6}>>(mRegistrar, new And<{7}, Pair<{5}, {6}>>(mPattern.mFst,
                                             new And<{5}, {6}>(mPattern.mSnd, new Atom<{6}>(channel))), null);
   }}
", args);

      string AndMethod = (i + 1 == max) ? "" :
@"
      /// <summary>
      /// Extends this pattern to wait for one message on <paramref name=""channel""/>.
      /// </summary>
      /// <typeparam name=""" + PSucc + @"""> the type of the channel and additional continuation argument.</typeparam>
      /// <param name=""channel""> the channel to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channel""/> is null.</exception>
      /// <returns>A new pattern taking a continuation with one additional parameter of type <c>" + PSucc + @"</c> arguments.</returns>
" + 
(
      (i < 1) ? String.Format(
@"   public OpenPattern<{8}> And<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern<{8}>(mRegistrar, new And<{6}>(new Atom<{6}>(channel),mPattern), null);
   }}
"
, args) :
      String.Format(
@"   public OpenPattern<{8}> And<{6}>(Asynchronous.Channel<{6}> channel) {{
      return new OpenPattern<{8}>(mRegistrar, new And<{2},{6}>(mPattern, new Atom<{6}>(channel)), null);
   }}
"
, args));

      string AndNullaryMethod =
@"
        /// <summary>
        /// Extends this pattern to wait for one message on <paramref name=""channel""/>.
        /// </summary>
        /// <param name=""channel""> the channel to wait on.</param>
        /// <exception cref=""JoinException""> thrown when <paramref name=""channel""/> is null.</exception>
        /// <returns>A new pattern taking a continuation with " + i + @" arguments.</returns>
" +
(
     (i <= 1) ? String.Format(
@"   public OpenPattern{0} And(Asynchronous.Channel channel) {{
      return new OpenPattern{0}(mRegistrar, new And<{2}>(mPattern, new Atom(channel)), null);
   }}
", args)
      : String.Format(
@"   public OpenPattern{0} And(Asynchronous.Channel channel) {{
      return new OpenPattern{0}(mRegistrar, new And<{7},{5}>(mPattern.mFst, new And<{5}>(mPattern.mSnd,new Atom(channel))), null);
   }}
", args));

    string AndVecNullaryMethod =
@"
      /// <summary>
      /// Extends this pattern to wait for one message on each channel in the array <paramref name=""channels""/>.
      /// </summary>
      /// <param name=""channels""> the array of channels to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channels""/> is null or contains a null channel.</exception>
      /// <returns>A new pattern taking a continuation with "+ i + @" arguments.</returns>
" +
(
      (i <= 1) ? String.Format(
@"  public OpenPattern{0} And(Asynchronous.Channel[] channels) {{
      return new OpenPattern{0}(mRegistrar, new And<{2}>(mPattern, new Vector(channels)), null);
   }} 
", args)
  : String.Format(
@"  public OpenPattern{0} And(Asynchronous.Channel[] channels) {{
      return new OpenPattern{0}(mRegistrar, new And<{7},{5}>(mPattern.mFst, new And<{5}>(mPattern.mSnd,new Vector(channels))), null);
   }}
", args));

    string AndVecMethod = (i + 1 == max) ? "" :
@"
      /// <summary>
      /// Extends this pattern to wait for one message on each channel in the array <paramref name=""channels""/>.
      /// </summary>
      /// <param name=""channels""> the array of channels to wait on.</param>
      /// <exception cref=""JoinException""> thrown when <paramref name=""channels""/> is null or contains a null channel.</exception>
      /// <returns>A new pattern taking a continuation with one additional argument <c>" + pSucc + @"</c> of type <c>" + PSucc+ @"[]</c>, with <c>" + pSucc + @"[i]</c>
      /// containing the value consumed from channel <c>channels[i]</c>.</returns>
" +
(                                                                                     
      (i < 1) ? String.Format(
@"
   public OpenPattern<{8}[]> And<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern<{8}[]>(mRegistrar, new And<{6}[]>(new Vector<{6}>(channels),mPattern), null);
   }}
"
, args) :
String.Format(
  @"
   public OpenPattern<{8}[]> And<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern<{8}[]>(mRegistrar, new And<{2},{6}[]>(mPattern, new Vector<{6}>(channels)), null);
   }}
"
, args));

    string PairVecMethod = (i == 0) ? "" : 
      (i == 1) ? 
String.Format(
@"
   public OpenPattern<Pair<{5}, {6}[]>> AndPair<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern<Pair<{5}, {6}[]>>(mRegistrar,new And<{5}, {6}[]>(mPattern, new Vector<{6}>(channels)), null);
   }}
", args) :
String.Format(
@"
   public OpenPattern<{4}, Pair<{5}, {6}[]>> AndPair<{6}>(Asynchronous.Channel<{6}>[] channels) {{
      return new OpenPattern<{4}, Pair<{5}, {6}[]>>(mRegistrar, new And<{7}, Pair<{5}, {6}[]>>(mPattern.mFst,
                                             new And<{5}, {6}[]>(mPattern.mSnd, new Vector<{6}>(channels))), null);
   }}
", args);


      return String.Format(
@"
namespace JoinPatterns {{

/// <summary>
/// The type of an incomplete join pattern.
/// <para>
/// The open  pattern may be refined by an invoking instance method <c>And(ai)</c>, where 
/// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
/// or completed by invoking <c>Do(d)</c>, where <c>d</c> is the body of the pattern. 
/// The body <c>d</c> is a delegate of type <c>Continuation{15}</c> that 
/// returns <c>void</c>,
/// and receives the values of the currently joined channels
/// as delegate parameters.
/// </para>
/// </summary>
public class OpenPattern{0} : JoinPatterns.JoinPattern {{

   internal {1} mPattern;
   internal Continuation{0} mContinuation;

   internal override Pattern GetPattern() {{
      return mPattern;
   }}

   internal override void Fire(Join join) {{
      {2} p = mPattern.Get();
      join.Scan();
      Monitor.Exit(join);
      mContinuation({3});
   }}

   internal override void Spawn() {{
        // NB: a unit pattern may contain void channels, so we still need to call Get().
        {2} p = mPattern.Get();
        new Thread((delegate() {{ mContinuation({3}); }})).Start();
      }}

   internal OpenPattern(Registrar registrar, {1} Pattern, Continuation{0} continuation)
      : base(registrar) {{
      mPattern = Pattern;
      mContinuation = continuation;
    }}
 
   {9}

   {10}

   {11}

   {12}

   {13}

   {14}
   /// <summary>
   /// Completes this join pattern with body <paramref name=""continuation""/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name=""continuation""> the code to execute when the join pattern is enabled.</param>
   /// <exception cref=""JoinException""> thrown if <paramref name=""continuation""/> is null, the pattern repeats an
   /// asynchronous channel,
   /// an (a)synchronous channel is foreign to this pattern's join instance, the join pattern is redundant, 
   /// or the join pattern is empty.
   /// </exception>    
   public void Do(Continuation{0} continuation) {{
      if (continuation == null) JoinException.NullContinuationException();
      mRegistrar(new OpenPattern{0}(null, mPattern, continuation));
   }}
}}
}}
"
,
  args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], AndNullaryMethod, AndMethod, PairMethod, AndVecNullaryMethod, AndVecMethod, PairVecMethod,docParams);
    }

    static string mkArgs(int i)
    {
      if (i == 0) return "";
      if (i == 1) return "P0 p0";
      else return mkArgs(i - 1) + ", " + "P" + (i - 1) + " p" + (i - 1);
    }

    static string continuation(int i)
    {
      return
        @"  public delegate R continuation<R" + mkParams(i) + ">(" + mkArgs(i) + ");";
    }

    static string Continuation(int i)
    {
      return
        @"  public delegate void Continuation " + ((i==0) ? "" : "<" + mkParamsNoSep(i) + ">")+"(" + mkArgs(i) + ");";
    }

    static void Main(string[] args)
    {
      int max = 9;

      System.Console.WriteLine(
@"
using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Continuations;
using Microsoft.Research.Joins.Patterns;
using System.Threading;
namespace Microsoft.Research.Joins {
");

      System.Console.WriteLine(
@"
namespace Continuations {
");

      for (int i = 0; i < max; i++)
      {
        System.Console.WriteLine(continuation(i));
      }

      for (int i = 0; i < max; i++)
      {
        System.Console.WriteLine(Continuation(i));
      }

      System.Console.WriteLine(
@"
} // namespace Continuations
");


      System.Console.WriteLine(
@"
    internal delegate void Registrar<R>(JoinPattern<R> JoinPattern);

    public abstract class JoinPattern<R> {

      internal abstract R Fire(Join join);
      internal abstract Pattern GetPattern();

      internal Registrar<R> mRegistrar;

      internal JoinPattern(Registrar<R> registrar) {
        mRegistrar = registrar;
      }
"
            + JoinPatternClass(0, max) +
      @"
    }
");



      System.Console.WriteLine(
@"
} // namespace Microsoft.Research.Joins
");

      System.Console.ReadLine();
    }
  }


}
