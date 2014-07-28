using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
//using Microsoft.Research.Joins.JoinPatterns;
using Microsoft.Research.Joins;


namespace Sanity {

  class OverflowTests {

    static object Initialize(Join j, int i) {
      switch (i % 8) {
        case 0 : {
           Asynchronous.Channel c;
           j.Initialize(out c);
           return c;
        }
        case 1 : {
           Asynchronous.Channel<string> c;
           j.Initialize(out c);
           return c;
        }
        case 2 : {
           Synchronous<string>.Channel<string> c;
           j.Initialize(out c);
           return c;
        }
        case 3 : {
           Synchronous<string>.Channel c;
           j.Initialize(out c);
           return c;
        }
        case 4 : {
           Synchronous.Channel<string> c;
           j.Initialize(out c);
           return c;
        }
        case 5 : {
           Synchronous.Channel c;
           j.Initialize(out c);
           return c;
        }
        case 6: {
          Asynchronous.Channel[] c;
          j.Initialize(out c, 1);
          return c;
        }
        case 7: {
          Asynchronous.Channel<string>[] c;
          j.Initialize(out c, 1);
          return c;
        }
        default : 
          Debug.Assert(false);
          return null;
      }
    }

    static void TestInitialize(int size) {
      Join j = Join.Create(size);
      // fill up j
      for (int i = 0; i < size; i++) {
        Debug.Assert(j.Size == size);
        Debug.Assert(j.Count == i);
        Debug.Assert(Initialize(j,i) != null);
      }
      // overflow j
      for (int i = 0; i<8*2 ; i++)
      { string msg = null;
        object channel = null;
        try {
          channel = Initialize(j,i);
          Debug.Assert(false);
        }
        catch (JoinException e) {
          if (msg == null) { // check we get consistent messages
            msg = e.Message; 
            Debug.Assert(msg != null); 
           } else
           Debug.Assert(e.Message == msg); 
         };
        Debug.Assert(channel == null);
        Debug.Assert(j.Count == size);
        Debug.Assert(j.Size == size);
      }
    }

    public static void Test() {
      for (int i = 1; i < 8; i = 2 * i) {
        TestInitialize(i - 1);
        TestInitialize(i);
        TestInitialize(i + 1);
      }
    }
  }

  class NullChannels {

    // Test When(null) raises JoinException (consistently)
    public static void TestWhenNull() {
      string msg = null;
      Join j = Join.Create();
      try {
        j.When((Asynchronous.Channel)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
      try {
        j.When((Asynchronous.Channel<string>)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Synchronous<string>.Channel<string>)null).Do(delegate { return ""; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Synchronous<string>.Channel)null).Do(delegate { return ""; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Synchronous.Channel<string>)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Synchronous.Channel)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      string msg2 = null;
      try {
        j.When((Asynchronous.Channel[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg2 = e.Message;
        Debug.Assert(msg2 != null);
      }
      try {
        j.When((Asynchronous.Channel<string>[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg2 == e.Message);
      }

      try {
        j.When((Asynchronous.Channel[])new Asynchronous.Channel[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Asynchronous.Channel<string>[])new Asynchronous.Channel<string>[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

    }

    // Test And(null) raises JoinException (consistently)
    public static void TestAndNull() {
      string msg = null;
      Join j = Join.Create();
      Synchronous.Channel chan;
      j.Initialize(out chan);
      var jp = j.When(chan);
      try {
        jp.And((Asynchronous.Channel)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
      try {
        jp.And((Asynchronous.Channel<string>)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
    
      string msg2 = null;
      try {
        jp.And((Asynchronous.Channel[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg2 = e.Message;
        Debug.Assert(msg2 != null);
      }
      try {
        jp.And((Asynchronous.Channel<string>[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg2 == e.Message);
      }

      try {
        jp.And((Asynchronous.Channel[])new Asynchronous.Channel[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        jp.And((Asynchronous.Channel<string>[])new Asynchronous.Channel<string>[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

    }
    // Test And(null) raises JoinException (consistently)
    public static void TestAndAndNull() {
      string msg = null;
      Join j = Join.Create();
      Synchronous.Channel<string> chan1;
      Asynchronous.Channel<string> chan2;
      j.Initialize(out chan1);
      j.Initialize(out chan2);
      var jp = j.When(chan1).And(chan2);
      try {
        jp.And((Asynchronous.Channel)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
      try {
        jp.And((Asynchronous.Channel<string>)null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      string msg2 = null;
      try {
        jp.And((Asynchronous.Channel[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg2 = e.Message;
        Debug.Assert(msg2 != null);
      }
      try {
        jp.And((Asynchronous.Channel<string>[])null).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg2 == e.Message);
      }

      try {
        jp.And((Asynchronous.Channel[])new Asynchronous.Channel[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        jp.And((Asynchronous.Channel<string>[])new Asynchronous.Channel<string>[] { null }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

    }

    // Test When(foreignChannel) raises JoinException (consistently)
    public static void TestWhenForeign() {
      string msg = null;
      Join j = Join.Create();
      Join f = Join.Create();
      Asynchronous.Channel async; 
      Asynchronous.Channel<string> asyncString;
      Synchronous<string>.Channel syncString;
      Synchronous<string>.Channel<string> syncStringString;
      Synchronous.Channel<string> syncVoidString;
      Synchronous.Channel syncVoid;

      f.Initialize(out async);
      f.Initialize(out asyncString);
      f.Initialize(out syncString);
      f.Initialize(out syncStringString);
      f.Initialize(out syncVoidString);
      f.Initialize(out syncVoid);

      try {
        j.When(async).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
      try {
        j.When(asyncString).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When(syncStringString).Do(delegate { return ""; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When(syncString).Do(delegate { return ""; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When(syncVoidString).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When(syncVoid).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }


      try {
        j.When((Asynchronous.Channel[])new Asynchronous.Channel[] { async }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        j.When((Asynchronous.Channel<string>[])new Asynchronous.Channel<string>[] { asyncString }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

    }

    // Test When(foreignChannel) raises JoinException (consistently)
    public static void TestAndForeign() {
      string msg = null;
      Join j = Join.Create();
      
 
      Synchronous.Channel chan;
      j.Initialize(out chan);
      var jp = j.When(chan); 

      Join f = Join.Create();
      Asynchronous.Channel async;
      Asynchronous.Channel<string> asyncString;
      Synchronous<string>.Channel syncString;
      Synchronous<string>.Channel<string> syncStringString;
      Synchronous.Channel<string> syncVoidString;
      Synchronous.Channel syncVoid;

      f.Initialize(out async);
      f.Initialize(out asyncString);
      f.Initialize(out syncString);
      f.Initialize(out syncStringString);
      f.Initialize(out syncVoidString);
      f.Initialize(out syncVoid);

      try {
        jp.And(async).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
      try {
        jp.And(asyncString).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
     
      try {
        jp.And((Asynchronous.Channel[])new Asynchronous.Channel[] { async }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      try {
        jp.And((Asynchronous.Channel<string>[])new Asynchronous.Channel<string>[] { asyncString }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

    }

    // Test Do(null) raises JoinException (consistently)
    public static void TestDoNull() {
      string msg = null;
      Join j = Join.Create();
      Synchronous.Channel syncVoid;
      Synchronous<string>.Channel syncString;

      j.Initialize(out syncVoid);
      j.Initialize(out syncString);

      try {
        j.When(syncVoid).Do(null);
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };

      try {
        j.When(syncString).Do(null);
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };
     

    }


     // Test repeated channel raises JoinException (consistently)
    public static void TestRepeatedChannels() {
      string msg = null;
      Join j = Join.Create();
      Asynchronous.Channel async1;
      Asynchronous.Channel async2;
      Asynchronous.Channel<string> asyncString1;
      Asynchronous.Channel<string> asyncString2;
      j.Initialize(out async1);
      j.Initialize(out async2);
      j.Initialize(out asyncString1);
      j.Initialize(out asyncString2);

      try {
        j.When(async1).And(async1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };

      try {
        j.When(asyncString1).And(asyncString1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel[] { async1, async2, async1 }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel[] { async1, async2}).And(async1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel<string>[] {asyncString1,asyncString2,asyncString1}).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel<string>[] { asyncString1, asyncString2}).And(asyncString1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
    }


    // Test redundant pattern raises JoinException (consistently)
    public static void TestRedundantPatterns() {
      string msg = null;
      Join j = Join.Create();
      Asynchronous.Channel async1;
      Asynchronous.Channel async2;
      Asynchronous.Channel async3;
      Asynchronous.Channel<string> asyncString1;
      Asynchronous.Channel<string> asyncString2;
      Asynchronous.Channel<string> asyncString3;
      Synchronous<String>.Channel<String> syncStringString;
      Synchronous<String>.Channel syncString;
      Synchronous.Channel<String> syncVoidString;
      Synchronous.Channel syncVoid;
      
      j.Initialize(out async1);
      j.Initialize(out async2);
      j.Initialize(out async3);

      j.Initialize(out asyncString1);
      j.Initialize(out asyncString2);
      j.Initialize(out asyncString3);

      j.Initialize(out syncStringString);
      j.Initialize(out syncString);
      j.Initialize(out syncVoidString);
      j.Initialize(out syncVoid);
    
      j.When(async1).Do(delegate { });   
      try {
        j.When(async1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };

      try {
        j.When(async1).And(async2).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(async2).And(async3).Do(delegate { });

      try {
        j.When(async2).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel[] { async2}).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(asyncString1).Do(delegate { });

      try {
        j.When(asyncString1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };

      try {
        j.When(asyncString1).And(asyncString2).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(asyncString2).And(asyncString3).Do(delegate { });

      try {
        j.When(asyncString2).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      try {
        j.When(new Asynchronous.Channel<string>[] { asyncString2 }).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(syncStringString).Do(delegate { return null; });
      try {
        j.When(syncStringString).And(async1).Do(delegate { return null; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(syncString).Do(delegate { return null; });
      try {
        j.When(syncString).And(async1).Do(delegate { return null; });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(syncVoidString).Do(delegate { });
      try {
        j.When(syncVoidString).And(async1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }

      j.When(syncVoid).Do(delegate { });
      try {
        j.When(syncVoid).And(async1).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      
    }

    // Test empy pattern raises JoinException (consistently)
    public static void TestEmptyPatterns() {
      string msg = null;
      Join j = Join.Create();
      Asynchronous.Channel[] asyncs;
      Asynchronous.Channel<string>[] asyncStrings;
      Asynchronous.Channel async1;
      Asynchronous.Channel async2;

      j.Initialize(out asyncs, 0);
      j.Initialize(out asyncStrings, 0);
      j.Initialize(out async1);
      j.Initialize(out async2);

      try {
        j.When(asyncs).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        msg = e.Message;
        Debug.Assert(msg != null);
      };

      try {
        j.When(asyncStrings).Do(delegate { });
        Debug.Assert(false);
      }
      catch (JoinException e) {
        Debug.Assert(msg == e.Message);
      }
      
      // these are legal, because the entire pattern is still non-empty
      try {
        j.When(asyncs).And(async1).Do(delegate { });
      }
      catch (JoinException) {
        Debug.Assert(false);
      }

      try {
        j.When(asyncStrings).And(async2).Do(delegate { });
      }
      catch (JoinException) {
        Debug.Assert(false);
      }


    }



    public static void Test() {
      TestWhenNull();
      TestAndNull();
      TestAndAndNull();
      // we could go on, but let's not and say we did.
      TestWhenForeign();
      TestAndForeign();
      TestDoNull();
      TestRepeatedChannels();
      //TestRedundantPatterns();
      TestEmptyPatterns();

    }
  }

  class Program {

    static void Main() {
      OverflowTests.Test();
      NullChannels.Test();
      Console.WriteLine("Done");
      Console.ReadLine();
    }



  }
}
