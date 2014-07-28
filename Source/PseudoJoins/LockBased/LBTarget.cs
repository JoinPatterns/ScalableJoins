//------------------------------------------------------------------------------
// <copyright file="LBTarget.cs" company="Microsoft">
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
using System.Diagnostics;
using System.Threading;

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;

namespace Microsoft.Research.Joins.LockBased {
  internal partial class LBJoin<IntSet> : Join<IntSet> where IntSet : struct, IIntSet<IntSet> {
    internal abstract class Chan : IChannelTarget {

#warning "TODO: remove me"
      internal readonly IntSet mSetID;

      internal readonly LBJoin<IntSet> mOwner;
      internal readonly int mID;
      int IChannelTarget.id { get { return mID; } }
      void IChannelTarget.CheckOwner(Join join) {
        if (join != mOwner) JoinException.ForeignJoinException();
      }

      public abstract bool IsAsync { get; }

      internal Chan(LBJoin<IntSet> join) {
        mID = join.NextChannel(this);
        mSetID = join.mState.Empty();
        mSetID.Add(mID);
        mOwner = join;
      }
    }
    internal abstract class Chan<A> : Chan, IChannelTarget<A> {
     
      internal Chan(LBJoin<IntSet> join) : base(join) { 
      }

      internal abstract A Get(ref Waiter waiters); 
    }
  }
}