//------------------------------------------------------------------------------
// <copyright file="SyncOutcome.cs" company="Microsoft">
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
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Research.Joins.Paper.Bag;

namespace Microsoft.Research.Joins.Paper {

  internal partial class SJoin<IntSet, MessageArray> : Join<IntSet> {
    internal abstract class SyncOutcome {

      internal abstract class OfType<R> : SyncOutcome {
        abstract internal R Get();
      }

      internal class AsyncWakeup<R> : OfType<R> {
        private MessageRef mAsyncClaim;
        private Action mResend;

        internal override R Get() {
          throw new NotImplementedException();
        }

        internal void Resend() {
          mResend();
        }

        internal AsyncWakeup(MessageRef asyncClaim, Action resend) {
          mAsyncClaim = asyncClaim;
          mResend = resend;
        }

        public MessageRef AsyncClaim { get { return mAsyncClaim; } }
      }

      internal class WaitingOn<R> : OfType<R> {
        protected ManualResetEventSlim mSignal;
        protected Exception mThrownExn;
        protected R mResult;

        override internal R Get() {
          mSignal.Wait();
          mSignal.Reset();
          if (mThrownExn != null) throw mThrownExn;
          return mResult;
        }

        internal void Put(Exception exn) {
          mThrownExn = exn;
          mSignal.Set();
        }

        internal void Put(R ret) {
          mResult = ret;
          mSignal.Set();
        }

        internal WaitingOn(ManualResetEventSlim signal) {
          mSignal = signal;
        }
      }

      // static to avoid allocating closure when running directly from synchronous caller
      internal static R Fire<R>(MessageArray claims, SChord<R> chord, object payload, int claimID, ManualResetEventSlim signal) {
        if (signal != null) signal.Reset();

        R ret = default(R);
        Exception exn = null;
        Queue<MessageRef> waiters = null;

        //Console.WriteLine(claimID);

        for (var i = 0; i < chord.chans.Length; i++) {
          if (i != claimID && !chord.chans[i].IsAsync) {
            if (waiters == null) waiters = new Queue<MessageRef>(claims.Length);
            waiters.Enqueue(claims[i]);
          }
        }

        try {
          ret = chord.mClaimContinuationRef(ref claims);
        }
        catch (Exception e) {
          exn = e;
        }

        if (waiters != null) {
          foreach (var waiter in waiters) {
            if (exn == null) waiter.SetResult(ret);
            else throw new NotImplementedException();
            //Thread.MemoryBarrier();

            //Console.WriteLine("{0} setting", Thread.CurrentThread.ManagedThreadId);
            waiter.WakeUpSignal.Set();
          }
        }

        if (exn == null) return ret;
        throw exn;
      }

      internal class Thunk<R> : OfType<R> {
        protected ManualResetEventSlim mSignal;
        protected object mPayload;
        protected MessageArray mClaims;
        protected SChord<R> mChord;
        protected int mClaimID;

        override internal R Get() {
          return Fire(mClaims, mChord, mPayload, mClaimID, mSignal);
        }

        internal Thunk(MessageArray claims, Chord chord, object payload, int claimID, ManualResetEventSlim signal) {
          mClaims = claims;
          mChord = (SChord<R>)chord;
          mPayload = payload;
          mClaimID = claimID;
          mSignal = signal;
        }
      }
    }
  }
}
