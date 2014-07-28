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

namespace JoinMany
{

  public class JoinMany<R> {
    private readonly Asynchronous.Channel<R>[] Responses;
    public readonly Synchronous<R[]>.Channel Wait;
    public Asynchronous.Channel<R> Response(int i) {
      return Responses[i];
    }
    public JoinMany(int n) {
      Join j = Join.Create(n + 1);
      j.Initialize(out Responses, n);
      j.Initialize(out Wait);
      j.When(Wait).And(Responses).Do(delegate(R[] results)
      {
        return results;
      });
    }
  }

  public class JoinMany {
    private readonly Asynchronous.Channel[] Responses;
    public readonly Synchronous.Channel Wait;
    public Asynchronous.Channel Response(int i) {
      return Responses[i];
    }
    public JoinMany(int n) {
      Join j = Join.Create(n + 1);
      j.Initialize(out Responses, n);
      j.Initialize(out Wait);
      j.When(Wait).And(Responses).Do(delegate() { });
    }
  }

  class Program
  {

    static void Main(string[] args)
    {
      
      int n = 100;
      // Wait for n results
      {
        JoinMany<string> nway = new JoinMany<string>(n);

        for (int i = 0; i < n; i++)
        {
          int k = i;
          new Thread(new ThreadStart(delegate()
          {
            Random r = new Random(k);
            System.Threading.Thread.Sleep(r.Next(20, 300));
            System.Console.Write("(Thread {0} responding)",k);
            nway.Response(k)(k.ToString());
          })).Start();
        }
        Console.WriteLine("\nMain thread waiting.");
        string[] result = nway.Wait();
        Console.WriteLine("\nMain thread got {0} results:", result.Length);
        foreach (string s in result)
        {
          Console.Write(s); Console.Write(";");
        }
      }

      Console.WriteLine("\n--------------------------------");
      // Wait for n replies.
      {
        JoinMany nway = new JoinMany(n);

        for (int i = 0; i < n; i++) {
          int k = i;
          new Thread(new ThreadStart(delegate()
          {
            Random r = new Random(k);
            System.Threading.Thread.Sleep(r.Next(20, 300));
            System.Console.Write("(Thread {0} responding)", k);
            nway.Response(k)();
          })).Start();
        }
        Console.WriteLine("\nMain thread waiting.");
        nway.Wait();
        Console.WriteLine("\nMain thread got all signals.");
        
        
      }

      Console.WriteLine("Hit return to exit!");
      Console.ReadLine();
    }
  }
}
