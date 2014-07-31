using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Joins;

public class Slide
{
    static bool WaitCompleted(Task x, int timeout)
    {
        x.Wait(timeout);
        return x.IsCompleted;
    }
    static void AsyncTest(Func<Task<int>> test, string s, int timeout, bool shouldDeadlock)
    {
        var x = System.Threading.Tasks.Task.Factory.StartNew(test);
        if(WaitCompleted(x,timeout) && WaitCompleted(x.Result,timeout))
        {
            Console.WriteLine("Completed test: {0} {1}", s, shouldDeadlock ? "- should deadlock !!" : "");
        }
        else
        {
            Console.WriteLine("Killed test: {0} - {1}", s, shouldDeadlock ? "Expected" : "should not deadlock!!");
        }

    }
    static async Task<int> Test1()
    {
        Console.WriteLine("Starting test 1");
        var table = Join.Create<Join.LockBased>();
        Synchronous.Channel<string> a; table.Initialize(out a);
        Synchronous.Channel<string> b; table.Initialize(out b);
        table.When(a).And(b).Do((_a,_b) => Console.WriteLine("Success - Test 1 - {0} - {1}",_a,_b));
        var r = a.Send("a");
        await r;
        Console.WriteLine("Finished Test 1");
        b("b");
        Console.WriteLine("Finished Test 1");
        return 0;
    }

    static async Task<int> Test2()
    {
        Console.WriteLine("Starting test 2");
        var table = Join.Create<Join.LockBased>();
        Synchronous.Channel a; table.Initialize(out a);
        Synchronous.Channel b; table.Initialize(out b);
        table.When(a).And(b).Do(() => Console.WriteLine("Success - Test 2"));
        var r = a.Send();
        b();
        await r;
        return 1;
    }

    //TODO: Add some more tests.

    public static void Main()
    {
        AsyncTest(Test1, "Test1", 3000, true);
        AsyncTest(Test2, "Test2", 3000, false);

        //var n = 2;
         
        //var table = Join.Create<Join.LockBased>(2*n);

        //// channel arrays for requests and resources
        //Synchronous.Channel[] hungry; table.Initialize(out hungry, n);
        //Asynchronous.Channel[] forks; table.Initialize(out forks, n);
        //for (int i = 0; i < n; i++)
        //{
        //    var leftFork = forks[i]; var rightFork = forks[(i + 1) % n];
        //    var j = i;
        //    // a join pattern 
        //    table.When(hungry[i]).And(leftFork).And(rightFork).Do(() =>
        //    {
        //        // eat ... 
        //        Console.WriteLine("{0} eat",j);
        //        System.Threading.Thread.Sleep(10);
        //        leftFork(); rightFork(); // replace forks     
        //        // think ... 
        //    });
        //}
        //// set the table
        //foreach (var fork in forks) fork();

        ///*
        
        //// spawn the philosopher tasks
        //foreach (Synchronous.Channel h in hungry)
        //{
        //    new System.Threading.Thread(obj =>
        //    {  
        //        while (true) h(); // request to eat  
        //    }).Start();

        //}
        //*/
        //int k = 0;
        //// spawn the philosopher tasks
        //foreach (Synchronous.Channel h in hungry)
        //{
        //    var j = k++;
        //    Action phil = async () =>
        //    {
        //        Console.WriteLine("{0} phil",j);
        //        int l = 0;
        //        await Task.Yield();
        //        while (true)
        //        {
        //            l++;
        //            Console.WriteLine("{0} phil loop {1}", j, l);
                    
        //            //await Task.Yield();
        //            await h.Send(); // request to eat  
        //        }

        //    };
        //    phil();
        //}
        
        
        System.Console.ReadLine();
    }
}