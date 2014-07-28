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
using System.Diagnostics;
using Microsoft.Research.Joins;

public class Future<T> {
  public delegate T Computation();
  public readonly Synchronous<T>.Channel Get;

  private readonly Asynchronous.Channel Execute, Done;
  private T Value;
  public Future(Computation comp) {
    if (comp == null) throw new ArgumentNullException();
    Join j = Join.Create();
    j.Initialize(out Get);
    j.Initialize(out Done);
    j.Initialize(out Execute);
    j.When(Get).And(Done).Do(delegate
    {
      Done(); // // reissue Done to allow multiple Gets
      return Value;
    });
    j.When(Execute).Do(delegate
    {
      Value = comp();
      comp = null; // discard comp to avoid space leak
      Done();
    });
    Execute();
  }
}

public class Test {

  public static int Max(int[] a, int start, int finish) {
    int max = a[start];
    for (int i = start; i < finish; i++) {
      if (max <= a[i]) max = a[i];
      if ((i % 128) == 0) Thread.Sleep(1); // artificial delay
    }
    return max;
  }

  // computes the maximum of a random integer array, first sequentially, 
  // then concurrently, using a Future<int> to start work on the top half 
  // of the array while working on the bottom half.
  static void Main() {

    int size = 10000;
    Random r = new System.Random();
    int[] a = new int[size];
    for (int i = 0; i < size; i++) {
      a[i] = r.Next();
    }
    Stopwatch stopwatch = new Stopwatch();

    for (int k = 1; k < 10; k++) {

      // sequential max
      stopwatch.Reset();
      stopwatch.Start();
      int sequentialMax = Max(a, 0, a.Length);
      stopwatch.Stop();
      Console.WriteLine("Sequential max time: {0} (milliseconds)  max: {1}", stopwatch.ElapsedMilliseconds, sequentialMax);

      // concurrent max
      stopwatch.Reset();
      stopwatch.Start();
      Future<int> topMax = new Future<int>(delegate { return Max(a, a.Length / 2, a.Length); });
      int bottomMax = Max(a, 0, a.Length / 2);
      int concurrentMax = (bottomMax <= topMax.Get()) ? topMax.Get() : bottomMax;
      stopwatch.Stop();
      Console.WriteLine("Concurrent max time: {0} (milliseconds) max: {1}", stopwatch.ElapsedMilliseconds, concurrentMax);
    }
    Console.WriteLine("Hit return to exit.");
    Console.ReadLine();
  }
}

