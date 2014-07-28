//------------------------------------------------------------------------------
// <copyright file="Join.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

#warning "TODO: exception handling, cleanup of LB firing, AndPair methods?, missing When methods"

// LOCKING determines whether or not to lock on Initialize and When for thread safe Join setup.
#define LOCKING

using System;
using System.Diagnostics;
using System.Threading;

using Microsoft.Research.Joins;
using Microsoft.Research.Joins.Patterns;
using Microsoft.Research.Joins.BitMasks;

[assembly: System.Security.AllowPartiallyTrustedCallers()]



namespace Microsoft.Research.Joins {
  /// <summary>
  /// The Join class provides a mostly declarative, type-safe mechanism for defining
  /// thread-safe asynchronous and synchronous communication channels and patterns, for
  /// use by concurrently executing local threads or remote processes.
  /// </summary>
  /// <remarks>
  /// <para>A new Join instance is allocated by calling an overload of factory method <c>Create</c>.</para>
  /// <para>A Join object notionally owns a set of channels, each obtained by calling an overload of
  /// method <c>Initialize</c>.
  /// Asynchronous channels are instances of types <c>Asynchronous.Channel</c> or <c>Asynchronous.Channel&lt;A&gt;</c>.
  /// Synchronous channels are instances of types <c>Synchronous.Channel</c>, <c>Synchronous.Channel&lt;A&gt;</c>, <c>Synchronous&lt;R&gt;.Channel</c> or <c>Synchronous&lt;R&gt;.Channel&lt;A&gt;</c>.
  /// The various channel flavours support zero or one arguments of type <c>A</c>, and zero or one return values of type <c>R</c>.
  /// Multiple arguments or results can be passed using user-declared types or the provided generic <c>Pair&lt;A,B&gt;</c> type.
  /// </para>
  /// <para>When a synchronous channel is called, the caller
  /// is blocked until the channel returns (void or some value). 
  /// When an asynchronous channel is called, there is no result and the
  /// caller proceeds immediately without being blocked.
  /// </para>
  /// <para>A Join object also notionally owns a set of chords. Each pattern is declared by calling an overload of 
  /// <c>When</c> 
  /// and takes the general form:
  /// </para>
  /// <para>
  ///     <code>join.When(a1).And(a2) ... .And(an).Do(d);</code>
  /// </para>
  /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
  /// <para>
  ///     <code>join.When(a1).And(a2) ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
  /// </para>
  /// <para> Here, the head <c>a1</c> is a synchronous or asynchronous channel or an array of asynchronous channels and, for <c>i</c>&gt;1,
  /// <c>ai</c> is an asynchronous channel or an array of asynchronous channels (but never synchronous).
  /// The body <c>d</c> is a continuation delegate of type:
  /// </para>
  /// <para>
  /// <code>public delegate R JoinPattern&lt;R&gt;.OpenPattern&lt;P1,...,Pm&gt;.Continuation(P1 p1, ...,Pm pm);</code>, or
  /// </para>
  /// <para>
  /// <code>public delegate void JoinPattern.OpenPattern&lt;P1,...,Pm&gt;.Continuation(P1 p1, ...,Pm pm);</code>
  /// </para>
  /// <para>
  /// depending on whether the head <c>a1</c> is a channel that returns a result of some type <c>R</c> or <c>void</c>.
  /// The body receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
  /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
  /// The presence and types of any additional parameters <c>P1 p1, ... Pm pm</c> 
  /// varies according the type of each argument <c>ai</c> joined with invocation <c>When(ai)</c>/<c>And(ai)</c> (for 1&lt;=<c>i</c>&lt;=<c>n</c>):
  /// </para>
  /// <list type="bullet">
  /// <item><description> If <c>ai</c> is of type <c>Synchronous&lt;R&gt;.Channel</c>, <c>Synchronous.Channel</c>, <c>Asynchronous.Channel</c> or  <c>Asynchronous.Channel[]</c> then <c>When(ai)</c>/<c>And(ai)</c> adds no parameter to delegate <c>d</c>.</description></item>
  /// <item><description> If <c>ai</c> is of type <c>Synchronous&lt;R&gt;.Channel&lt;P&gt;</c>, <c>Synchronous.Channel&lt;P&gt;</c>, or <c>Asynchronous.Channel&lt;P&gt;</c>  for some type <c>P</c> then <c>When(ai)</c>/ <c>And(ai)</c>
  /// adds one parameter <c>pj</c> of type <c>Pj=P</c> to delegate <c>d</c>.</description></item>
  /// <item><description> If <c>ai</c> is an array of type <c>Asynchronous.Channel&lt;P&gt;[]</c> for some type <c>P</c> then <c>When(ai)/And(ai)</c>  
  /// adds one parameter <c>pj</c> of array type <c>Pj=P[]</c> to delegate <c>d</c>. </description></item>
  /// <item><description> Parameters are added to <c>d</c> in increasing order of <c>i</c>.</description></item>
  /// </list>
  /// <para> Each chord associates a set of (a)synchronous channels with a body <c>d</c>.
  /// The body of a pattern can only execute once all the
  /// channels guarding it have been called. Thus, when a synchronous or asynchronous channel is 
  /// called there may be zero, one, or more patterns which are enabled:
  /// </para>
  /// <list type="bullet">
  /// <item><description> If no pattern is enabled then the channel invocation is queued up. If the channel is asynchronous,
  /// then this simply involves adding the arguments (the contents of the message) to an internal queue.
  /// If the channel is synchronous, then the calling thread is blocked.</description></item>
  /// <item><description>If there is a single enabled chord, then the arguments of the calls involved in the match are de-queued,
  /// any blocked thread involved in the match is awakened, and the body of the pattern is executed in one of the threads.</description></item>
  /// <item><description>When a chord that involves only asynchronous channels is enabled, then it runs in a new thread.</description></item>
  /// <item><description>If there are several enabled patterns, then an unspecified one is chosen to run.</description></item>
  /// <item><description>Similarly, if there are multiple calls to a particular (a)synchronous channel qeued up, 
  /// which call will be de-queued when there is a match is unspecified.</description></item>
  /// </list>
  /// <para>chords must be well-formed. Executing <c>Do(d)</c> to complete a chord will throw <see cref="JoinException"/> if <c>d</c> is null, the pattern repeats an
  /// asynchronous channel,
  /// an (a)synchronous channel is null or foreign to this pattern's join instance, the chord is redundant, 
  /// or the chord is empty. A chord is redundant 
  /// when the set of channels joined by the pattern 
  /// subsets or supersets the channels joined by an existing pattern on this <c>Join</c> instance.
  /// A chord is empty when the set of channels joined by the pattern is the empty set
  /// (emptiness can only arise from joining empty arrays of channels).
  /// </para>
  /// </remarks>
  /// <example>
  /// Here are two examples:
  /// <code>
  /// /* An unbounded, unordered string buffer.
  ///    Producers send strings by calling b.Put(s), never waiting. 
  ///    Consumers receive strings by calling b.Get(), possibly waiting until/unless there is a matching b.Put(s). */
  ///  class Buffer {
  ///    // Declare the (a)synchronous channels
  ///    public readonly Asynchronous.Channel&lt;string&gt; Put;
  ///    public readonly Synchronous&lt;string&gt;.Channel Get;
  ///    public Buffer() {
  ///      // Allocate a new Join object for this buffer
  ///      Join join = Join.Create();
  ///      // Use it to initialize the channels
  ///      join.Initialize(out Put);  
  ///      join.Initialize(out Get);
  ///      // Finally, declare the patterns(s)
  ///      join.When(Get).And(Put).Do(delegate(String s) {
  ///       return s; 
  ///      });
  ///    }
  ///  }
  /// </code>
  /// <code>
  /// /* An object joinmany = new JoinMany&lt;R&gt;(n) waits for n responses of type R, arriving asynchronously.
  ///    The example demonstrates joining with an array of asynchronous channels.
  ///    Producer i posts his results on joinmany.Response(i), asynchronously.
  ///    The consumer calls joinmany.Wait(), blocking until/unless all responses have come in. */
  ///  public class JoinMany&lt;R&gt; {
  ///    private readonly Asynchronous.Channel&lt;R&gt;[] Responses;
  ///    public readonly Synchronous&lt;R[]&gt;.Channel Wait;
  ///    public Asynchronous.Channel&lt;R&gt; Response(int i) {
  ///      return Responses[i];
  ///    }
  ///    public JoinMany(int n) {
  ///      Join j = Join.Create(n + 1);
  ///      j.Initialize(out Responses, n);
  ///      j.Initialize(out Wait);
  ///      j.When(Wait).And(Responses).Do(delegate(R[] results)
  ///      {
  ///        return results;
  ///      });
  ///    }
  ///  }
  /// </code>
  /// </example>
  public abstract partial class Join {


    internal readonly int mSize;
    internal int mChannelCount;

    /// <summary>
    /// The maximum number of channels 
    /// supported by this <see cref="Join"/> instance.
    /// </summary>
    /// <value>
    /// <para>
    /// The value is constant and determined by the initial call to <see cref="Create()"/> used to obtain 
    /// this <see cref="Join"/> instance.
    /// </para>
    /// </value>
    public int Size {
      get { return mSize; }
    }

    /// <summary>
    /// The current number of channels initialized on, and owned by, this <see cref="Join"/>  instance.
    /// <para>
    /// <c>Count</c> is increased by calls to <c>Initialize</c> and bounded by <see cref="Size"/>.
    /// </para>
    /// </summary>
    public int Count {
      get { return mChannelCount; }
    }

    //BEWARE: forall i, Count <= i < Chans.Length) Chans[i] = null;
    internal readonly IChannelTarget[] Chans;

    internal int NextChannel(IChannelTarget chan) {
      int channelNum = mChannelCount;
      if (mChannelCount == Int32.MaxValue) JoinException.SizeExceeded();
      if (mSize >= 0 && channelNum >= mSize) JoinException.SizeExceeded();
      Chans[mChannelCount++]=chan;
      return channelNum;
    }

      /*
    public void Init(out Asynchronous.Channel channel) { Initialize(out channel); }
    public void Init<A>(out Asynchronous.Channel<A> channel)  { Initialize(out channel); }
    public void Init<R>(out Synchronous<R>.Channel channel)  { Initialize(out channel); }
    public void Init<R, A>(out Synchronous<R>.Channel<A> channel) { Initialize(out channel); }
    public void Init(out Synchronous.Channel channel) { Initialize(out channel); }
    public void Init<A>(out Synchronous.Channel<A> channel) { Initialize(out channel); }
    public void Init(out Asynchronous.Channel[] channels, int length) { Initialize(out channels, length); }
    public void Init<A>(out Asynchronous.Channel<A>[] channels, int length) { Initialize(out channels, length); }
    public void Init(out Synchronous.Channel[] channels, int length) { Initialize(out channels, length); }
    public void Init<A>(out Synchronous.Channel<A>[] channels, int length)  { Initialize(out channels, length); }
    public void Init<R,A>(out Synchronous<R>.Channel<A>[] channels, int length)  { Initialize(out channels, length); }
    public void Init<R>(out Synchronous<R>.Channel[] channels, int length) { Initialize(out channels, length); }
      */
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new asynchronous channel declared on this  <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The location of <paramref name = "channel"/> will contain an asynchronous channel that takes no arguments and
    /// returns <c>void</c> immediately.</para>
    /// <para>Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize(out Asynchronous.Channel channel);
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new asynchronous channel declared on this  <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The location of <paramref name = "channel"/> will contain an asynchronous channel that takes a single argument of type <c>A</c> and
    /// returns <c>void</c> immediately.</para>
    /// <para>Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <typeparam name="A">is the argument type of the asynchronous channel. </typeparam>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize<A>(out Asynchronous.Channel<A> channel);
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new synchronous channel declared on this <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The location of <paramref name = "channel"/> will contain a synchronous channel that returns a result of type <c>R</c> and takes no argument.</para>
    /// <para>A call to <paramref name = "channel"/> will block until or unless some pattern involving <paramref name = "channel"/> has been enabled.</para>
    /// <para>Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <typeparam name="R">is the return type of the synchronous channel. </typeparam>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize<R>(out Synchronous<R>.Channel channel);
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new synchronous channel declared on this <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The location of <paramref name = "channel"/> will contain a synchronous channel that returns a result of type <c>R</c> and takes a single argument of type <c>A</c></para>
    /// <para> A call to <paramref name = "channel"/> will block until or unless some pattern involving <paramref name = "channel"/> has been enabled.</para>
    /// <para> Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <typeparam name="R">is the return type of the synchronous channel. </typeparam>
    /// <typeparam name="A">is the argument type of the synchronous channel. </typeparam>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize<R, A>(out Synchronous<R>.Channel<A> channel);
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new synchronous channel declared on this <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The location of <paramref name = "channel"/> will contain a synchronous channel that returns void and takes no arguments. </para>
    /// <para>A call to <paramref name = "channel"/> will block until or unless some pattern involving <paramref name = "channel"/> has been enabled.</para>
    /// <para>Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize(out Synchronous.Channel channel);
    /// <summary>Initialize the location of <paramref name = "channel"/> with a new synchronous channel declared on this <see cref="Join"/> instance.
    /// </summary>
    /// <remarks> 
    /// <para>The location of <paramref name = "channel"/> will contain a synchronous channel that returns void and takes a single argument of type <c>A</c>. </para>
    /// <para>A call to <paramref name = "channel"/> will block until or unless some pattern involving <paramref name = "channel"/> has been enabled.</para>
    /// <para>Increments <see cref="Count"/>.</para>
    /// </remarks>
    /// <typeparam name="A">is the argument type of the synchronous channel. </typeparam>
    /// <param name="channel">is typically a field of some enclosing class, a private local variable or the location of an array element.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public abstract void Initialize<A>(out Synchronous.Channel<A> channel);
    /// <summary>Initialize the location of <paramref name = "channels"/> with an array of <paramref name = "length"/> new asynchronous channels declared on this  <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>Every entry of <paramref name = "channels"/> will contain a new asynchronous channel that takes no arguments and
    /// returns <c>void</c> immediately.</para>
    /// <para>Increases <see cref="Count"/> by <paramref name = "length"/>.</para>
    /// </remarks>
    /// <param name="channels">is typically a field of some enclosing class or a private local variable.</param> 
    /// <param name="length">is the length of the array and the number of distinct asynchronous channels.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public void Initialize(out Asynchronous.Channel[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Asynchronous.Channel[] a = new Asynchronous.Channel[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }
    /// <summary>Initialize the location of <paramref name = "channels"/> with an array of <paramref name = "length"/> new asynchronous channels declared on this  <see cref="Join"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>Every entry of <paramref name = "channels"/> will contain a new synchronous channel that takes a single argument of type <c>A</c> and
    /// returns <c>void</c> immediately.</para>
    /// <para>Increases <see cref="Count"/> by <paramref name = "length"/>.</para>
    /// </remarks>
    /// <typeparam name="A"> is the argument type of each channel in the array.</typeparam>
    /// <param name="channels">is typically a field of some enclosing class or a private local variable.</param> 
    /// <param name="length">is the length of the array and the number of distinct asynchronous channels.</param> 
    /// <exception cref = "JoinException"> when <see cref="Count"/> would exceed <see cref="Size"/>. </exception>
    public void Initialize<A>(out Asynchronous.Channel<A>[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Asynchronous.Channel<A>[] a = new Asynchronous.Channel<A>[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }

    public void Initialize(out Synchronous.Channel[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Synchronous.Channel[] a = new Synchronous.Channel[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }

    public void Initialize<A>(out Synchronous.Channel<A>[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Synchronous.Channel<A>[] a = new Synchronous.Channel<A>[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }
    
  public void Initialize<R>(out Synchronous<R>.Channel[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Synchronous<R>.Channel[] a = new Synchronous<R>.Channel[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }

    public void Initialize<R,A>(out Synchronous<R>.Channel<A>[] channels, int length) {
#if LOCKING
      lock (this)
#endif
 {
        if (Int32.MaxValue - Count < length) JoinException.SizeExceeded();
        if (Size > 0 && Size - Count < length) JoinException.SizeExceeded();
        Synchronous<R>.Channel<A>[] a = new Synchronous<R>.Channel<A>[length];
        for (int i = 0; i < length; i++) {
          this.Initialize(out a[i]);
        }
        channels = a;
      }
    }  
    
    /// <summary>
    /// Commences the declaration of a chord on synchronous channel <paramref name="channel"/> that, when enabled,
    /// awakens all threads waiting on an invocation of <paramref name="channel"/>, executing the continuation of the pattern in one of the threads.
    /// The continuation receives the thread's argument to <paramref name="channel"/> as its first parameter of type <typeparamref name="A"/>
    /// and returns a value of type <c>R</c>, the return type of the channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(delegate(A a,P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Func&lt;A, P1, ..., Pm, R&gt;</c> that 
    /// returns a value of type <c>R</c>, the return type of <paramref name="channel"/>,
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined channels
    /// as delegate parameters <c> A a,P1 p1, ... Pm pm</c>.
    /// The first parameter of <c>d</c> has type <c>A</c>, the argument type of <paramref name="channel"/>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or  
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns>   
    /// <typeparam name="R">is the return type of both <paramref name="channel"/> and any delegate <c>d</c> used to complete the chord.</typeparam>
    /// <typeparam name="A">is the argument type of both <paramref name="channel"/> and the type of the first argument of any delegate <c>d</c> used to complete the chord.</typeparam>
    /// <param name="channel">is a synchronous channel declared on this instance.</param>
    /// <exception cref="JoinException">thrown when <paramref name="channel"/> is null or foreign to this join instance.
    /// </exception>
    public FuncChord<ArgCounts.S<ArgCounts.Z>, A, R> When<A, R>(Synchronous<R>.Channel<A> channel) { return ChordFactory.FromChannel(this, channel); }
    /// <summary>
    /// Commences the declaration of a synchronous chord that waits for one invocation of each synchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in the thread of one unspecified synchronous caller.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/> 
    /// and receives those invocation arguments as its first parameter of type <typeparamref name ="A"/><c>[]</c>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channels"/> <c>P1</c> stand for the array type <typeparamref name="A"/><c>[]</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Func&lt;P1,...,Pm,R&gt;</c> that 
    /// returns the same value of type <c>R</c> to each synchronous caller 
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// Note that <c>p1</c> is an array of <c>A</c> values: entry <c>p1[i]</c> will contain the value consumed from corresponding channel <paramref name="channels"/><c>[i]</c>.
    /// </para>
    /// </remarks>
    /// <returns> An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns> 
    /// <typeparam name="A"> is the argument type of each channel in <paramref name="channels"/> and the array element type of the first continuation argument.</typeparam>
    /// <typeparam name="R"> is the return type of each channel in <paramref name="channels"/> and the pattern's continuation.</typeparam>
    /// <param name="channels">is an array of asynchronous channels declared on this instance.</param>
    public FuncChord<ArgCounts.S<ArgCounts.Z>, A[], R> When<A, R>(Synchronous<R>.Channel<A>[]channels) { return ChordFactory.FromChannels(this, channels); }

    /// <summary>
    /// Commences the declaration of a chord on synchronous channel <paramref name="channel"/> that, when enabled,
    /// awakens all threads waiting on an invocation of <paramref name="channel"/>, executing the continuation of the pattern in one of the threads.
    /// The continuation returns a value of type <c>R</c>, the return type of the channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Func&lt;P1, ..., Pm, R&gt;</c> that 
    /// returns a value of type <c>R</c>, the return type of <paramref name="channel"/>,
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c> P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns> An incomplete chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or  
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is delegate determining the body of the pattern.
    /// </returns>   
    /// <typeparam name="R">is the return type of both <paramref name="channel"/> and any delegate <c>d</c> used to complete the chord.</typeparam>
    /// <param name="channel">is a synchronous channel declared on this instance.</param>
    /// <exception cref="JoinException">thrown when <paramref name="channel"/> is null or foreign to this join instance.
    /// </exception>
    public FuncChord<ArgCounts.Z,Unit,R> When<R>(Synchronous<R>.Channel channel) { return ChordFactory.FromChannel(this, channel); }

    /// <summary>
    /// Commences the declaration of a synchronous chord that waits for one invocation of each synchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in the thread of one unspecified synchronous caller.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Func&lt;P1,...,Pm,R&gt;</c> that 
    /// returns the same value of type <c>R</c> to each synchronous caller 
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns> An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns> 
    /// <typeparam name="R"> is the return type of each channel in <paramref name="channels"/> and the pattern's continuation.</typeparam>
    /// <param name="channels">is an array of asynchronous channels declared on this instance.</param>
    public FuncChord<ArgCounts.Z, Unit, R> When<R>(Synchronous<R>.Channel[] channels) { return ChordFactory.FromChannels(this, channels); }

#if MONO
    // MONO WORKAROUND
    public FuncChord<ArgCounts.Z, Unit, R> WhenVoidArg<R>(Synchronous<R>.Channel channel) { return ChordFactory.FromChannel(this, channel); }
#endif 
    /// <summary>
    /// Commences the declaration of a chord on synchronous channel <paramref name="channel"/> that, when enabled,
    /// awakens all threads waiting on an invocation of <paramref name="channel"/>, executing the continuation of the pattern in one of the threads.
    /// The continuation receives the thread's argument  to <paramref name="channel"/> as its first parameter of type <typeparamref name="A"/>
    /// and returns <c>void</c>, the return type of the channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(delegate(A a,P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;A, P1, ..., Pm&gt;</c> that 
    /// returns void, the return type of <paramref name="channel"/>,
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c> A a,P1 p1, ... Pm pm</c>.
    /// The first parameter of <c>d</c> has type <c>A</c>, the argument type of <paramref name="channel"/>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns>   
    /// <typeparam name="A">is the argument type of both <paramref name="channel"/> and the type of the first argument of any delegate <c>d</c> used to complete the chord.</typeparam>
    /// <param name="channel">is a synchronous channel declared on this instance.</param>
    /// <exception cref="JoinException">thrown when <paramref name="channel"/> is null or foreign to this join instance.
    /// </exception>
    public ActionChord<ArgCounts.S<ArgCounts.Z>, A> When<A>(Synchronous.Channel<A> channel) { return ChordFactory.FromChannel(this, channel); }
    /// <summary>
    /// Commences the declaration of a synchronous chord that waits for one invocation of each synchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in the thread of one unspecified synchronous caller.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/> 
    /// and receives those invocation arguments as its first parameter of type <typeparamref name ="A"/><c>[]</c>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channels"/> <c>P1</c> stand for the array type <typeparamref name="A"/><c>[]</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1,...,Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// Note that <c>p1</c> is an array of <c>A</c> values: entry <c>p1[i]</c> will contain the value consumed from corresponding channel <paramref name="channels"/><c>[i]</c>.
    /// </para>
    /// </remarks>
    /// <returns> An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns> 
    /// <typeparam name="A"> is the argument type of each channel in <paramref name="channels"/> and the array element type of the first continuation argument.</typeparam>
    /// <param name="channels">is an array of asynchronous channels declared on this instance.</param>
    public ActionChord<ArgCounts.S<ArgCounts.Z>, A[]> When<A>(Synchronous.Channel<A>[] channels) { return ChordFactory.FromChannels(this, channels); }

    /// <summary>
    /// Commences the declaration of a chord on synchronous channel <paramref name="channel"/> that, when enabled,
    /// awakens all threads waiting on an invocation of <paramref name="channel"/>, executing the continuation of the pattern in one of the threads.
    /// The continuation returns <c>void</c>, the return type of the channel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(channel).And(a1) ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1, ..., Pm&gt;</c> that 
    /// returns void, the return type of <paramref name="channel"/>,
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c> P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns>   
    /// <param name="channel">is a synchronous channel declared on this instance.</param>
    /// <exception cref="JoinException">thrown when <paramref name="channel"/> is null or foreign to this join instance.
    /// </exception>
    public ActionChord<ArgCounts.Z,Unit> When(Synchronous.Channel channel) { return ChordFactory.FromChannel(this, channel); }



    /// <summary>
    /// Commences the declaration of a synchronous chord that waits for one invocation of each synchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in the thread of one unspecified synchronous caller.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channels"/>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1, ..., Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete synchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is delegate determining the body of the pattern.
    /// </returns>   
    /// <param name="channels">is an array of synchronous channels declared on this instance.</param>
    public ActionChord<ArgCounts.Z, Unit> When(Synchronous.Channel[] channels) { return ChordFactory.FromChannels(this, channels); }

#if MONO
    // MONO WORKAROUND
    public ActionChord<ArgCounts.Z, Unit> WhenSyncVoidArr(Synchronous.Channel[] chs) { return ChordFactory.FromChannels(this, chs); }
#endif
    /// <summary>
    /// Commences the declaration of an asynchronous chord on asynchronous channel <paramref name="channel"/> that, when enabled,
    /// executes the continuation of the pattern in a new thread.
    /// The continuation consumes one pending invocation of <paramref name="channel"/>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channel"/> : </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1, ..., Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or  
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns>   
    /// <param name="channel">is an asynchronous channel declared on this instance.</param>
    public ActionChord<ArgCounts.Z, Unit> When(Asynchronous.Channel channel) { return ChordFactory.FromChannel(this, channel); }

    /// <summary>
    /// Commences the declaration of an asynchronous chord on asynchronous channel <paramref name="channel"/> that, when enabled,
    /// executes the continuation of the pattern in a new thread.
    /// The continuation consumes one pending invocation of <paramref name="channel"/>
    /// and receives that invocation's argument as its first parameter of type <typeparamref name="A"/>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channel"/> and <c>P1</c> stand for <typeparamref name="A"/>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1, ..., Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns>  
    /// <typeparam name="A"> is the argument type of the channel and the type of the first continuation argument.</typeparam>
    /// <param name="channel">is an asynchronous channel declared on this instance.</param>
    public ActionChord<ArgCounts.S<ArgCounts.Z>, A> When<A>(Asynchronous.Channel<A> channel) { return ChordFactory.FromChannel(this, channel); }

    /// <summary>
    /// Commences the declaration of an asynchronous chord that waits for one invocation of each asynchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in a new thread.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/> 
    /// and receives those invocation arguments as its first parameter of type <typeparamref name ="A"/><c>[]</c>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channels"/> <c>P1</c> stand for the array type <typeparamref name="A"/><c>[]</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1,...,Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// Note that <c>p1</c> is an array of <c>P</c> values: entry <c>p1[i]</c> will contain the value consumed from corresponding channel <paramref name="channels"/><c>[i]</c>.
    /// </para>
    /// </remarks>
    /// <returns> An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is a delegate determining the body of the pattern.
    /// </returns> 
    /// <typeparam name="A"> is the argument type of each channel in <paramref name="channels"/> and the array element type of the first continuation argument.</typeparam>
    /// <param name="channels">is an array of asynchronous channels declared on this instance.</param>
    public ActionChord<ArgCounts.S<ArgCounts.Z>, A[]> When<A>(Asynchronous.Channel<A>[] channels) { return ChordFactory.FromChannels(this, channels); }

    /// <summary>
    /// Commences the declaration of an asynchronous chord that waits for one invocation of each asynchronous channel in the array <paramref name="channels"/>. 
    /// When the pattern is enabled, the continuation of the pattern executes in a new thread.
    /// The continuation consumes one invocation of each channel in <paramref name="channels"/>.
    /// The continuation returns <c>void</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The incomplete chord may be refined by sequentially invoking zero or more overloads of instance method <c>And(ai)</c>, where 
    /// <c>ai</c> is an asynchronous channel or array of asynchronous channels,
    /// and finally completed by invoking <c>Do(d)</c> once, where <c>d</c> is the body of the pattern. 
    /// For example, letting <c>a1</c> stand for <paramref name="channels"/>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(d);</code>
    /// </para>
    /// <para>Alternatively, using an anonymous delegate for <c>d</c>: </para>
    /// <para>
    ///     <code>join.When(a1). ... .And(an).Do(delegate(P1 p1, ... Pm pm){...});</code>
    /// </para>
    /// <para>
    /// The body <c>d</c> is a delegate of type <c>Action&lt;P1, ..., Pm&gt;</c> that 
    /// returns void
    /// and receives <c>m</c>&lt;=<c>n</c> arguments of the joined invocations 
    /// as delegate parameters <c>P1 p1, ... Pm pm</c>.
    /// </para>
    /// </remarks>
    /// <returns>An incomplete asynchronous chord that may either be refined by invoking an overload of instance method <c>And(a)</c> or 
    /// completed by invoking <c>Do(d)</c> where <c>d</c> is delegate determining the body of the pattern.
    /// </returns>   
    /// <param name="channels">is an array of asynchronous channels declared on this instance.</param>
    public ActionChord<ArgCounts.Z, Unit> When(Asynchronous.Channel[] channels) { return ChordFactory.FromChannels(this, channels); }

  
    internal abstract void Register(Chord cho);

    internal Join(int size) {
      mSize = size;
      Chans = new IChannelTarget[size];
    }
  }

  internal abstract partial class Join<IntSet> : Join where IntSet : IIntSet<IntSet> {
   

    public Join(int size) : base(size) {
     
    }

    internal string IntSetToString(IntSet m) {
      string s = "{";
      for (int i = 0; i < Count; i++) {
        if (m.Contains(i)) { s += i; s += " "; }
      }
      return s + "}";
    }
  }
}

