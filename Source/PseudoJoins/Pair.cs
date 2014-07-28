//------------------------------------------------------------------------------
// <copyright file="Pair.cs" company="Microsoft">
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

namespace Microsoft.Research.Joins {
  /// <summary>
  /// Pairs of values of type A and B. This type is serializable.
  /// </summary>
  /// <typeparam name="A">The type of the first component <see cref="Fst"/>.</typeparam>
  /// <typeparam name="B">The type of the second component <see cref="Snd"/>.</typeparam>
 // [Serializable]
  public struct Pair<A, B> {
    /// <summary>
    /// The first component of the pair.
    /// </summary>
    public readonly A Fst;
    /// <summary>
    /// The second component of the pair.
    /// </summary>
    public readonly B Snd;
    /// <summary>
    /// Allocate a new pair. 
    /// </summary>
    /// <param name="fst">The value of the first component of the pair, <see cref="Fst"/></param>
    /// <param name="snd">The value of the second compoent of the pair, <see cref="Snd"/></param>
    public Pair(A fst, B snd) { this.Fst = fst; this.Snd = snd; }
  }
}