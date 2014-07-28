using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Research.Joins.Paper {


  internal static class ThreadStatic<A> {
    [ThreadStatic]
    internal static A Value;
  }

  internal static class TLRandom {
    [ThreadStatic]
    private static Random rand;

    public static Random GetRand() {
      var r = rand;
      if (r == null) {
        r = new Random();
        rand = r;
      }
      return r;
    }
  }

  internal struct Backoff {
    int count;
    public void Once() {
      // todo: no spins on a single proc
      count++;
      if (count > 10) {
        Thread.Sleep(0);
        count = 0;
      } else if (count > 8) {
        Thread.Yield();
      } else {
        var r = TLRandom.GetRand();
        Thread.SpinWait(r.Next(4, (Thread.CurrentThread.ManagedThreadId % 100) + (4 << count)));
      }
    }
  }
}
