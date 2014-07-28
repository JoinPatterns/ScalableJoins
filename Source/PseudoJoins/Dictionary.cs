//------------------------------------------------------------------------------
// <copyright file="Dictionary.cs" company="Microsoft">
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

namespace Microsoft.Research.Joins {
  internal partial class Join<IntSet> : Join where IntSet : IIntSet<IntSet> {
    // An IntSet indexed, cyclic linked list of values representing a queue.
    // Owners reference the last element in the queue, giving 0(1) access to the head and tail.
    // The empty queue is represented by null.
    // For fair scheduling, we advance the queue pointer so that the most recently scheduled entry becomes the new last entry.
    // For first-match scheduling we leave the queue pointer unchanged, and always search the queue from first to last entry.
    internal class Dictionary<A> {
      internal IntSet mKey;
      internal A mValue;
      internal Dictionary<A> mTail;

      internal Dictionary(ref Dictionary<A> last, IntSet key, A value) {
        mKey = key;
        mValue = value;
        if (last == null) {
          last = mTail = this;
        }
        else {
          mTail = last.mTail;
          last.mTail = this;
          last = this;
        }
      }

    }
  }
}