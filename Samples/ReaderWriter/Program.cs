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

public class ReaderWriter {
  
  private readonly Asynchronous.Channel Idle;
  private readonly Asynchronous.Channel<int> Sharing;
  public readonly Synchronous.Channel Shared, ReleaseShared;
  public readonly Synchronous.Channel Exclusive, ReleaseExclusive;

  public ReaderWriter() {
    Join j = Join.Create();
    j.Initialize(out Idle);
    j.Initialize(out Sharing);
    j.Initialize(out Shared);
    j.Initialize(out ReleaseShared);
    j.Initialize(out Exclusive);
    j.Initialize(out ReleaseExclusive);
    
    j.When(Shared).And(Idle).Do(delegate{ Sharing(1);});
    j.When(Shared).And(Sharing).Do(delegate(int sharing){ 
      Sharing(sharing+1);});
    j.When(ReleaseShared).And(Sharing).Do(delegate(int sharing){ 
      if (sharing==1) Idle(); else Sharing(sharing-1);});
    j.When(Exclusive).And(Idle).Do(delegate{});
    j.When(ReleaseExclusive).Do(delegate{ Idle();});
  
    Idle(); 
  }

}

public class client {
  private Asynchronous.Channel run;

  ReaderWriter rwlock;
  int id;
  Random r = new Random();

  public client(int id, ReaderWriter rwlock) {

    Join j = Join.Create();
    
    j.Initialize(out run);
   
    j.When(run).Do(delegate() { 
       while (true) {
         read();
         delay();
         write();
         delay();};}
       );


    this.id=id;
    this.rwlock=rwlock;

    run();
  }


  void delay() {
    Thread.Sleep(100);
  }

  void read() {
    Console.WriteLine("{0} waiting to read", id);
    rwlock.Shared();
    Console.WriteLine("{0} reading", id);
    delay();
    rwlock.ReleaseShared();
    Console.WriteLine("{0} finished reading", id);
  }

  void write() {
    Console.WriteLine("{0} waiting to write", id);
    rwlock.Exclusive();
    Console.WriteLine("{0} writing", id);
    delay();
    rwlock.ReleaseExclusive();
    Console.WriteLine("{0} finished writing", id);
  }
}

public class Program {

  public static void Main() {
    ReaderWriter rwlock = new ReaderWriter();

    for(int i = 0; i<5; i++) {
      client w = new client(i,rwlock);
    }

    Console.ReadLine();
  }
}

