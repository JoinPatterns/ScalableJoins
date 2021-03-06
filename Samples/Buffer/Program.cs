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

//some examples

namespace Buffer {

  
  class Buffer {
    // Declare the (a)synchronous channels
    public readonly Asynchronous.Channel<string> Put;
    public readonly Synchronous<string>.Channel Get;
    public Buffer() {
      // Allocate a new Join object for this buffer
      Join join = Join.Create();
      // Use it to initialize the channels
      join.Initialize(out Put);  
      join.Initialize(out Get);
      // Finally, declare the patterns(s)
      join.When(Get).And(Put).Do(delegate(String s) {
         return s; 
      });
    }
  }

  /* client code */
  class Program {

    static Buffer b = new Buffer();
   
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
        Thread.Sleep(8);
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
