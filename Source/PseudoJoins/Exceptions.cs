//------------------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

namespace Microsoft.Research.Joins {

  /// <summary>
  /// The class of exceptions thrown by assembly <c>Microsoft.Research.Joins</c>. 
  /// </summary>
  [global::System.Serializable]
  public sealed class JoinException : System.Exception {
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    internal JoinException() { }
    internal JoinException(string message) : base(message) { }
    internal JoinException(string message, System.Exception inner) : base(message, inner) { }
    internal JoinException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context) { }


    internal static void RepeatedChannelException() {
      throw new JoinException("The pattern joins a channel with itself. A channel may occur at most once in a pattern.");
    }

    internal static void UnInitializedChannelException() {
      throw new JoinException("The channel has not been initialized.");
    }

    internal static void UnInitializedChannelsException() {
      throw new JoinException("The channels have not been initialized.");
    }

    internal static void ForeignJoinException() {
      throw new JoinException("The channel values occurring in a pattern must all have been initialized from the same Join instance on which the pattern is declared.");
    }

    internal static void RedundantPatternException() {
      throw new JoinException("The pattern is redundant and overlaps with another pattern already declared on this Join instance.");
    }

    internal static void EmptyPatternException() {
      throw new JoinException("The pattern is empty, joining zero channels.");
    }

    internal static void SizeExceeded() {
      throw new JoinException("The number of channels that can be initialized from this Join instance is limited and has been exceeded.");
    }

    internal static void MaxSizeExceeded() {
      throw new JoinException("The requested size of this join exceeds the maximum permitted.");
    }

    internal static void AsynchronousChannelOverflow() {
      throw new JoinException("The maximum number of pending calls on a non-generic Asynchronous.Channel channel is finite and has been exceeded.");
    }

    internal static void OwnerNull() {
      throw new JoinException("The owner is null.");
    }

    internal static void NullContinuationException() {
      throw new JoinException("The pattern's continuation is null.");
    }
  }
}