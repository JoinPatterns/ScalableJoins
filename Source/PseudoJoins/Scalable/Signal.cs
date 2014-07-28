using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Research.Joins.Paper {
#if false

  internal class Signal {
    private bool mSet = false;

    public bool IsSet { get { return mSet; } }

    public void Set() {
      Monitor.Enter(this);
      mSet = true;
      Monitor.PulseAll(this);
      Monitor.Exit(this);
    }

    public void Reset() {
      Monitor.Enter(this);
      mSet = false;
      Monitor.Exit(this);
    }

    public void Wait() {
      Monitor.Enter(this);
      while (!mSet) {
        Monitor.Wait(this);
      }
      Monitor.Exit(this);
    }

    [ThreadStatic]
    private static Signal TLSignal;

    public static Signal GetTLSignal() {
      var sig = TLSignal;
      if (sig == null) {
        sig = new Signal();
        TLSignal = sig;
      }
      return sig;
    }
  }

#else 

  internal class Signal {
#warning "These constant factors require tuning"
    private const int MIN_SPINS = 256; // WAS 256
    private const int INITIAL_SPINS = 2048; // was 4096
    private const int MAX_SPINS = 4096; // WAS 2 << 12
    private int mProcCount;

    public Signal() {
      var procCount = 0;
     
        for(ulong mask = (ulong) System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity; mask > 0; mask = mask >> 1)
      {
          procCount++;
      }
     

      //mProcCount = Environment.ProcessorCount;
      mProcCount = procCount;
    }

    private ManualResetEventSlim mEvt = new ManualResetEventSlim(false,0);

    public bool IsSet { get { return mEvt.IsSet; } }

    public void Set() {
      mEvt.Set();
    }

    public void Reset() {
      mEvt.Reset();
    }

    public void Block(ThreadLocal<int> spinCount) {

      int curSpinCount = spinCount.Value;

      if (curSpinCount < 0)
      {
          //var numThreads = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
          //curSpinCount = INITIAL_SPIN * Math.Min(numThreads, mProcCount);
          curSpinCount = INITIAL_SPINS * mProcCount;
      }
         
      int spinTill = Math.Min(Math.Max(curSpinCount, mProcCount *  MIN_SPINS), mProcCount * MAX_SPINS);

      int newSpinTill = spinTill;

      int count;
      
      
      for (count = 0;  !mEvt.IsSet && count < spinTill; count++) ;

      if (!mEvt.IsSet) {
        if (curSpinCount > 0) newSpinTill -= spinTill >> 6;
        mEvt.Wait();
      } else if (curSpinCount < mProcCount * MAX_SPINS) {
        if (count > 64)
          newSpinTill += count >> 2;
        else
          newSpinTill += spinTill >> 4;
      }

      // low-pass filter:
      newSpinTill = curSpinCount + ((newSpinTill - spinTill) >> 3);

      if (newSpinTill != curSpinCount) {
        spinCount.Value = newSpinTill;
      }

      mEvt.Reset();
    }

    [ThreadStatic]
    private static Signal TLSignal;

    public static Signal GetTLSignal() {
      var sig = TLSignal;
      if (sig == null) {
        sig = new Signal();
        TLSignal = sig;
      }
      sig.Reset();
      return sig;
    }
  }

#endif
}
