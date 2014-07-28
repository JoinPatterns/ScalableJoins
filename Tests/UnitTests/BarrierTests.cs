using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.Joins;
using System.Threading;


namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class BarrierTests
    {
        public BarrierTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public void TestBarrier<F>() where F : struct, Join.IJoinFactory
        {
            for (int n = 1; n < 50; n++)
            {
                var N = n;
                Synchronous<int>.Channel<int>[] chans;
                var barrier = Join.Create<F>(n);
                barrier.Initialize(out chans, n);
                var k = 0;
                barrier.When(chans).Do(a =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        Assert.AreEqual(a[i], k*i);
                    };
                    return k++;
                });
                var threads = new Thread[n];
                for (int i = 0; i < n; i++)
                {
                    var I = i;
                    threads[i] = new Thread(_ =>
                    {
                        for (var j = 0; j < 100; j++)
                        {
                            Assert.AreEqual(j, chans[I](j*I));
                        }
                    });
                }

                foreach (var thread in threads)
                {
                    thread.Start();
                }
                foreach (var thread in threads)
                {
                    thread.Join();
                }
                                     
                }
               

        }


        class Exception : System.Exception
        {
            public readonly int value;
            public Exception(int value)
            {
                this.value = value;
            }
        }

        // like TestBarrier but throws an exception every second turn.
        public void TestBarrierExn<F>() where F : struct, Join.IJoinFactory
        {
            for (int n = 1; n < 50; n++)
            {
                var N = n;
                Synchronous<int>.Channel<int>[] chans;
                var barrier = Join.Create<F>(n);
                barrier.Initialize(out chans, n);
                var k = 0;
                barrier.When(chans).Do(a =>
                {
                    for (int i = 0; i < n; i++)
                    {
                        Assert.AreEqual(a[i], k * i);
                    };
                    var doThrow = k % 2 == 0;
                    if (doThrow)  
                     throw new Exception(k++);
                    else return k++;
                });
                var threads = new Thread[n];
                for (int i = 0; i < n; i++)
                {
                    var I = i;
                    threads[i] = new Thread(_ =>
                    {
                        for (var j = 0; j < 10; j++)
                        {
                            var doCatch = (j % 2 == 0);
                            if (doCatch)
                            {
                                try
                                {
                                    chans[I](j * I);
                                    Assert.Fail();
                                }
                                catch (Exception e)
                                {
                                    Assert.AreEqual(e.value, j);
                                }
                            }
                            else
                            {
                                try
                                {
                                    Assert.AreEqual(j, chans[I](j * I));
                                }
                                catch (Exception e)
                                {
                                    Assert.Fail();
                                }

                               
                            }

                        }
                    });
                }

                foreach (var thread in threads)
                {
                    thread.Start();
                }
                foreach (var thread in threads)
                {
                    thread.Join();
                }

            }


        }
   
        [TestMethod]
        public void TestBarrierLockBased()
        {
            TestBarrier<Join.LockBased>();
        }

        [TestMethod]
        public void TestBarrierScalable()
        {
            TestBarrier<Join.Scalable>();
        }

        [TestMethod]
        public void TestBarrierExnLockBased()
        {
            TestBarrierExn<Join.LockBased>();
        }

        [TestMethod]
        public void TestBarrierExnScalable()
        {
            TestBarrierExn<Join.Scalable>();
        }
    }
}
