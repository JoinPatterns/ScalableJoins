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

// the Santa Claus Problem
// See: J. A. Trono. A new exercise in concurrency. SIGCSE Bulletin, 26(3):8-10,
//      1994. Corrigendum: 26(4):63.
// And: N. Benton. Jingle Bells: Solving the Santa Claus Problem in Polyphonic C# 2003
//      http://research.microsoft.com/~nick/publications.htm

using System;
using Microsoft.Research.Joins;

public class nway {
  string formatstring;

  private readonly Asynchronous.Channel<int> tokens; 
  public readonly Synchronous.Channel<int> entry;
  public void acceptn(int n, string message) {
    tokens(n);
    wait();
    Console.WriteLine(message);
  }
  private readonly Asynchronous.Channel allgone;
  private readonly Synchronous.Channel wait; 
  public nway(string formatstring){
    Join j = Join.Create();

    j.Initialize(out tokens);
    j.Initialize(out entry);
    j.Initialize(out allgone);
    j.Initialize(out wait);
    
    j.When(entry).And(tokens).Do(delegate(int id,int n){
      Console.WriteLine(formatstring, id);
      if (n==1) {
        allgone();
      } else {
      tokens(n-1);
      }
    });
    
    j.When(wait).And(allgone).Do(delegate(){});
    
    this.formatstring = formatstring;
    }
    
}

class santa {
  static Random ran = new Random();

 
  static readonly Asynchronous.Channel dlock;
  static readonly Synchronous.Channel delay;

  static int rcount = 0;
  static int ecount = 0;

  static nway harness = new nway("reindeer {0} is harnessed and ready to deliver");
  static nway unharness = new nway("reindeer {0} has been unharnessed and is off for a rest");
  static nway roomin = new nway("elf {0} is in the room");
  static nway roomout = new nway("elf {0} has been shown out again");

  // could be async but we'll call it from the main thread
  static void santalife() {
    while (true) {
      Console.WriteLine("Santa sleeping. Elves: {0} Reindeer: {1}",ecount,rcount);
      waittobewoken();
      // get back here when dealt with elves or reindeer
    }
  }

  
  static readonly Asynchronous.Channel elvesready;
  static readonly Asynchronous.Channel reindeernotready;
  static readonly Asynchronous.Channel reindeerready;
  static readonly Synchronous.Channel waittobewoken;
  static readonly Asynchronous.Channel<int /*elfid*/> elflife;
  static readonly Asynchronous.Channel<int /*e*/> elveswaiting;
  static readonly Synchronous.Channel elfqueue;
  static readonly Asynchronous.Channel<int /*rid*/> reindeerlife;
  static readonly Asynchronous.Channel<int /*r*/> reinwaiting;
  static Synchronous.Channel reindeerback;
  static Synchronous.Channel clearreindeernotready;

  static santa () {
      Join j = Join.Create(); 
      j.Initialize(out dlock);
      j.Initialize(out delay);
      j.When(delay).And(dlock).Do(delegate()
      {
          int d = ran.Next(3000);
          dlock();
          System.Threading.Thread.Sleep(d);
      }
      );
      j.Initialize(out elvesready);
      j.Initialize(out reindeernotready);
      j.Initialize(out reindeerready);
      j.Initialize(out waittobewoken);

      j.When(waittobewoken).And(elvesready).And(reindeernotready).Do(delegate()
      { 
        reindeernotready();
        Console.WriteLine("santa woken by elves");
        roomin.acceptn(3,"all elves in the room, engaging in consultation");
        elveswaiting(0);
        delay();
        roomout.acceptn(3,"all elves shown out of consulting room, off to bed");
        ecount++;
      });
      j.When(waittobewoken).And(reindeerready).Do(delegate()
      {
        Console.WriteLine("santa woken by reindeer");
        harness.acceptn(9,"all reindeer harnessed, off to deliver presents");
        reindeernotready();
        reinwaiting(0);
        delay();
        unharness.acceptn(9,"all reindeer unharnessed, off to bed");
        rcount++;
      });

      j.Initialize(out elflife);
      j.When(elflife).Do(delegate(int elfid)
      {
          while (true)
          {
              Console.WriteLine("elf {0} working", elfid);
              delay();
              Console.WriteLine("elf {0} needs help, queueing up", elfid);
              elfqueue();
              roomin.entry(elfid);
              roomout.entry(elfid);
          }
      });

      j.Initialize(out elfqueue);
      j.Initialize(out elveswaiting);
      j.When(elfqueue).And(elveswaiting).Do(delegate(int e)
      {
          if (e == 2)
          {
              // we're the last elf in a group so wake santa
              elvesready();
          }
          else
          {
              elveswaiting(e + 1);
          }
      });

      j.Initialize(out reindeerlife);
      j.When(reindeerlife).Do(delegate(int rid)
      {
          while (true)
          {
              Console.WriteLine("reindeer {0} is on holiday", rid);
              delay();
              Console.WriteLine("reindeer {0} has returned", rid);
              reindeerback();
              harness.entry(rid);
              unharness.entry(rid);
          }

      });

      j.Initialize(out reinwaiting);
      j.Initialize(out reindeerback);
      j.When(reindeerback).And(reinwaiting).Do(delegate(int r)
      {
          if (r == 8)
          {
              // last reindeer
              clearreindeernotready();
              reindeerready();
          }
          else
          {
              reinwaiting(r + 1);
          }
      }
      );

      j.Initialize(out clearreindeernotready);
      j.When(clearreindeernotready).And(reindeernotready).Do(delegate() { });
      

  }

  public static void Main() {
    for(int i=1; i<10; i++) {
      reindeerlife(i);
    }
    for (int j=1; j<11; j++) {
      elflife(j);
    }
    dlock();
    elveswaiting(0);
    reinwaiting(0);
    reindeernotready();
    santalife();
  }
 
}
