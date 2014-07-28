// Idealized code for the paper.
#if false
#region TryClaim
Msg[] TryClaim(Msg myMsg, ref bool retry) {
  var msgs = new Msg[Chans.length];

  for (int i = 0; i < Chans.Length; i++) {
    if (Chans[i] == myMsg.Chan) {
      msgs[i] = myMsg;
    } else {
      msgs[i] = Chans[i].FindPendingMsg(ref retry);
      if (msgs[i] == null) return null;
    }
  }
 
  for (int i = 0; i < Chans.Length; i++) {
    if (!msgs[i].TryClaim()) {
      for (int j = 0; j < i; j++) msgs[j].Rollback();
      retry = true;
      return null;
    }
  }

  return msgs;
}
#endregion TryClaim

#region TryChords
Pair<Chord, Msg[]> TryChords(Msg msg) {
  var backoff = new Backoff();
  while (true) {
    var retry = false;

    foreach (var chord in msg.chan.Chords) {
      Msg[] claims = chord.TryClaim(msg, ref retry);
      if (claims != null)
        return Pair.Create(chord, claims);
    }

    if (!retry || msg.IsConsumed || msg.IsAwoken) 
      return null;

    backoff.Once();
  }
}
#endregion TryChords

#region AsyncSend
void AsyncSend<A>(Chan<A> chan, A a) {
  HandleAsyncMsg(chan.Enqueue(a));
}
void RetryAsync(Msg msg) {
  if (!msg.IsConsumed) HandleAsyncMsg(msg);
}
void HandleAsyncMsg(Msg msg){
  Pair<Chord, Msg[]> res = TryChords(msg);
  if (res == null) return;

  var chord = res.Fst;
  var claims = res.Snd;

  if (chord.IsAsync) {
    new Thread(_ => chord.Fire(claims)).Start();
  } else {
    bool foundSleeper = false;
    for (int i = 0; i < chord.Chans.Length; i++) {
      if (chord.Chans[i].IsSync && !foundSleeper) {
        foundSleeper = true;
        claims[i].SetAsyncWaker(myMsg);
	claims[i].MarkAwoken();
        claims[i].WakeUpSignal.Set();
      } else {
        claims[i].Rollback();
      }
    }
  }
}
#endregion AsyncSend

#region SyncSend
R SyncSend<R, A>(Chan<A> chan, A a) {
  var backoff = new Backoff();
  Msg waker = null;

  Msg myMsg = chan.Enqueue(a);
  Pair<Chord, Msg[]> res; 
  
  while (true) {
    res = TryChords(myMsg);
    if (res != null) break;
    if (waker != null) RetryAsync(waker);

    myMsg.WakeUpSignal.Block(chan.SpinCount);

    if (myMsg.IsConsumed) return (R)myMsg.GetResult();
    waker = myMsg.GetAsyncWaker();
    myMsg.Rollback();
    backoff.Once();
  }

  var chord = res.Fst;
  var claims = res.Snd;

  ConsumeAll(claims);
  if (waker != null) RetryAsync(waker);

  // ignore exceptions for simplicity
  var r = chord.Fire(claims); 

  for (int i = 0; i < chord.Chans.Length; i++) {
    if (chord.Chans[i].IsSync) {
      claims[i].SetResult(r);
      claims[i].WakeUpSignal.Set();
    }
  }

  return (R)r;
}
#endregion SyncSend
#endif