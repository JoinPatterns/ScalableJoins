//------------------------------------------------------------------------------
// <copyright file="LBChords.cs" company="Microsoft">
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
using Microsoft.Research.Joins.LockBased;
using System.Diagnostics;



using Microsoft.Research.Joins.Paper;
using Microsoft.Research.Joins.Paper.Bag;
using Microsoft.Research.Joins.BitMasks;

namespace Microsoft.Research.Joins {

  public partial class Chord {
    internal abstract LockBased.LBJoin<IntSet>.Chord Compile<IntSet>(LBJoin<IntSet> join)
      where IntSet : struct, IIntSet<IntSet>;

  }

  public partial class Chord<A, R> {
    internal override LockBased.LBJoin<IntSet>.Chord Compile<IntSet>(LBJoin<IntSet> join) {
      var mask = this.GetPattern().GetIntSet(join, default(IntSet));
      if (mask.IsEmpty()) JoinException.EmptyPatternException();

      var isAsync = true;
      LockBased.LBJoin<IntSet>.Chan syncChan = null;
      for(int i = 0; i < join.Count; i++) { 
        var chan = join.Chans[i];
        if (mask.Contains(chan.id)) {
          isAsync &= chan.IsAsync;
          if (syncChan == null & !chan.IsAsync) syncChan = (LockBased.LBJoin<IntSet>.Chan)chan;
        }
      }
      return new LockBased.LBJoin<IntSet>.Chord<A, R>(this, isAsync, syncChan, ref mask);
    }

  }

}

namespace Microsoft.Research.Joins.LockBased {
 
  partial class LBJoin<IntSet> where IntSet : struct, IIntSet<IntSet>  {


    internal abstract partial class Chord {
    
      internal readonly bool IsAsync;
      internal readonly IntSet mask;
      internal readonly Chan syncChan;

      internal Chord(bool isAsync, Chan syncChan, ref IntSet mask) {
        this.IsAsync = isAsync;
        this.syncChan = syncChan;
        this.mask = mask;
      }
      

      internal abstract void FireAsync();
    }

    internal abstract partial class Chord<R> : Chord {
      internal Chord(bool isAsync, Chan syncChan, ref IntSet mask)
        : base(isAsync, syncChan, ref mask) {
      }


      internal abstract R FireSync();

    }

    internal partial class Chord<A, R> : Chord<R> {
      internal readonly LBJoin<IntSet> mOwner;
      internal readonly Func<A, R> mContinuation;
      internal readonly Pattern<A> mPattern;
       
      
#if CONTINUATIONATTRIBUTES
      internal readonly ContinuationAttribute mContinuationAttribute; 
#endif

      




      internal Chord(Joins.Chord<A, R> chord, bool isAsync, Chan syncChan, ref IntSet mask)
        : base(isAsync, syncChan, ref mask) {
        this.mOwner = (LBJoin<IntSet>)chord.mJoin;
        this.mPattern = chord.mPattern.Compile<IntSet>();
        this.mContinuation = chord.mContinuation;
#if CONTINUATIONATTRIBUTES
        this.mContinuationAttribute = chord.mContinuationAttribute;
#endif
      }

      internal override void FireAsync() {
        Waiter waiters = null;
        var a = mPattern.Get(ref waiters);
#if CONTINUATIONATTRIBUTES
        mContinuationAttribute.BeginInvoke(() => mContinuation(a));
#else
        new Thread(delegate() {
          mContinuation(a);
        }).Start();
#endif
      }

      internal override R FireSync() {
        Waiter waiters = null;
        var a = mPattern.Get(ref waiters);
        mOwner.Scan();
        Monitor.Exit(mOwner);
        try {
#if CONTINUATIONATTRIBUTES
          var r = default(R);
          mContinuationAttribute.Invoke(() => { r = mContinuation(a);} );
#else
          var r = mContinuation(a);
#endif  
          if (waiters != null) ((Waiter<R>)waiters).Succeed(r);
          return r;
        }
        catch (Exception e) {
          if (waiters != null) waiters.Fail(e);
          throw;
        }

      }
    }
  }
}




