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


public class BoundedBuffer<T> {
  private readonly Asynchronous.Channel Token;
  private readonly Asynchronous.Channel<T> Value;
  public readonly Synchronous.Channel<T> Put;
  public readonly Synchronous<T>.Channel Get;

  public BoundedBuffer(int size) {
    Join join = Join.Create();
    join.Initialize(out Token);
    join.Initialize(out Value);
    join.Initialize(out Put);
    join.Initialize(out Get);
    join.When(Put).And(Token).Do(delegate(T t)
    {
      Value(t);
    });
    join.When(Get).And(Value).Do(delegate(T t)
    {
      Token();
      return t;
    });
    for (int i = 0; i < size; i++) {
      Token();
    }
  }
}

class Program {

  private static BoundedBuffer<int> b = new BoundedBuffer<int>(5);

  private static void Producer() {
    Thread.CurrentThread.IsBackground = true;
    int i = 0;
    while (true) {
      Console.WriteLine("Producing {0}", i);
      b.Put(i++);
      Thread.Sleep(75);
    }
  }

  private static void Consumer1() {
    Thread.CurrentThread.IsBackground = true;
    while (true) {
      Console.WriteLine("Consumer 1 Getting {0}", b.Get());
      Thread.Sleep(100);
    }
  }

  private static void Consumer2() {
    Thread.CurrentThread.IsBackground = true;
    while (true) {
      Console.WriteLine("Consumer 2 Getting {0}", b.Get());
      Thread.Sleep(50);
    }
  }

  static void Main() {
    Console.WriteLine("Hit Return to exit.");
    new Thread(new ThreadStart(Producer)).Start();
    new Thread(new ThreadStart(Consumer1)).Start();
    new Thread(new ThreadStart(Consumer2)).Start();
    Console.ReadLine();
  }

}

