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

namespace OnePlaceBuffer {


  public class OnePlaceBuffer {
    private readonly Asynchronous.Channel Empty;
    private readonly Asynchronous.Channel<string> Contains;
    public readonly Synchronous.Channel<string> Put;
    public readonly Synchronous<string>.Channel Get;
    public OnePlaceBuffer() {
      Join j = Join.Create();
      j.Initialize(out Empty);
      j.Initialize(out Contains);
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Put).And(Empty).Do(delegate(string s)
      {
        Contains(s);
      });
      j.When(Get).And(Contains).Do(delegate(string s)
      {
        Empty();
        return s;
      });
      Empty();
    }
  }

  /* client code */
  class Program {

    static OnePlaceBuffer b = new OnePlaceBuffer();

    static void Producer() {
      Console.WriteLine("Producer Started");
      for (int i = 0; i < 20; i++) {
        b.Put(i.ToString());
        Console.WriteLine("Produced!{0}", i);
        Thread.Sleep(10);
      }
    }

    static void Consumer() {
      Console.WriteLine("Consumer Started");
      for (int i = 0; i < 20; i++) {
        string s = b.Get();
        Console.WriteLine("Consumed?{0}", s);
        Thread.Sleep(10);
      }
      Console.WriteLine("Done Consuming");
    }

    public static void Main() {
      Thread producer = new Thread(new ThreadStart(Producer));
      Thread consumer = new Thread(new ThreadStart(Consumer));
      producer.Start();
      consumer.Start();
      consumer.Join();
      Console.WriteLine("Hit return to exit");
      System.Console.ReadLine();
    }
  }
}
