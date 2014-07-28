//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
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
using System.Threading;
using Microsoft.Research.Joins;

public class Counter {
  private readonly Asynchronous.Channel<long> mystate;
  public readonly Synchronous.Channel Inc;
  public readonly Synchronous<long>.Channel Value;
  public Counter() {
    Join join = Join.Create();
    join.Initialize(out mystate);
    join.Initialize(out Inc);
    join.Initialize(out Value);
    join.When(Inc).And(mystate).Do(delegate(long i)
    {  // acquire the state
      mystate(i + 1); // reissue the state
    });
    join.When(Value).And(mystate).Do(delegate(long i)
    { // acquire the state
      mystate(i); // reissue the state
      return i;
    });
    mystate(0);
  }
}

// this is a thread-unsafe counter, to demonstrate the difference
public class UnsafeCounter {
   private long i;
   public void Inc() { 
     long old = i;
     Thread.Sleep(7); // we sleep to provoke the race condition.
     i = old + 1; 
   } 
   public long Value() { return i; }
}

// [Main] Spawns [numMutators] Mutator threads, each performing [numIncrements] to two counters, 
// one thread safe, the other not, and reports the final counter values when all threads are done.

public class Program {
  private static int numMutators = 5;
  private static int numIncrements = 100;

  private static Counter counter = new Counter();
  private static UnsafeCounter unsafeCounter = new UnsafeCounter();
  
  private static void Mutator(Asynchronous.Channel Done) {
    Thread.CurrentThread.IsBackground = true;
    for (int i = 0; i < numIncrements; i++) {
      Console.Write(".");
      counter.Inc();
      unsafeCounter.Inc();
      Thread.Sleep(10);
     }
     Done();
   }

  static void Main() {
    
    // set up a join object to spawn and wait for [numMutator] threads.
    Asynchronous.Channel<Asynchronous.Channel> Spawn;
    Asynchronous.Channel[] Done;
    Synchronous.Channel Wait;
    Join j = Join.Create();
    j.Initialize(out Wait);
    j.Initialize(out Spawn);
    j.Initialize(out Done, numMutators);

    j.When(Wait).And(Done).Do(delegate { });
    j.When(Spawn).Do(Mutator);

    // Spawn the threads
    for(int i = 0; i < numMutators; i++) {
      Spawn(Done[i]);
    }

    // Wait for them to finish
    Wait();

    // Report the (un)expected counter values.
    Console.WriteLine("Expected counter values: {0}", numIncrements * numMutators);
    Console.WriteLine("Got: counter = {0}, unsafeCounter = {1}",
                       counter.Value(), unsafeCounter.Value());
    Console.WriteLine("Hit return to exit!");
    Console.ReadLine();
  }
}


