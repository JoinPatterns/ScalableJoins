//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
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
  class Program {

    static string mkArgCount(int i) {
      if (i <= 0) return "Z";
      else return "S<" + mkArgCount(i - 1) + ">";
    }

    static string mkTuple(int i) {
      if (i <= 0) return "Unit";
      if (i == 1) return "P0";
      else return "Pair<" + mkTuple(i - 1) + ", P" + (i - 1) + ">";
    }

    static string mkProjections(string sep, int i, string p) {
      if (i <= 0) return "";
      if (i == 1) return sep + p;
      else return mkProjections(sep, i - 1, p + ".Fst") + "," + p + ".Snd";
    }

    static string mkInnerParams(int i) {
      if (i <= 0) return "";
      if (i == 1) return "P0";
      else return mkInnerParams(i - 1) + ", " + "P" + (i - 1);
    }

    static string mkParams(int i) {
      if (i == 0) return "";
      else return "<" + mkInnerParams(i) + ">";
    }
    
    static string mkNVParams(int i) {
      if (i == 0) return "<R>";
      else return "<" + mkInnerParams(i) + ", R>";
    }

    static string mkParamsNoSep(int i) {
      if (i <= 0) return "";
      if (i == 1) return "P0";
      else return mkParamsNoSep(i - 1) + ", " + "P" + (i - 1);
    }

    static string mkArgs(int i) {
      if (i == 0) return "";
      if (i == 1) return "P0 p0";
      else return mkArgs(i - 1) + ", " + "P" + (i - 1) + " p" + (i - 1);
    }

    static string mkParamDoc(int i) {
      if (i == 0) return "\r\n";
      if (i == 1) return "  ///<param name=\"p0\"> argument 0 of type <c>P0</c>.</param>\r\n";
      else return mkParamDoc(i - 1) + "   ///<param name=\"p" + (i - 1) + "\"> argument " + (i - 1) + " of type <c>P" + (i - 1) + "</c>.</param>\r\n";
    }

    static string mkTypeParamDoc(int i) {
      if (i == 0) return "\r\n";
      if (i == 1) return "  ///<typeparam name=\"P0\"> the type of continuation argument <c>p0</c>.</typeparam>\r\n";
      else return mkTypeParamDoc(i - 1) + "   ///<typeparam name=\"P" + (i - 1) + "\"> the type of continuation argument <c>p" + (i - 1) + "</c>.</typeparam>\r\n";
    }

    static string mkDo(int i, bool isVoid) {
      string typeArgs;
      string contType;
      string chordType;
      string retType;
      string invocation; 

      if (isVoid) {
        typeArgs = mkParams(i);
        contType = "Action";
        chordType = "ActionChord<" + mkArgCount(i) + ", " + mkTuple(i) + ">";
        retType = "Unit";
        invocation = "continuation(" + mkProjections("", i, "a") + "); return Unit.Null;";
       
      } else {
        typeArgs = mkNVParams(i);
        contType = "Func";
        chordType = "FuncChord<" + mkArgCount(i) + ", " + mkTuple(i) + ",R>";
        retType = "R";
        invocation = "return continuation(" + mkProjections("", i, "a") + ");";
      }

      return String.Format(
@"
   /// <summary>
   /// Completes this join pattern with body <paramref name=""continuation""/> and registers it with the current <c>Join</c> instance.
   /// </summary>
   /// <param name=""chord""> the join pattern to complete.</param>
   /// <param name=""continuation""> the code to execute when the join pattern is enabled.</param>
   /// <exception cref=""JoinException""> thrown if <paramref name=""continuation""/> is null, the pattern repeats a
   /// channel, a channel is foreign to this pattern's join instance, or the join pattern is empty.
   /// </exception>    
   public static void Do{0}(this {2} chord, {1}{0} continuation) {{
      if (continuation == null) JoinException.NullContinuationException();
      var copy = 
        new Chord<{4},{3}>(
        chord.mJoin, chord.mPattern, continuation,
        delegate ({4} a) {{ {5} }}
      );
      chord.mJoin.Register(copy);
   }}
", 
      typeArgs, contType, chordType ,retType, mkTuple(i), invocation);
    }
 

    static void Main(string[] args) {
      int max = 17;

      if (args.Length == 1 && Int32.TryParse(args[0], out max)) { max = max + 1; };


      System.Console.WriteLine(
@"
using System;
using Microsoft.Research.Joins.ArgCounts;


namespace Microsoft.Research.Joins {
  public static class ChordExtensions {
");

      for (int i = 0; i < max; i++) 
        System.Console.WriteLine(mkDo(i, true));
      for (int i = 0; i < max; i++) 
        System.Console.WriteLine(mkDo(i, false));

      System.Console.WriteLine("  } // class ChordExtensions");

      System.Console.WriteLine("} // namespace Microsoft.Research.Joins");
      
     // System.Console.ReadKey();
    }
  }


}