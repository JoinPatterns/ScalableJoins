using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if true
namespace Microsoft.Research.Joins
{
    public static class Extensions {

    /*
    public static Synchronous.Send<T> Send<T>(this Synchronous<T>.Channel This)
    {
        return new Send<T>(This);
    }
    */
    public static Synchronous<R>.Send<T> Send<R, T>(this Synchronous<R>.Channel<T> This, T t)
    {
        return new Synchronous<R>.Send<T>(This, t);
    }
      
}
    public static partial class Synchronous<R> {

        
        // naive because it doesn't take advantage of the fastpath.
        // Also needs to be a class so that we can capture .result in the delegate.
        public struct Send
        {
            readonly TaskCompletionSource<R> tcs;
            readonly Channel chan;
            public bool IsCompleted { get { return false; } }
            public void OnCompleted(Action Resume)
            {
                this.tcs.Task.ContinueWith(t => { Resume(); });
                var chan = this.chan;
                var tcs = this.tcs;
                chan.BeginInvoke(ar =>
                {
                    try
                    {
                        var t = chan.EndInvoke(ar);
                        tcs.SetResult(t);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    };
                },
                null);
            }
            public Send(Channel chan)
            {
                this.tcs = new TaskCompletionSource<R>();
                this.chan = chan;

            }
            public R GetResult() { return tcs.Task.Result; }

            public Send GetAwaiter()
            {
                return this;
            }
        }


        public class Send<T> : System.Runtime.CompilerServices.INotifyCompletion //huh?
        {
            readonly T t;
            readonly TaskCompletionSource<R> tcs;
            readonly Channel<T> chan;
            public bool IsCompleted { get { return false; } }
            public void OnCompleted(Action Resume)
            {
                tcs.Task.ContinueWith(_ => { Resume(); });

                chan.BeginInvoke(this.t, ar =>
                {
                    try
                    {
                        var r = chan.EndInvoke(ar);
                        tcs.SetResult(r);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    };
                },
                null);
            }
            public Send(Synchronous<R>.Channel<T> chan, T t)
            {
                this.t = t;
                this.tcs = new TaskCompletionSource<R>();
                this.chan = chan;

            }
            public R GetResult() { return tcs.Task.Result; }

            public Send<T> GetAwaiter()
            {
                return this;
            }
        }



       
            
    
    
    
    
    }

    public static partial class Synchronous
    {


        // naive because it doesn't take advantage of the fastpath.
        // Also needs to be a class so that we can capture .result in the delegate.
        public struct Send
        {
            readonly TaskCompletionSource<Unit> tcs;
            readonly Channel chan;
            public bool IsCompleted { get { return false; } }
            public void OnCompleted(Action Resume)
            {
                this.tcs.Task.ContinueWith(t => { Resume(); });
                var chan = this.chan;
                var tcs = this.tcs;
                chan.BeginInvoke(ar =>
                {
                    try
                    {
                        chan.EndInvoke(ar);
                        tcs.SetResult(Unit.Null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    };
                },
                null);
            }
            public Send(Channel chan)
            {
                this.tcs = new TaskCompletionSource<Unit>();
                this.chan = chan;

            }
            public void GetResult() { var one = tcs.Task.Result;  }

            public Send GetAwaiter()
            {
                return this;
            }
        }


        public class Send<T> : System.Runtime.CompilerServices.INotifyCompletion //huh?
        {
            readonly T t;
            readonly TaskCompletionSource<Unit> tcs;
            readonly Channel<T> chan;
            public bool IsCompleted { get { return false; } }
            public void OnCompleted(Action Resume)
            {
                tcs.Task.ContinueWith(_ => { Resume(); });

                chan.BeginInvoke(this.t, ar =>
                {
                    try
                    {
                        chan.EndInvoke(ar);
                        tcs.SetResult(Unit.Null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    };
                },
                null);
            }
            public Send(Synchronous.Channel<T> chan, T t)
            {
                this.t = t;
                this.tcs = new TaskCompletionSource<Unit>();
                this.chan = chan;

            }
            public void GetResult() { var one = tcs.Task.Result; }

            public Send<T> GetAwaiter()
            {
                return this;
            }
        }









    }
}
#endif