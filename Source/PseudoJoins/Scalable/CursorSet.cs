using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Research.Joins.BitMasks;
using Microsoft.Research.Joins.Paper.Bag;

namespace Microsoft.Research.Joins.Paper {
  /*
  internal struct CursorSet<IntSet> where IntSet : IIntSet<IntSet> {

    internal IntSet mSawClaimed;
    internal IntSet mStarted;
    internal MessageRef[] mCursors;
    internal MessageRef mAvoid;

    public CursorSet(int size) {
      mSawClaimed = default(IntSet).Empty(); 
      mStarted = default(IntSet).Empty(); 
      mCursors = new MessageRef[size];

      mAvoid = default(MessageRef);
    }

    public void Avoid(MessageRef avoid) {
      mAvoid = avoid;
    }   

    public void Clear() {
      mSawClaimed = default(IntSet);
      mStarted = default(IntSet);
    }

    public bool SawClaimed(ChannelTarget c) {
      return mSawClaimed.Contains(c.mID);
    }

    public bool NextPending(ChannelTarget c, ref MessageRef loc) {
      var id = c.mID;
      MessageRef cursor;

      if (!mStarted.Contains(id)) {
        cursor = ((IChannel)c).Head();
        mStarted.Add(id);
      } else {
        cursor = mCursors[id];
        if (!cursor.IsNull) 
          cursor.MoveNext();
      }

      if (cursor.IsNull) {
        loc.Clear();
        mCursors[id] = cursor;
        return false;
      }

      Status s = cursor.status;
      while (s != Status.PENDING || cursor.Is(mAvoid)) {
        if (s == Status.CLAIMED && !cursor.Is(mAvoid))
          mSawClaimed.Add(id);
        cursor.MoveNext();
        if (cursor.IsNull) {
          loc.Clear();
          mCursors[id] = cursor;
          return false;
        }
        s = cursor.status;
      }

      loc = cursor;
      mCursors[id] = cursor;
      return true;
    }
  }
   */
}
