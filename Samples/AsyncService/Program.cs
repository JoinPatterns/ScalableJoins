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
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.Joins;

interface IService<R, A> {
  Asynchronous.Channel<Pair<A, Asynchronous.Channel<R>>> Service { get;}
}

public class SimpleService : IService<string, int> {
  readonly Asynchronous.Channel<Pair<int, Asynchronous.Channel<string>>> Service;

  Asynchronous.Channel<Pair<int, Asynchronous.Channel<string>>> IService<string, int>.Service { get { return Service; } }

  public SimpleService() {
    Join j = Join.Create();
    j.Initialize(out Service);
    j.When(Service).Do(delegate(Pair<int, Asynchronous.Channel<string>> p)
    {
      string result = p.Fst.ToString(); // do some work with p.Fst
      for (int i = 0; i < p.Fst; i++) {
        Console.WriteLine("Service(...({0},...) on iteration {1}", p.Fst, i);
        Thread.Sleep(100);
      }
      Console.WriteLine("Service(...({0},...) responding", p.Fst);
      p.Snd(result); // send the result on p.Snd
    });
  }
}

public class JoinTwo<R1, R2> {
  public readonly Asynchronous.Channel<R1> First;
  public readonly Asynchronous.Channel<R2> Second;
  public readonly Synchronous<Pair<R1, R2>>.Channel Wait;
  public JoinTwo() {
    Join j = Join.Create();
    j.Initialize(out First);
    j.Initialize(out Second);
    j.Initialize(out Wait);
    j.When(Wait).And(First).And(Second).Do(
      delegate(R1 r1, R2 r2)
      {
        return new Pair<R1, R2>(r1, r2);
      });
  }
}

namespace Client {
  using Request = Pair<int, Asynchronous.Channel<string>>;
  class Program {
    public static void Main() {
      IService<string, int> s1, s2;
      s1 = new SimpleService();
      s2 = new SimpleService();
      JoinTwo<string, string> j = new JoinTwo<string, string>();
      Console.WriteLine("Issuing service requests.");
      s1.Service(new Request(10, j.First));
      s2.Service(new Request(20, j.Second));
      for (int i = 0; i < 30; i++) {
        Console.WriteLine("Client on iteration {0}", i);
        Thread.Sleep(100);
      }
      Pair<string, string> result = j.Wait(); // wait for both results to come back
      Console.WriteLine("first result={0}, second result={1}", result.Fst, result.Snd);
      Console.WriteLine("Hit return to exit!");
      Console.ReadLine();
    }
  }
}
