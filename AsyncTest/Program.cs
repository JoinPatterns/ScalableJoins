using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Joins;

public class Slide
{


    public static void Main()
    {
        var n = 2;
         
        var table = Join.Create<Join.LockBased>(2*n);

        // channel arrays for requests and resources
        Synchronous.Channel[] hungry; table.Initialize(out hungry, n);
        Asynchronous.Channel[] forks; table.Initialize(out forks, n);
        for (int i = 0; i < n; i++)
        {
            var leftFork = forks[i]; var rightFork = forks[(i + 1) % n];
            var j = i;
            // a join pattern 
            table.When(hungry[i]).And(leftFork).And(rightFork).Do(() =>
            {
                // eat ... 
                Console.WriteLine("{0} eat",j);
                System.Threading.Thread.Sleep(10);
                leftFork(); rightFork(); // replace forks     
                // think ... 
            });
        }
        // set the table
        foreach (var fork in forks) fork();

        /*
        
        // spawn the philosopher tasks
        foreach (Synchronous.Channel h in hungry)
        {
            new System.Threading.Thread(obj =>
            {  
                while (true) h(); // request to eat  
            }).Start();

        }
        */
        int k = 0;
        // spawn the philosopher tasks
        foreach (Synchronous.Channel h in hungry)
        {
            var j = k++;
            Action phil = async () =>
            {
                Console.WriteLine("{0}fuck me",j);
                while (true)
                {
                    Console.WriteLine("{0}fuck me", j);
                    await Task.Delay(10);
                    await h.Send(); // request to eat  
                }

            };
            phil();
        }
        
        
        System.Console.ReadLine();
    }
}