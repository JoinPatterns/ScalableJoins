//------------------------------------------------------------------------------
// <copyright file="LBPatterns.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERLBTargetTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

using Microsoft.Research.Joins.BitMasks;

namespace Microsoft.Research.Joins.LockBased {

  internal partial class LBJoin<IntSet> where IntSet: struct, IIntSet<IntSet> {

    internal abstract class Pattern<P> {
      internal abstract P Get(ref Waiter waiters);
    }

    internal class Atom<P> : Pattern<P> {
      Chan<P> mCT;
      internal Atom(Patterns.Atom<P> p) {
        this.mCT = (Chan<P>)p.mCT;
      }

      internal override P Get(ref Waiter waiters) {
        return mCT.Get(ref waiters);
      }

    }



    internal class Vector<P> : Pattern<P[]> {
      internal Chan<P>[] mCTs;
      internal Vector(Patterns.Vector<P> p) {
        mCTs = new Chan<P>[p.mCTs.Length];
        for (int i = 0; i < p.mCTs.Length; i++)
          mCTs[i] = (Chan<P>)p.mCTs[i];

      }

      internal override P[] Get(ref Waiter waiters) {
        P[] ps = new P[mCTs.Length];
        for (int i = 0; i < mCTs.Length; i++) {
          ps[i] = mCTs[i].Get(ref waiters);
        }
        return ps;
      }

    }
    internal class Vector : Pattern<Unit> {
      internal Chan<Unit>[] mCTs;
      internal Vector(Patterns.Vector p) {
        mCTs = new Chan<Unit>[p.mCTs.Length];
        for (int i = 0; i < p.mCTs.Length; i++)
          mCTs[i] = (Chan<Unit>)p.mCTs[i];

      }
      internal override Unit Get(ref Waiter waiters) {
        var mCTs = this.mCTs;
        for (int i = 0; i < mCTs.Length; i++) {
          mCTs[i].Get(ref waiters);
        }
        return Unit.Null;
      }

    }
    internal class And<P, Q> : Pattern<Pair<P, Q>> {
      internal Pattern<P> mFst;
      internal Pattern<Q> mSnd;
      internal And(Patterns.And<P, Q> p) {
        mFst = p.mFst.Compile<IntSet>();
        mSnd = p.mSnd.Compile<IntSet>();
      }

      internal override Pair<P, Q> Get(ref Waiter waiters) {
        return new Pair<P, Q>(mFst.Get(ref waiters), mSnd.Get(ref waiters));
      }

    }

    internal class And<P> : Pattern<P> {
      Pattern<P> mFst;
      Pattern<Unit> mSnd;

      internal And(Patterns.And<P> p) {
        mFst = p.mFst.Compile<IntSet>();
        mSnd = p.mSnd.Compile<IntSet>();
      }


      internal override P Get(ref Waiter waiters) {
        mSnd.Get(ref waiters);
        return mFst.Get(ref waiters);
      }

    }
  }
}


namespace Microsoft.Research.Joins.Patterns {


  internal abstract partial class Pattern<P> : Pattern {

    internal abstract LockBased.LBJoin<IntSet>.Pattern<P> Compile<IntSet>()
      where IntSet : struct, IIntSet<IntSet>;

  }

  internal partial class Atom<P> : Pattern<P> {
    internal override LockBased.LBJoin<IntSet>.Pattern<P> Compile<IntSet>() {
      return new LockBased.LBJoin<IntSet>.Atom<P>(this);
    }
  }


  internal partial class Vector<P> : Pattern<P[]> {
    internal override LockBased.LBJoin<IntSet>.Pattern<P[]> Compile<IntSet>() {
      return new LockBased.LBJoin<IntSet>.Vector<P>(this);
    }
  }


  internal partial class Vector : Pattern<Unit> {
    internal override LockBased.LBJoin<IntSet>.Pattern<Unit> Compile<IntSet>() {
      return new LockBased.LBJoin<IntSet>.Vector(this);
    }

  }

  internal partial class And<P, Q> : Pattern<Pair<P, Q>> {
    internal override LockBased.LBJoin<IntSet>.Pattern<Pair<P, Q>> Compile<IntSet>() {
      return new LockBased.LBJoin<IntSet>.And<P, Q>(this);
    }
  }

  internal partial class And<P> : Pattern<P> {
    internal override LockBased.LBJoin<IntSet>.Pattern<P> Compile<IntSet>() {
      return new LockBased.LBJoin<IntSet>.And<P>(this);
    }
  }
}

#if false

using System;
using Microsoft.Research.Joins;
using Microsoft.Research.Joins.BitMasks;
using Microsoft.Research.Joins.LockBased;

namespace Microsoft.Research.Joins.Patterns {
  
  internal abstract partial class Pattern<P> : Pattern {
    //TODO: if we indexed Pattern<P> by return type R then we could make Waiter<R> strongly typed...
    internal abstract P Get(ref Waiter waiters);
  }


  internal partial class Atom<P> : Pattern<P> {
    internal override P Get(ref Waiter waiters) {
      return mCT.Get(ref waiters);
    }
  }


  internal partial class Vector<P> : Pattern<P[]> {
    internal override P[] Get(ref Waiter waiters) {
      P[] ps = new P[mCTs.Length];
      for (int i = 0; i < mCTs.Length; i++) {
        ps[i] = mCTs[i].Get(ref waiters);
      }
      return ps;
    }
  }


  internal partial class Vector : Pattern<Unit> {
    
    internal override Unit Get(ref Waiter waiters) {
      var mCTs = this.mCTs;
      for (int i = 0; i < mCTs.Length; i++) {
         mCTs[i].Get(ref waiters);
      }
      return Unit.Null;
    }
  }

  internal partial class And<P, Q> : Pattern<Pair<P, Q>> {
    internal override Pair<P, Q> Get(ref Waiter waiters) {
      return new Pair<P, Q>(mFst.Get(ref waiters), mSnd.Get(ref waiters));
    }
  }

  internal partial class And<P> : Pattern<P> {
    internal override P Get(ref Waiter waiters) {
      mSnd.Get(ref waiters); 
      return mFst.Get(ref waiters);
    }
  }
}
#endif