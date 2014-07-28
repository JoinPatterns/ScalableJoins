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
//using System.Collections.Generic;
//using System.Text;
using System.Collections;
using System.Threading;
using Microsoft.Research.Joins;


namespace ActiveObjects {

  public abstract class ActiveObject {
    protected bool done = false;
    private readonly Asynchronous.Channel Start;
    protected Synchronous.Channel ProcessMessage;
    protected Join join;

    public ActiveObject() {
      join = Join.Create();
      join.Initialize(out ProcessMessage);
      join.Initialize(out Start);
      join.When(Start).Do(delegate
      {
        Thread.CurrentThread.IsBackground = true;
        while (!done) ProcessMessage();
      });
      Start();
    }
  }

  public interface EventSink {
    Asynchronous.Channel<string> Post { get; }
  }

  public class Distributor : ActiveObject, EventSink {
    private ArrayList subscribers = new ArrayList();
    private string myname;

    public readonly Asynchronous.Channel<EventSink> Subscribe;
    public readonly Asynchronous.Channel<string> Post;

    Asynchronous.Channel<string> EventSink.Post { get { return Post; } }

    public Distributor(string name) {
      join.Initialize(out Post);
      join.Initialize(out Subscribe);
      join.When(ProcessMessage).And(Subscribe).Do(
      delegate(EventSink sink)
      {
        subscribers.Add(sink);
      });
      join.When(ProcessMessage).And(Post).Do(
      delegate(string message)
      {
        foreach (EventSink sink in subscribers) {
          sink.Post(myname + ":" + message);
        }
      });
      myname = name;
    }
  }


  public class Subscriber : EventSink {
    private string name;
    private readonly Asynchronous.Channel<string> post;
    public Asynchronous.Channel<string> Post { get { return post; } }
    public Subscriber(string name) {
      Join j = Join.Create();
      j.Initialize(out post);
      j.When(post).Do(delegate(string message)
        {
          Thread.CurrentThread.IsBackground = true;
          Console.WriteLine("{0} got message {1}", name, message);
        });
      this.name = name;
    }
  }

  class Program {
    public static void Main() {
      Distributor d = new Distributor("D");
      Subscriber a = new Subscriber("a");
      d.Subscribe(a);
      Thread.Sleep(1000);
      d.Post("First message");
      Subscriber b = new Subscriber("b");
      d.Subscribe(b);
      Thread.Sleep(1000);
      d.Post("Second message");
      Subscriber c = new Subscriber("c");
      d.Subscribe(c);
      Thread.Sleep(1000);
      d.Post("Third message");
      Console.WriteLine("done");
      Console.ReadLine();
    }
  }
}
