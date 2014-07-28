using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;



using Microsoft.Research.Joins.Paper.Bag;

namespace Microsoft.Research.Joins.Paper {

internal partial class Join<IntSet, MsgArray>  {


#warning "this might be faster if it wasn't an interface method"

#region TryClaim
partial class Chord<A,R> {
//++  Chan[] Chans; // channels in this chord, ordered by channel id.
  internal override 
  // try to claim enough messages -- including myMsg -- to fire
  bool TryClaim(ref MsgArray claims, Chan myChan, Msg myMsg,
                out bool sawClaimed) {
    sawClaimed = false;
    // first locate enough pending messages to fire the chord
    foreach (var chan in Chans)
      if (chan == myChan) claims[chan.id] = myMsg;
      else {
        Msg msg = chan.FindPendingMsg(out sawClaimed);
        if (msg.IsNull) return false;
        claims[chan.id] = msg;
      }
#if true
    // now try to claim the messages we found
    foreach (var chan in Chans) {
      if (!claims[chan.id].TryClaim()) {
        // another thread got a message before we did; revert 
        foreach(var claimed in Chans) {
          if (claimed == chan) break;
          var claim = claims[claimed.id];
          claim.Status = Stat.PENDING;
        }
        sawClaimed = true; // there may be CLAIMED messages
        return false;
      }
    } // success: each chan, claims[chan.id].Status == Stat.CLAIMED
#else

    // now try to claim the messages we found
    for(int i = 0; i < Chans.Length; i++)
    {
        var chan = Chans[i];
        if (!claims[chan.id].TryClaim())
        {
            // another thread got a message before we did; revert 
            for (int j = i - 1; j >= 0; j--) 
            {   var claimed = Chans[j];
                var claim = claims[claimed.id];
                claim.Status = Stat.PENDING;
            }
            sawClaimed = true; // there may be CLAIMED messages
            return false;
        }
    } // success: each chan, claims[chan.id].Status == Stat.CLAIMED
#endif
    return true; 
  }
}
#endregion TryClaim
}


internal partial class Join<IntSet, MsgArray> {
  static Chan ChannelOf(Msg msg) {
    //       Debug.Assert(!msg.IsNull);
#warning "TODO: remove this cast by making msg.Chan strongly typed (SegmentedBag must be nested)"
    return (Chan)msg.Chan();
  }
}
internal
#region Resolve
partial class Join<IntSet,MsgArray> {
  // resolves myMsg, which was already added to its channel's bag.
  // returns null: if no chord is firable or myMsg is consumed
  // o/w : a firable chord and sufficient claimed messages in claims
  static Chord Resolve(ref MsgArray claims, Chan chan, Msg myMsg) {
    var backoff = new Backoff();
    while (true) {
      bool retry = false, sawClaimed;
      foreach (var chord in chan.Chords) {
        if (chord.TryClaim(ref claims, chan, myMsg, out sawClaimed))
          return chord;
        retry |= sawClaimed;
      }

      if (!retry || myMsg.IsConsumed 
                 || myMsg.IsWoken)  return null;

      backoff.Once();
    }
  }

}
#endregion Resolve

#region Send
partial class Join<IntSet,MsgArray> {
  static void AsyncSend<A>(Chan<A> chan, A a) {
    HandleAsyncMsg(chan, chan.AddPendingMsg(a));
  }
  // resolve myMsg (which is asynchronous) and respond accordingly
  static void HandleAsyncMsg(Chan myChan, Msg myMsg) {
    MsgArray claims = new MsgArray();
    var chord = Resolve(ref claims, myChan, myMsg);
    if (chord == null) return; // no chord to fire; nothing to do

    if (chord.IsAsync) { // completely asynchronous chord 
      foreach (var ch in chord.Chans) claims[ch.id].Consume();
      chord.FireAsync(ref claims); // fire in a new thread!
    }
    else { // synchronous chord: wake a synchronous waiter
      bool foundSleeper = false;
      foreach (var chan in chord.Chans) {
        var claim = claims[chan.id];
        if (chan.IsSync && !foundSleeper) {
          foundSleeper = true;
          claim.AsyncWaker = myMsg; // transfer responsibility
          claim.Status = Stat.WOKEN;
          claim.Signal.Set();
        }
        else claim.Status = Stat.PENDING; // relinquish message
      }
    }
  }

  // create a message with payload a, and block until chord fired
  static R SyncSend<R, A>(Chan<A> chan, A a, bool fastpath) {
    MsgArray claims = new MsgArray();
    Msg waker = Msg.Null;
    Chord chord = (fastpath) ? FastPath(ref claims, chan, a) : null;

    if (chord == null) {
      var backoff = new Backoff();
      Msg myMsg = chan.AddPendingMsg(a);
      while (true) {
        chord = Resolve(ref claims, chan, myMsg);

        if (chord != null) break; // claimed a chord; break to fire

        if (!waker.IsNull)        // resend our last async waker
          RetryAsync(waker);
 
        myMsg.Signal.Block(chan.SpinCount);

        if (myMsg.IsConsumed)     // rendezvous: chord already fired
          return myMsg.GetResult<R>();

        waker = myMsg.AsyncWaker; // record waker to retry later
        myMsg.Status = Stat.PENDING;
        backoff.Once();
      }
    }
    // consume all claims
    foreach (var ch in chord.Chans) claims[ch.id].Consume();

    if (!waker.IsNull)      // resend our last async waker
      RetryAsync(waker);    // *after* consuming our own message

    try {
      R r = chord.FireSync<R>(ref claims);
      foreach (var other in chord.Chans)
        if (other.IsSync && other != chan) {// synchronous rendezvous
          claims[other.id].SetResult(r);    // transfer computed result
          claims[other.id].Signal.Set();
        }
      return r;
    }
    catch (Exception e) {
       foreach (var other in chord.Chans)
        if (other.IsSync && other != chan) {// synchronous rendezvous
          claims[other.id].SetException(e); // transfer computed exception
          claims[other.id].Signal.Set();
        }
       throw;
    }
  }
  static void RetryAsync(Msg myMsg) {
    if (!myMsg.IsConsumed) HandleAsyncMsg(ChannelOf(myMsg),myMsg);
  }
  static Chord FastPath<A>(ref MsgArray claims, Chan<A> chan, A a) {
    var chord = Resolve(ref claims, chan, Msg.Null);
    if (chord != null) ThreadStatic<A>.Value = a;
    return chord;
  }
}
#endregion Send

partial class Join<IntSet,MsgArray> {
  #region Fastpath
  
  #endregion Fastpath
} 
} 


