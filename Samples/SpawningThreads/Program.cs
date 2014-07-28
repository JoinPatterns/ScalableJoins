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


public class ThreadSpawner {
  static readonly Asynchronous.Channel<string> Spawn;
  static ThreadSpawner() {
    Join join = Join.Create();
    join.Initialize(out Spawn);
    join.When(Spawn).Do(delegate(string s)
    {
      while (true) {
        Console.WriteLine(s);
        Thread.Sleep(1000);
      };
    });
  }
  static void Main() {
    Spawn("thread one");
    Spawn("thread two");
  }
}
