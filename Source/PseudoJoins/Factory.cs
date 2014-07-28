//------------------------------------------------------------------------------
// <copyright file="Factory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------


using DEFAULT = Microsoft.Research.Joins.Join.Scalable;
//using DEFAULT = Microsoft.Research.Joins.Join.LBJoin;

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;


namespace Microsoft.Research.Joins {
  
  /// <summary>
  /// partial class with members dedicated to Join creation, including default implementation flavour.
  /// </summary>
  public abstract partial class Join {

    internal static int MAXSIZE = 128;
   
    /// <summary>
    /// The interface for allocating various flavours of joins.
    ///
    /// Define an empty struct implementing this interface to provide a new join flavour.
    /// </summary>
    public interface IJoinFactory {
      Join New<IntSet>(int size) where IntSet : struct, IIntSet<IntSet>;
    }

    /// <summary>
    /// Allocate a new  <see cref="Join"/> instance of default flavour and size.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Size"/>
    /// of this instance, which limits the number of its (a)synchronous channels, is 32.
    /// </para>
    /// <para>
    /// Use overload <c>Join.Create(n)</c> to create a new instance of specific size <c>n</c>.
    /// <c>Join.Create()</c> is equivalent to <c>Join.Create(32)</c> (and <c>Join.Create(32,false)</c>).
    /// </para>
    /// </remarks>
    /// <returns>A new instance of abstract class <see cref="Join"/> with an initially empty set of channels and chords.</returns>
    public static Join Create() {
      return Create(32);
    }

    /// <summary>
    /// Allocate a new  <see cref="Join"/> instance of a particular flavour and size.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Size"/>
    /// of this instance, which limits the number of its (a)synchronous channels, is 32.
    /// </para>
    /// </remarks>
    /// <typeparam name="JF"> The flavour of this join: <see cref="Join.Scalable"/> or  <see cref="Join.LockBased"/>. </typeparam>
    /// <returns>A new instance of abstract class <see cref="Join"/> with an initially empty set of channels and chords.</returns>
    public static Join Create<JF>() where JF : struct, IJoinFactory {
      return Create<JF>(32);
    }



    /// <summary>
    /// Allocate a new <see cref="Join"/> instance of default flavour and specified <paramref name="size"/>  (see <see cref="Size"/>).
    /// </summary>
    /// <remarks>
    /// Since <see cref="Join"/>  is an abstract class this factory method actually returns an object whose run-time type 
    /// is a particular subclass of <see cref="Join"/>. The precise subclass 
    /// depends on the value of <paramref name="size"/>.
    /// </remarks>
    /// <param name="size">  the maximum  number of channels 
    /// supported by this <see cref="Join"/> instance. Must be non-negative. </param>
    /// <returns>A new instance of abstract class <see cref="Join"/> with an initially empty set of channels and chords.</returns>
    public static Join Create(int size) {
      return Create<DEFAULT>(size);
    }

    /// <summary>
    /// Allocate a new  <see cref="Join"/> instance of a particular flavour and size. 
    /// </summary>
    /// <typeparam name="JF"> The flavour of this join: <see cref="Join.Scalable"/> or  <see cref="Join.LockBased"/>. </typeparam>
    /// <param name="size"></param>
    /// <returns>A new instance of abstract class <see cref="Join"/> with an initially empty set of channels and chords.</returns>
    public static Join Create<JF>(int size) where JF : struct, IJoinFactory {
     // if (size > MAXSIZE) JoinException.MaxSizeExceeded();

      var jf = default(JF);
      if (size <= 32) return jf.New<IntSet32>(size);
      if (size <= 64) return jf.New<IntSet64>(size);
      if (size % 64 == 0) {
        return Create<JF, IntSet64>(size, (size / 64) - 1);
      }
      else {
        return Create<JF, IntSet64>(size, (size / 64));
      }
    }

    private static Join Create<JF, IntSet>(int size, int n)
      where JF : struct, IJoinFactory
      where IntSet : struct, IIntSet<IntSet> {
      var jf = default(JF);
      if (n == 0) return jf.New<IntSet>(size);
      else return Create<JF, PairSet<IntSet>>(size, n >> 1);
    }
  }

}