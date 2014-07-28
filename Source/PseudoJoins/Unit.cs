//------------------------------------------------------------------------------
// <copyright file="Unit.cs" company="Microsoft">
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
  /// A unit type with a single, non-informative value <see cref="Null"/>.
  /// This type is serializable.
  /// <para>  This type is useful for specifying dummy arguments and return values.  
  /// When used as a return type, <c>Unit</c> is similar to <c>void</c>. Unlike <c>void</c>, however, <c>Unit</c> is a proper 
  /// type that can be used to instantiate generic type parameters.
  /// </para>
  /// </summary>
  [Serializable]
  public struct Unit {
    /// <summary>
    /// The single value of type <see cref="Unit"/>, equivalent to <c>default(Unit)</c>.
    /// </summary>
    public readonly static Unit Null;
  };

}