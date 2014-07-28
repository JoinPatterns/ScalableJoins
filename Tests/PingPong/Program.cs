
using System;
using System.Threading;
using Microsoft.Research.Joins;

namespace PingPong {
 
public interface Sem {
  void signal();
  void wait();
}

public class LSem : Sem {
  int i = 0;

  public void signal() {
    Monitor.Enter(this);
    i++;
    Monitor.Pulse(this);
    Monitor.Exit(this);
  }

  public void wait() {
    Monitor.Enter(this);
    while (i == 0) {
       Monitor.Wait(this);
    }
    i--;
    Monitor.Exit(this);
  }
}


/*
public class PSem : Sem {
  public void signal() {s();}

  async s() & public void wait() {
  }
}
 */

public class PSem : Sem {
  public void signal() {s();}

  Asynchronous.Channel s;
  Synchronous.Channel wait;

  void Sem.wait() { wait(); }


  public PSem(){
    Join j = Join.Create();
    j.Initialize(out s);
    j.Initialize(out wait);
    j.When(wait).And(s).Do(delegate {});
  }
}

public class PingPong {
  static Sem s1;
  static Sem s2;
  static Thread t1;
  static Thread t2;
  static int max = 30000;

  static void mt1() {
    for(int i=0;i<max;i++) {
      try{
       Thread.Sleep(Timeout.Infinite);
      } catch (ThreadInterruptedException ) {}
      t2.Interrupt();
      if(i%1000 == 0) {Console.Write(1);}
    }
    done();
  }

  static void mt2() {
    for(int i=0;i<max;i++) {
      t1.Interrupt();
      try{
       Thread.Sleep(Timeout.Infinite);
      } catch (ThreadInterruptedException ) {}
      if(i%1000 == 0) {Console.Write(2);}
    }
    done();
  }
  static Synchronous.Channel wait;
  static Asynchronous.Channel m1, m2, done;
  
  static void Main() {
   
    Join j = Join.Create();
    j.Initialize(out wait);
    j.Initialize(out m1);
    j.Initialize(out m2);
    j.Initialize(out done);

    j.When(m1).Do(delegate {
    for(int i=0;i<max;i++) {
      s2.signal();
      s1.wait();
      if(i%1000 == 0) {Console.Write(1);}
    }
    done();});

    j.When(m2).Do(delegate{
    for(int i=0;i<max;i++) {
      s2.wait();
      s1.signal();
      if(i%1000 == 0) {Console.Write(2);}

    }
    done();
  } );
    j.When(wait).And(done).Do(delegate {});




    DateTime start, finish;

    Console.WriteLine("locking semaphores");
    s1=new LSem();
    s2=new LSem();
    start=DateTime.Now;
    m1();
    m2();
    wait(); wait();
    finish=DateTime.Now;
    Console.WriteLine(finish.Subtract(start));

    Console.WriteLine("polyphonic semaphores");
    s1=new PSem();
    s2=new PSem();
    start=DateTime.Now;
    m1();
    m2();
    wait(); wait();
    finish=DateTime.Now;
    Console.WriteLine(finish.Subtract(start));

    Console.WriteLine("direct interruption");
    t1 = new Thread(new ThreadStart(mt1));
    t2 = new Thread(new ThreadStart(mt2));
    start=DateTime.Now;
    t1.Start();
    t2.Start();
    wait(); wait();
    finish=DateTime.Now;
    Console.WriteLine(finish.Subtract(start));

  }

}
}
