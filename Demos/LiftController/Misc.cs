//------------------------------------------------------------------------------
// <copyright file="Misc.cs" company="Microsoft">
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

namespace LiftController
{
 
  class Helpers
  {
    public static Pair<A, B> pair<A, B>(A a, B b) { return new Pair<A, B>(a, b); }
  }

  public abstract class Lst<T>
  {
    public static readonly Nil<T> nil = new Nil<T>();
    public abstract bool Null();
    public abstract T Hd();
    public abstract Lst<T> Tl();
    public abstract R Case<R>(Converter<Nil<T>, R> nullCase, Converter<Cons<T>, R> headCase);
    public Lst<T> Cons(T head) { return new Cons<T>(head, this); }
    public Lst<T> ReverseAppend(Lst<T> acc)
    {
      Lst<T> r = acc;
      Lst<T> cur = this;
      while (!cur.Null())
      {
        r = r.Cons(cur.Hd());
        cur = cur.Tl();
      }
      return r;
    }

    public Lst<T> Reverse()
    {
      return this.ReverseAppend(nil);
    }
  }

  public class Nil<T> : Lst<T>
  {
    public override bool Null() { return true; }
    public override T Hd() { throw new System.InvalidOperationException(); }
    public override Lst<T> Tl() { throw new System.InvalidOperationException(); }
    public override R Case<R>(Converter<Nil<T>, R> nullCase, Converter<Cons<T>, R> headCase)
    {
      return nullCase(this);
    }
  }

  public class Cons<T> : Lst<T>
  {
    public readonly T Head; public readonly Lst<T> Tail;
    public override bool Null() { return false; }
    public override T Hd() { return Head; }
    public override Lst<T> Tl() { return Tail; }

    public Cons(T head, Lst<T> tail) { Head = head; Tail = tail; }

    public override R Case<R>(Converter<Nil<T>, R> nullCase, Converter<Cons<T>, R> headCase)
    {
      return headCase(this);
    }
  }

  public class Buffer<T>
  {
    public readonly Asynchronous.Channel<T> Put;
    public readonly Synchronous<T>.Channel Get;
    public Buffer()
    {
      Join j = Join.Create();
      j.Initialize(out Put);
      j.Initialize(out Get);
      j.When(Get).And(Put).Do(delegate(T t) { return t; });
    }
  }


  public class Ack
  {
    public readonly Asynchronous.Channel Send;
    public readonly Synchronous.Channel Receive;
    public Ack()
    {
      Join j = Join.Create();
      j.Initialize(out Send);
      j.Initialize(out Receive);
      j.When(Receive).And(Send).Do(delegate { });
    }
  }

  public class ActiveObject
  {
    protected bool Done = false;
    protected Join join = Join.Create();
    protected readonly Synchronous.Channel ProcessMessage;
    public readonly Asynchronous.Channel Start;

    public ActiveObject()
    {
      join.Initialize(out ProcessMessage);
      join.Initialize(out Start);

      join.When(Start).Do(MainLoop);
    }

    private void MainLoop()
    {
      Thread.CurrentThread.IsBackground = true;
      Thread.CurrentThread.Name = this.GetType().ToString();
      while (!Done) ProcessMessage();
    }

  }

  


}
