//------------------------------------------------------------------------------
// <copyright file="LBPatterns.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------
using Microsoft.Research.Joins.BitMasks;

namespace Microsoft.Research.Joins.Paper {
  internal partial class Join<IntSet, MsgArray> : Join<IntSet> {

    internal abstract class Pattern<P> {
      internal abstract P Get(ref MsgArray claims);
    }

    internal class Atom<P> : Pattern<P> {
      Chan<P> mCT;
      internal Atom(Patterns.Atom<P> p) {
        this.mCT = (Chan<P>)p.mCT;
      }

      internal override P Get(ref MsgArray claims) {
        return mCT.GetPayload(claims[mCT.id]);
      }
    }



    internal class Vector<P> : Pattern<P[]> {
      internal Chan<P>[] mCTs;
      internal Vector(Patterns.Vector<P> p) {
        mCTs = new Chan<P>[p.mCTs.Length];
        for (int i = 0; i < p.mCTs.Length; i++)
          mCTs[i] = (Chan<P>)p.mCTs[i];

      }

      internal override P[] Get(ref MsgArray claims) {
        P[] ps = new P[mCTs.Length];
        for (int i = 0; i < mCTs.Length; i++) {
          ps[i] = mCTs[i].GetPayload(claims[mCTs[i].id]);
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
      internal override Unit Get(ref MsgArray claims) {
#warning "assumes GetPayload is pure" 
       //for (int i = 0; i < mCTs.Length; i++) {
       //   mCTs[i].GetPayload(claims[mCTs[i].id]);
       // }
       return Unit.Null;
      }

    }
    internal  class And<P, Q> : Pattern<Pair<P, Q>> {
      internal Pattern<P> mFst;
      internal Pattern<Q> mSnd;
      internal And(Patterns.And<P, Q> p) {
        mFst = p.mFst.Compile<IntSet,MsgArray>();
        mSnd = p.mSnd.Compile<IntSet, MsgArray>();
      }

      internal override Pair<P, Q> Get(ref MsgArray claims) {
        return new Pair<P, Q>(mFst.Get(ref claims), mSnd.Get(ref claims));
      }
    }

    internal  class And<P> : Pattern<P> {
      Pattern<P> mFst;
      Pattern<Unit> mSnd;

      internal And(Patterns.And<P> p) {
        mFst = p.mFst.Compile<IntSet, MsgArray>();
        mSnd = p.mSnd.Compile<IntSet, MsgArray>();
      }

      internal override P Get(ref MsgArray claims) {
#warning "assumes Get is pure"
       //mSnd.Get(ref claims);
        return mFst.Get(ref claims);
      }

    }
  }
}


namespace Microsoft.Research.Joins.Patterns {


  internal abstract partial class Pattern<P> : Pattern {

    internal abstract Paper.Join<IntSet, MessageArray>.Pattern<P> Compile<IntSet, MessageArray>()
      where IntSet : struct, IIntSet<IntSet>
      where MessageArray : struct, IStackArray<Paper.Bag.Msg>;

  }

  internal partial class Atom<P> : Pattern<P> {
    internal override Paper.Join<IntSet, MessageArray>.Pattern<P> Compile<IntSet, MessageArray>() {
      return new Paper.Join<IntSet, MessageArray>.Atom<P>(this);
    }
  }


  internal partial class Vector<P> : Pattern<P[]> {
    internal override Paper.Join<IntSet, MessageArray>.Pattern<P[]> Compile<IntSet, MessageArray>() {
      return new Paper.Join<IntSet, MessageArray>.Vector<P>(this);
    }
  }


  internal partial class Vector : Pattern<Unit> {
    internal override Paper.Join<IntSet, MessageArray>.Pattern<Unit> Compile<IntSet, MessageArray>() {
      return new Paper.Join<IntSet, MessageArray>.Vector(this);
    }
    
  }

  internal partial class And<P, Q> : Pattern<Pair<P, Q>> {
    internal override Paper.Join<IntSet, MessageArray>.Pattern<Pair<P,Q>> Compile<IntSet, MessageArray>() {
      return new Paper.Join<IntSet, MessageArray>.And<P,Q>(this);
    }
  }

  internal partial class And<P> : Pattern<P> {
    internal override Paper.Join<IntSet, MessageArray>.Pattern<P> Compile<IntSet, MessageArray>() {
      return new Paper.Join<IntSet, MessageArray>.And<P>(this);
    }
  }
}

