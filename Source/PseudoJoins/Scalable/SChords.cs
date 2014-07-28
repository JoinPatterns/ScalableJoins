//------------------------------------------------------------------------------
// <copyright file="SChords.cs" company="Microsoft">
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
using Microsoft.Research.Joins.Paper;
using Microsoft.Research.Joins.Paper.Bag;
using Microsoft.Research.Joins.BitMasks;

namespace Microsoft.Research.Joins {

  public partial class Chord {
    internal abstract Paper.Join<IntSet, MsgArray>.Chord Compile<IntSet, MsgArray>(Join<IntSet, MsgArray> join)
      where IntSet : struct, IIntSet<IntSet>
      where MsgArray: struct, IStackArray<Msg>;
  }

  public partial class Chord<A, R> {
    internal override Paper.Join<IntSet, MsgArray>.Chord Compile<IntSet, MsgArray>(Join<IntSet, MsgArray> join) {
      var mask = this.GetPattern().GetIntSet(join, default(IntSet));
      if (mask.IsEmpty()) JoinException.EmptyPatternException();

      int size = 0;
      for(int i = 0; i < join.Count; i++) { 
        var chan = join.Chans[i];
        if (mask.Contains(chan.id)) size++;
      }
      int k = 0;
      var chans = new Paper.Join<IntSet,MsgArray>.Chan[size];
      var isAsync = true;
       for(int i = 0; i < join.Count; i++) { // ensures sorted order
         var chan = join.Chans[i];
         if (mask.Contains(chan.id)) {
          isAsync &= chan.IsAsync;
          chans[k++] = (Paper.Join<IntSet, MsgArray>.Chan)chan;
        }
      }
      return new Paper.Join<IntSet, MsgArray>.Chord<A, R>(this,chans,isAsync);
    }

  }

}

namespace Microsoft.Research.Joins.Paper {
  internal partial class Join<IntSet, MsgArray> : Join<IntSet> {
    internal  abstract partial  class Chord<R>  : Chord  {
      internal Chord(bool isAsync, Chan[] chans)
        : base(isAsync, chans) {
      }
      internal abstract R FireSync(ref MsgArray claims);
  
    }
    internal partial class Chord<A,R> : Chord<R>
      {

     
      
      internal readonly Func<A, R> mContinuation;
      internal readonly Pattern<A> mPattern;
      #if CONTINUATIONATTRIBUTES
      internal readonly ContinuationAttribute mContinuationAttribute; 
      #endif

      

    
      internal Chord(Joins.Chord<A,R> chord, Chan[] chans, bool isAsync)
        : base(isAsync, chans) {
          this.mPattern = chord.mPattern.Compile<IntSet, MsgArray>();
          this.mContinuation = chord.mContinuation;
#if CONTINUATIONATTRIBUTES
          this.mContinuationAttribute = chord.mContinuationAttribute;
#endif
      }

      internal override void FireAsync(ref MsgArray claims) {
          var P = mPattern.Get(ref claims);
#if CONTINUATIONATTRIBUTES
          mContinuationAttribute.BeginInvoke(() => mContinuation(P));
#else
          new Thread(delegate() {
            mContinuation(P);
          }).Start();
#endif
      }

      internal override R FireSync(ref MsgArray claims) {
        var P = mPattern.Get(ref claims);
#if CONTINUATIONATTRIBUTES
        var r = default(R);
        mContinuationAttribute.Invoke(() => { r = mContinuation(P); });
        return r;
#else
        return mContinuation(P);
#endif

      }
    }
  }
}