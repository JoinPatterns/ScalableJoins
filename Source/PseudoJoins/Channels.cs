//------------------------------------------------------------------------------
// <copyright file="Channels.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace Microsoft.Research.Joins {

  /// <summary>
  /// The most basic internal view of a Channel, which serves as the 
  /// target of any Channel delegate type.
  /// </summary>
  internal interface IChannelTarget { //: System.MarshalByRefObject {
    void CheckOwner(Join join);
    bool IsAsync { get; }
    /// <summary>Channel's ID, unique within a Join (and each channel belongs to a Join).</summary>
    int id { get; }

  }

  /// <summary>
  /// A typed internal view of a ChannelTarget.
  /// </summary>
  /// <typeparam name="A">The type of messages over the channel.  Void 
  /// argument channels are given type Unit</typeparam>
  internal interface IChannelTarget<A> : IChannelTarget {
   
  }

  /// <summary>
  /// Contains nested classes for synchronous channels returning one result of type <c>R</c>.
  /// </summary>
  /// <typeparam name="R"> the return type of nested channel types <c>Channel&lt;A&gt;</c> <c>Channel&lt;A&gt;</c> </typeparam>
  public static partial class Synchronous<R> {

    /// <summary>
    /// A synchronous channel that returns a value of type <c>R</c> and takes one argument of type <c>A</c>. 
    /// <para>
    /// A value <c>channel</c>of type <c>Synchronous&lt;R&gt;.Channel&lt;A&gt;</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other asynchronous channels initialised on <c>j</c>.
    /// Invoking <c>channel(a)</c> on an argument of type <c>A</c> will wait (block) until or unless some join pattern declared on <c>j</c> is enabled by zero or more pending asynchronous channel invocations.
    /// </para>
    /// </summary>
    /// <typeparam name="A">The type of the single argument to the delegate's implicit <c>Invoke</c> method.</typeparam>
    /// <param name="a">The single argument of the delegate's implicit <c>Invoke</c> method.</param>
    /// <returns>The result of type <c>R</c> of some join pattern declared on the synchronous channel.</returns>
    [DebuggerDisplay("{Target}")]
    public delegate R Channel<A>(A a);

    /// <summary>
    /// A synchronous channel that returns a value of type <c>R</c> and takes no arguments. 
    /// <para>
    /// A value <c>channel</c>of type  <c>Synchronous&lt;R&gt;.Channel</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other asynchronous channels initialised on <c>j</c>.
    /// Invoking <c>channel()</c>  will wait (block) until or unless some join pattern declared on <c>j</c> is enabled by zero or more pending asynchronous channel invocations.
    /// </para>
    /// </summary>
    /// <returns>The result of type <c>R</c> of some join pattern declared on the synchronous channel.</returns>
    [DebuggerDisplay("{Target}")]
    public delegate R Channel();

    /// <summary>
    /// Allocate a new synchronous channel (returning a value of type <c>R</c> and taking one argument of type <c>A</c>) on <see cref="Join"/> instance <paramref name="owner"/> . 
    /// This is an alternative to <c>Initialize(out channel)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <typeparam name="A"> the argument type of the channel.</typeparam>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when owner <paramref name="owner"/> is null. </exception>
    public static Synchronous<R>.Channel<A> CreateChannel<A>(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Synchronous<R>.Channel<A> sync;
      owner.Initialize(out sync);
      return sync;
    }

    /// <summary>
    /// Allocate a new synchronous channel (returning a value of type <c>R</c> and taking no argument) on <see cref="Join"/> instance <paramref name="owner"/> . 
    /// This is an alternative to <c>Initialize(out channel)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when owner <paramref name="owner"/> is null. </exception>
    public static Synchronous<R>.Channel CreateChannel(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Synchronous<R>.Channel sync;
      owner.Initialize(out sync);
      return sync;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <typeparam name="A">The message type of the channel.</typeparam>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<A> ToChannelTarget<A>(Channel<A> ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<A> ct = ch.Target as IChannelTarget<A>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<Unit> ToChannelTarget(Channel ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<Unit> ct = ch.Target as IChannelTarget<Unit>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }

    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<A>[] ToChannelTarget<A>(Channel<A>[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<A>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Synchronous<R>.ToChannelTarget(chs[i]); }
      return cts;
    }

   
    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<Unit>[] ToChannelTarget(Channel[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<Unit>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Synchronous<R>.ToChannelTarget(chs[i]); }
      return cts;
    }
  }

  /// <summary>
  /// Contains nested classes for synchronous channels returning <c>void</c>.
  /// </summary>
  public static partial class Synchronous {
    /// <summary>
    ///  A synchronous channel that returns void and takes one argument of type <c>A</c>.
    /// <para>
    /// A value <c>channel</c>of type <c>Synchronous.Channel&lt;A&gt;</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other asynchronous channels initialized on <c>j</c>.
    /// Invoking <c>channel(a)</c> on an argument of type <c>A</c> will wait (block) until or unless some join pattern declared on <c>j</c> is enabled by zero or more pending asynchronous channel invocations.
    /// </para>
    /// </summary>
    /// <typeparam name="A">The type of the single argument to the delegate's implicit <c>Invoke</c> method.</typeparam>
    /// <param name="a">The single argument of the delegate's implicit <c>Invoke</c> method.</param>
    [DebuggerDisplay("{Target}")]
    public delegate void Channel<A>(A a);


    /// <summary>
    /// A synchronous channel that returns void and takes no arguments.
    /// <para>
    /// A value <c>channel</c>of type <c>Synchronous.Channel</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other asynchronous channels initialised on <c>j</c>.
    /// Invoking <c>channel()</c> will wait (block) until or unless some join pattern declared on <c>j</c> is enabled by zero or more pending asynchronous channel invocations.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{Target}")]
    public delegate void Channel();

    /// <summary>
    /// Allocate a new synchronous channel (returning <c>void</c> and taking no argument) on <see cref="Join"/> instance <paramref name="owner"/> . 
    /// This is an alternative to <c>Initialize(out channel)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when owner <paramref name="owner"/> is null. </exception>
    public static Synchronous.Channel CreateChannel(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Synchronous.Channel sync;
      owner.Initialize(out sync);
      return sync;
    }

    /// <summary>
    /// Allocate a new synchronous channel (returning <c>void</c> and taking one argument of type <c>A</c>) on <see cref="Join"/> instance <paramref name="owner"/> . 
    /// This is an alternative to <c>Initialize(out channel)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <typeparam name="A"> the argument type of the channel.</typeparam>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when owner <paramref name="owner"/> is null. </exception>
    public static Synchronous.Channel<A> CreateChannel<A>(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Synchronous.Channel<A> sync;
      owner.Initialize(out sync);
      return sync;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <typeparam name="A">The message type of the channel.</typeparam>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<A> ToChannelTarget<A>(Channel<A> ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<A> ct = ch.Target as IChannelTarget<A>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<Unit> ToChannelTarget(Channel ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<Unit> ct = ch.Target as IChannelTarget<Unit>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }


    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<A>[] ToChannelTarget<A>(Channel<A>[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<A>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Synchronous.ToChannelTarget(chs[i]); }
      return cts;
    }


    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<Unit>[] ToChannelTarget(Channel[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<Unit>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Synchronous.ToChannelTarget(chs[i]); }
      return cts;
    }
  }
  /// <summary>
  /// Contains nested classes for asynchronous channels (returning <c>void</c>).
  /// </summary>
  public static class Asynchronous {
    /// <summary>
    /// An asynchronous channel that accepts one argument of type <c>A</c> and immediately returns void.
    /// <para>
    /// A value <c>channel</c>of type <c>Asynchronous.Channel&lt;A&gt;</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other synchronous and asynchronous channels initialised on <c>j</c>.
    /// Invoking <c>channel(a)</c> on an argument of type <c>A</c> queues the invocation and may enable the execution of some join pattern declared on <c>j</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="A"> The type of the single argument to the delegate's implicit <c>Invoke</c> method.</typeparam>
    /// <param name="a">The single argument of the delegate's implicit <c>Invoke</c> method.</param>
    [DebuggerDisplay("{Target}")]
    public delegate void Channel<A>(A a);

    /// <summary>
    /// An asynchronous channel that takes no arguments and immediately returns void.
    /// <para>
    /// A value <c>channel</c> of type <c>Asynchronous.Channel</c> allocated by some <see cref="Join"/> instance <c>j</c>  
    /// may then be joined with other synchronous and asynchronous channels initialised on <c>j</c>.
    /// Invoking <c>channel()</c> queues the invocation and may enable the execution of some join pattern declared on <c>j</c>.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{Target}")]
    public delegate void Channel();

    /// <summary>
    /// Allocate a new asynchronous channel (taking no argument) on  <see cref="Join"/> instance <paramref name="owner"/>. 
    /// This is an alternative to <c>Initialize(out channel)</c> that does not use an <c>out</c> parameter.
    /// </summary>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when <paramref name="owner"/> is null. </exception>
    public static Asynchronous.Channel CreateChannel(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Asynchronous.Channel async;
      owner.Initialize(out async);
      return async;
    }

    /// <summary>
    /// Allocate a new asynchronous channel (taking one argument of type <c>A</c>) on <see cref="Join"/> instance <paramref name="owner"/>. 
    /// This is an alternative to <c>Initialize(out channel)</c> that does not use an <c>out</c> parameter.
    /// </summary>
    /// <typeparam name="A"> the argument type of the channel.</typeparam>
    /// <param name="owner"> the join instance that owns the new channel.</param>
    /// <returns> a new channel owned by <paramref name="owner"/>.</returns>
    /// <exception cref = "JoinException"> when <paramref name="owner"/> is null. </exception>
    public static Asynchronous.Channel<A> CreateChannel<A>(Join owner) {
      if (owner == null) JoinException.OwnerNull();
      Asynchronous.Channel<A> async;
      owner.Initialize(out async);
      return async;
    }

    /// <summary>
    /// Allocate a new array of new asynchronous channels (each taking no argument) on <see cref="Join"/> instance <paramref name="owner"/> . 
    /// This is an alternative to <c>Initialize(out channels, length)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <param name="owner"> the join instance that owns the new channels.</param>
    /// <param name="length"> the number of distinct channels to create.</param>
    /// <returns> an array of <paramref name="length"/> distinct channels</returns>
    /// <exception cref = "JoinException"> when  <paramref name="owner"/> is null. </exception>
    public static  Asynchronous.Channel[] CreateChannels(Join owner,int length) {
      if (owner == null) JoinException.OwnerNull();
      Asynchronous.Channel[] asyncs;
      owner.Initialize(out asyncs, length);
      return asyncs;
    }

    /// <summary>
    /// Allocate a new array of new asynchronous channels (each taking one argument of type <c>A</c>) on <see cref="Join"/> instance <paramref name="owner"/> .  
    /// This is an alternative to <c>Initialize(out channels, length)</c>
    /// that does not use an <c>out</c> parameter.
    /// </summary>
    /// <typeparam name="A"> the argument type of each channel.</typeparam>
    /// <param name="owner"> the join instance that owns the new channels.</param>
    /// <param name="length"> the number of distinct channels to create.</param>
    /// <returns> an array of <paramref name="length"/> distinct channels</returns>
    /// <exception cref = "JoinException"> when <paramref name="owner"/> is null. </exception>
    public static Asynchronous.Channel<A>[] CreateChannels<A>(Join owner, int length) {
      if (owner == null) JoinException.OwnerNull();
      Asynchronous.Channel<A>[] asyncs;
      owner.Initialize(out asyncs, length);
      return asyncs;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <typeparam name="A">The message type of the channel.</typeparam>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<A> ToChannelTarget<A>(Channel<A> ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<A> ct = ch.Target as IChannelTarget<A>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }

    /// <summary>
    /// Access the ChannelTarget underlying a channel.
    /// </summary>
    /// <param name="ch">The channel.</param>
    /// <returns>The underlying target.</returns>
    internal static IChannelTarget<Unit> ToChannelTarget(Channel ch) {
      if (ch == null) JoinException.UnInitializedChannelException();
      IChannelTarget<Unit> ct = ch.Target as IChannelTarget<Unit>;
      if (ct == null) JoinException.ForeignJoinException();
      return ct;
    }

    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<Unit>[] ToChannelTarget(Channel[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<Unit>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Asynchronous.ToChannelTarget(chs[i]); }
      return cts;
    }

    /// <summary>
    /// Access the ChannelTargets underlying a channel array.
    /// </summary>
    /// <param name="chs">The channels.</param>
    /// <returns>The underlying targets.</returns>
    internal static IChannelTarget<A>[] ToChannelTarget<A>(Channel<A>[] chs) {
      if (chs == null) JoinException.UnInitializedChannelException();
      var cts = new IChannelTarget<A>[chs.Length];
      for (int i = 0; i < cts.Length; i++) { cts[i] = Asynchronous.ToChannelTarget(chs[i]); }
      return cts;
    }
  }
}
