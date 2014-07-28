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

public class Semaphore {
  public readonly Asynchronous.Channel Signal;
  public readonly Synchronous.Channel Wait;
  public Semaphore() {
    Join join = Join.Create();
    join.Initialize(out Signal);
    join.Initialize(out Wait);
    join.When(Wait).And(Signal).Do(delegate { });
  }
}

public class Program {
  static Asynchronous.Channel<Pair<int, Semaphore>> Task1, Task2;

  public static void Main() {
    Semaphore s = new Semaphore();
    Task1(new Pair<int, Semaphore>(1, s));
    Task2(new Pair<int, Semaphore>(2, s));
    // do something in main thread
    s.Wait(); // wait for one to finish
    Console.WriteLine("one task finished");
    s.Wait(); // wait for another to finish
    Console.WriteLine("both tasks finished");
    Console.WriteLine("Hit return to exit.");
    Console.ReadLine();
  }

  static Program() {
    Join join = Join.Create();
    join.Initialize(out Task1);
    join.Initialize(out Task2);
    join.When(Task1).Do(delegate(Pair<int, Semaphore> p)
    {
      Console.WriteLine("Task1 started");
      // do something with p.Fst
      Thread.Sleep(3000);
      Console.WriteLine("Task1 signalling");
      p.Snd.Signal();
    });
    join.When(Task2).Do(delegate(Pair<int, Semaphore> p)
    {
      Console.WriteLine("Task2");
      Thread.Sleep(4000);
      // do something else with p.Fst
      Console.WriteLine("Task2 signalling");
      p.Snd.Signal();
    });
  }
}

