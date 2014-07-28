//------------------------------------------------------------------------------
// <copyright file="Main.cs" company="Microsoft">
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
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Collections;
using Microsoft.Research.Joins;

public interface IViewable {
  void Redraw(TimeSpan timepassed, Graphics g);
}
 
class Room {
   
  private readonly Asynchronous.Channel<int> HasSpaces;
  private readonly Asynchronous.Channel IsFull;

  public readonly Synchronous.Channel Enter;
  public readonly Synchronous.Channel Leave;

  public Room (int size) {
    Join j = Join.Create();
    j.Initialize(out HasSpaces);
    j.Initialize(out IsFull);
    j.Initialize(out Enter);
    j.Initialize(out Leave);

    j.When(Enter).And(HasSpaces).Do(delegate (int n) {
      if (n > 1) HasSpaces(n-1);
      else IsFull();
    });

    j.When(Leave).And(HasSpaces).Do(delegate (int n) {
      HasSpaces(n+1);
    });
    j.When(Leave).And(IsFull).Do(delegate {
      HasSpaces(1);
    });

    HasSpaces(size); 
  }
}

public class Fork : IViewable {
  private static Bitmap forkgif = Helper.LoadBitmap("fork.png");

  
  private readonly Asynchronous.Channel IsFree;
  private readonly Asynchronous.Channel NotFree;

  public readonly Synchronous.Channel<Pair<TimeSpan,Graphics>> Redraw;
  public readonly Synchronous.Channel Take;
  public readonly Synchronous.Channel Put;
  public void Close(){ }
 
  public Fork(){
    Join j = Join.Create();
    j.Initialize(out IsFree);
    j.Initialize(out NotFree);
    j.Initialize(out Redraw);
    j.Initialize(out Put);
    j.Initialize(out Take);

    j.When(Redraw).And(IsFree).Do(delegate (Pair<TimeSpan,Graphics> p) {
      IsFree();
      p.Snd.DrawImageUnscaled(forkgif,-7,-120);
    });
    j.When(Redraw).And(NotFree).Do(delegate {
      NotFree();
    });

    j.When(Take).And(IsFree).Do(delegate { 
       NotFree();
    });
    j.When(Put).And(NotFree).Do(delegate {
      IsFree();
    });

    IsFree();

  }

  void IViewable.Redraw(TimeSpan timepassed, Graphics g) {
    this.Redraw(new Pair<TimeSpan,Graphics>(timepassed, g));
  }
  
}

public class Phil : IViewable {
  private static Bitmap humface = Helper.LoadBitmap("humface.png");
  private static Bitmap humface2 = Helper.LoadBitmap("humface2.png");
  private static Bitmap humface3 = Helper.LoadBitmap("humface3.png");
  private static Bitmap humface4 = Helper.LoadBitmap("humface4.png");
  private static Bitmap humface5 = Helper.LoadBitmap("humface5.png");

  static int halfheight = humface.Height / 2;
  static int halfwidth = humface.Width /2;

  private static Room theRoom = new Room(4);
  const int minpos = 100;
  const int maxpos = 180;
  const int switchinterval = 500;
  private int switchtime=0;
  private bool switchstate=false;

  enum State
  {
    Thinking,
    Hungry,
    Entering,
    GotOne,
    Eating,
    Leaving
  }
 
    
  private Random rg; 
  private Fork leftFork, rightFork;
  private int pos;
  private int speed;
  private State state;

  private readonly Asynchronous.Channel GetALife;
  private readonly Asynchronous.Channel ViewLock;
  private readonly Synchronous.Channel<State> SetState;
  private readonly Asynchronous.Channel EndMove;

  public Synchronous.Channel<Pair<TimeSpan,Graphics>> Redraw;
  private Synchronous.Channel WaitForEndMove;

  public Phil(int seed, Fork left, Fork right) {

    Join j = Join.Create();
    j.Initialize(out GetALife);
    j.Initialize(out ViewLock);
    j.Initialize(out EndMove);
    j.Initialize(out Redraw);
    j.Initialize(out SetState);
    j.Initialize(out WaitForEndMove);
  

    j.When(GetALife).Do(getalife);
    j.When(Redraw).And(ViewLock).Do(redraw_viewlock);
    j.When(SetState).And(ViewLock).Do(setstate_viewlock);
    j.When(WaitForEndMove).And(EndMove).Do(delegate{});

    rg = new Random(seed); /* each phil get its own (thread unsafe) rg */
    pos = maxpos;
    state = State.Thinking;
    speed = 2;
    leftFork = left;
    rightFork = right; 
    ViewLock();
    GetALife();

  }


  private void getalife() {
    Thread.CurrentThread.IsBackground = true;
    while (true) {
      SetState(State.Thinking);
      Thread.Sleep(rg.Next(3000) + 1000);
      SetState(State.Hungry);
      theRoom.Enter();
      SetState(State.Entering);
      WaitForEndMove();
      leftFork.Take();
      SetState(State.GotOne);
      rightFork.Take();
      SetState(State.Eating);
      Thread.Sleep(rg.Next(3000) + 2000);
      leftFork.Put(); 
      rightFork.Put();
      theRoom.Leave();
      SetState(State.Leaving);
      WaitForEndMove();
    }
  }

  
  public void redraw_viewlock(Pair<TimeSpan,Graphics> p) {
    TimeSpan timepassed = p.Fst;
    Graphics g = p.Snd;
    Bitmap bmp = null;
    int tm = (int)timepassed.TotalMilliseconds;
    switchtime += tm;
    if (switchtime > switchinterval) {
      switchtime = 0;
      switchstate = !switchstate;
    }
    int distance = speed * ((int) Math.Round((double)tm / 40.0));
    switch (state) {
      case State.Thinking:
        bmp = humface;
        break;
      case State.Hungry:
        bmp = switchstate ? humface2 : humface;
        break;
      case State.Entering:
        bmp = switchstate ? humface2 : humface;
        pos -= distance;
        if (pos < minpos) {
          pos = minpos;
          state = State.Hungry;
          EndMove();
        }
        break;
      case State.GotOne:
        bmp = humface3;
        break;
      case State.Eating:
        bmp = switchstate ? humface4 : humface5;
        break;
      case State.Leaving:
        bmp = humface;
        pos += distance;
        if (pos > maxpos) {
          pos = maxpos;
          state = State.Thinking;
          EndMove();
        }
        break;
    }
    ViewLock();

    g.DrawImageUnscaled(bmp, -halfwidth, -pos-2*halfheight);
    return;
  }

  private void setstate_viewlock(State s) {
    state = s;
    ViewLock();
  }

  void IViewable.Redraw(TimeSpan timepassed, Graphics g)
  {
    this.Redraw(new Pair<TimeSpan,Graphics>(timepassed, g));
  }
} 

public class PhilView : Form {
  private static Bitmap spagbmp = Helper.LoadBitmap("spaghetti.gif");
  private DateTime lastRepaint = DateTime.Now;
  private System.Windows.Forms.Timer timer;
  private IViewable[] sprites;
  private float angle;

  const int bmpSize=600;
  const int halfSize = bmpSize / 2;
  const int tableRadius = 100;

  Bitmap offscreen = new Bitmap(bmpSize,bmpSize);

   
  public PhilView (IViewable[] objs) {
    
    this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    this.SetStyle(ControlStyles.UserPaint, true);
    this.SetStyle(ControlStyles.DoubleBuffer, true);
    
    Size = new Size(600,620);
    Text = "Dining Philosophers using Joins";
    sprites = objs;
    angle = 360f / objs.Length;
    
    Graphics g = Graphics.FromImage(offscreen);
    g.Clear(Color.AliceBlue);
    
  }
  
  protected override void OnLoad(EventArgs e){
    timer  = new System.Windows.Forms.Timer();
    timer.Interval = 40;
    timer.Tick += new EventHandler(OnTimer);
    timer.Start();
  }

  protected override void OnClosed(EventArgs e){
      timer.Stop();
      base.OnClosed(e);
  }

  private void OnTimer(object sender, EventArgs pe) {
    DrawToOffscreen();
    Invalidate();
  }
  
  void DrawToOffscreen() {
    DateTime timeCalled = DateTime.Now;
    TimeSpan delta = timeCalled - lastRepaint;
    lastRepaint = timeCalled;
       
    Graphics g = Graphics.FromImage(offscreen);
    using (g) {
        g.Clear(Color.AliceBlue);
        g.TranslateTransform(halfSize, halfSize);
        g.FillEllipse(Brushes.DarkSlateBlue, - tableRadius, - tableRadius, tableRadius*2, tableRadius*2);
        g.DrawImageUnscaled(spagbmp, -55,-25);

        foreach (IViewable sprite in sprites) {
            sprite.Redraw(delta, g);
            g.RotateTransform(angle, MatrixOrder.Prepend);
        }
    }
  }
      
  protected override void OnPaint(PaintEventArgs pe) {
    Graphics g = pe.Graphics;
    g.FillRectangle(Brushes.White, this.ClientRectangle);
    g.DrawImage(offscreen,0,0);

  }
 
  public static void Main() {
    const int howmany = 5;
    IViewable[] thingies = new IViewable[2*howmany];
    for(int i=0; i<2*howmany; i += 2) {
      thingies[i] = new Fork();
    }
    for(int i=1; i<2*howmany; i += 2) {
      thingies[i] = new Phil(i,(Fork)thingies[i-1],(Fork)thingies[(i+1)%(2*howmany)]);
    }
       
    Application.Run(new PhilView(thingies));

  }
    
}

public class Helper {
   public static Bitmap LoadBitmap(string filename) {
     string path = Path.GetDirectoryName(Application.ExecutablePath);
     string root = Path.GetPathRoot(path);
     while (path != root) {
     string file = path + "\\" + filename;
     if (File.Exists(file)) 
        return new Bitmap(file);
     else
        path = Path.GetDirectoryName(path);
     }
     throw new System.IO.FileNotFoundException("Could not locate:",filename);
  }
}

