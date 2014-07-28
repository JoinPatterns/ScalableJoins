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
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.Joins;
using System.Diagnostics;


namespace LargeJoin {
  class Program {
    static void Test(int size) {
      Console.WriteLine("\n------------------");
      Console.WriteLine("Testing Join of size " + size);
      Join j = Join.Create(size);

      Synchronous.Channel sync;
      j.Initialize(out sync);
      Asynchronous.Channel<int>[] asyncs = new Asynchronous.Channel<int>[size - 1];

      for (int i = 0; i < size - 1; i++) {
        j.Initialize(out asyncs[i]);

        asyncs[i](i);
        int _i = i;
        j.When(sync).And(asyncs[i]).Do(delegate(int k) { 
          Debug.Assert(_i == k); 
          Console.Write(k +" ,"); });
        sync();
      }
    }
    static void Main(string[] args) {
      Test(1);
      Test(31);
      Test(32);
      Test(33);
      Test(64);
      Test(65);
      Test(127);
      Test(128);
      Test(129);
      Test(64 * 8 - 1);
      Test(64 * 8);
      Test(64 * 8 + 1);
      Test(64 * 16 - 1);
      Test(64 * 16);
      Test(64 * 16 + 1);
      Console.WriteLine("\nDone!");
      Console.ReadLine();
    }
  }
}
