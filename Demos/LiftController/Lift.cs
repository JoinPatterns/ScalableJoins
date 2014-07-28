//------------------------------------------------------------------------------
// <copyright file="Lift.cs" company="Microsoft">
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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Media;
using System.Threading;
using Microsoft.Research.Joins;

namespace LiftController {
  using LiftController;

  using Time = System.Int32;
  using Reservation = Buffer<bool>;
  using WaitTime = Pair<int /*Time*/, Buffer<bool> /* Reservation */>;
  using FloorNum = System.Int32;

  public enum Dir { Up, Down };

  public delegate void Act();
  public class Sprite : ActiveObject {
    private readonly Asynchronous.Channel ViewLock;
    protected readonly Synchronous.Channel<Act> UnderViewLock;
    public Synchronous.Channel<Pair<TimeSpan, Graphics>> Redraw;

    protected readonly Asynchronous.Channel EndTransition;
    protected readonly Synchronous.Channel WaitForEndTransition;

    public virtual void RedrawUnderViewLock(TimeSpan t, Graphics g) { }
    protected Sprite() {
      join.Initialize(out ViewLock);
      join.Initialize(out Redraw);
      join.Initialize(out UnderViewLock);
      join.Initialize(out WaitForEndTransition);
      join.Initialize(out EndTransition);
      join.When(UnderViewLock).And(ViewLock).Do(delegate(Act action)
      {
        action();
        ViewLock();
      });

      join.When(Redraw).And(ViewLock).Do(delegate(Pair<TimeSpan, Graphics> p)
      {
        RedrawUnderViewLock(p.Fst, p.Snd);
        ViewLock();
      });

      join.When(WaitForEndTransition).And(EndTransition).Do(delegate { });
      ViewLock();
    }
  }



  public class Floor : Sprite {
    public Button mUpButton = new Button();
    public Button mDownButton = new Button();
    public Color mUpButtonBackColor = Color.Silver;
    public Color mDownButtonBackColor = Color.Silver;

    private List<Person> mPeople = new List<Person>();


    List<Lift> mLifts;
    public int mNum;

    public Lift Reserve(Dir dir) {
      List<Pair<Lift, WaitTime>> wait = WaitTimes(dir);
      wait.Sort(delegate(Pair<Lift, WaitTime> v1, Pair<Lift, WaitTime> v2) { return v1.Snd.Fst.CompareTo(v2.Snd.Fst); });
      wait[0].Snd.Snd.Put(true);
      for (int i = 1; i < wait.Count; i++) {
        wait[i].Snd.Snd.Put(false);
      }
      return wait[0].Fst;
    }

    private List<Pair<Lift, WaitTime>> WaitTimes(Dir dir) {
      List<Pair<Lift, WaitTime>> wait = new List<Pair<Lift, WaitTime>>();

      foreach (Lift lift in mLifts) {
        Buffer<Pair<Time, Buffer<bool>>> b = new Buffer<Pair<Time, Buffer<bool>>>();
        lift.HowLong(Helpers.pair(Helpers.pair(mNum, dir), b));
        WaitTime waittime = b.Get();
        wait.Add(Helpers.pair(lift, waittime));
      }

      return wait;
    }

    public readonly Asynchronous.Channel Up;

    public readonly Asynchronous.Channel Down;

    public readonly Asynchronous.Channel<Lift> Arrived;

    public readonly Asynchronous.Channel<Pair<Person, Ack>> PersonArrived;
    public readonly Asynchronous.Channel<Person> PersonDeparted;


    private Lift mReservedUp = null;
    private Lift mReservedDown = null;

    public Floor(List<Lift> lifts, FloorNum floor) {
      this.mLifts = lifts;
      this.mNum = floor;

      join.Initialize(out Up);
      join.Initialize(out Down);
      join.Initialize(out Arrived);
      join.Initialize(out PersonArrived);
      join.Initialize(out PersonDeparted);

      join.When(ProcessMessage).And(Up).Do(delegate
      {
        mReservedUp = Reserve(Dir.Up);
        UnderViewLock(delegate
        {
          mUpButtonBackColor = Color.Yellow;
        });
      });

      join.When(ProcessMessage).And(Down).Do(delegate
      {
        mReservedDown = Reserve(Dir.Down);
        UnderViewLock(delegate
        {
          mDownButtonBackColor = Color.Yellow;
        });
      });

      join.When(ProcessMessage).And(Arrived).Do(delegate(Lift lift)
      {
        if (mReservedUp == lift) {
          mReservedUp = null;
          UnderViewLock(delegate
          {
            mUpButtonBackColor = Color.Silver;
          });
          foreach (Person p in mPeople) p.Arrived(Helpers.pair(lift, Dir.Up));
        };
        if (mReservedDown == lift) {
          mReservedDown = null;
          UnderViewLock(delegate
          {
            mDownButtonBackColor = Color.Silver;
          });
          foreach (Person p in mPeople) p.Arrived(Helpers.pair(lift, Dir.Down));
        };
      });


      join.When(ProcessMessage).And(PersonArrived).Do(delegate(Pair<Person, Ack> p)
      {
        mPeople.Add(p.Fst);
        p.Snd.Send();
      });

      join.When(ProcessMessage).And(PersonDeparted).Do(delegate(Person person)
      {
        mPeople.Remove(person);
      });

      Font Wingdings = new System.Drawing.Font("Wingdings", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));

      Button up = mUpButton;
      up.Location = new Point(Program.COLUMNWIDTH, (Program.NUMFLOORS - mNum) * Program.COLUMNHEIGHT);
      up.Font = Wingdings;
      up.Text = "\x00F1";
      up.Size = new Size(Program.COLUMNWIDTH / 2, Program.COLUMNHEIGHT);
      up.ForeColor = Color.Green;
      up.BackColor = Color.Silver;
      up.Click += new EventHandler(delegate { this.Up(); });

      Button down = mDownButton;
      down.Location = new Point(Program.COLUMNWIDTH + Program.COLUMNWIDTH / 2 , (Program.NUMFLOORS - mNum) * Program.COLUMNHEIGHT);
      down.Text = "\x00F2";
      down.Font = Wingdings;
      down.Size = new Size(Program.COLUMNWIDTH / 2, Program.COLUMNHEIGHT);
      down.ForeColor = Color.Red;
      down.BackColor = Color.Silver;
      down.Click += new EventHandler(delegate { this.Down(); });
    }

    public override void RedrawUnderViewLock(TimeSpan t, Graphics g) {
      if (mUpButton.BackColor != mUpButtonBackColor)
        mUpButton.BackColor = mUpButtonBackColor;
      if (mDownButton.BackColor != mDownButtonBackColor)
        mDownButton.BackColor = mDownButtonBackColor;
    }
  }



  public class Lift : Sprite {
    public Button[] mStopButtons = new Button[Program.NUMFLOORS];
    public Color[] mStopButtonBackColors = new Color[Program.NUMFLOORS];
    public List<Person> mPeople = new List<Person>();

    public Asynchronous.Channel<Pair<Pair<FloorNum, Dir>, Buffer<WaitTime>>> HowLong;
    public Asynchronous.Channel<Pair<Floor, Buffer<Action>>> Arrived;
    public Asynchronous.Channel<FloorNum> Stop;

    public readonly Asynchronous.Channel<Pair<Person, Ack>> PersonArrived;
    public readonly Asynchronous.Channel<Person> PersonDeparted;

    public const Time StopTime = 5;

    private FloorNum Now = 0;

    public int mNum;

    private Lst<FloorNum> Stops = Lst<FloorNum>.nil;

    public Lift(int num) {
      mNum = num;

      join.Initialize(out HowLong);
      join.Initialize(out Arrived);
      join.Initialize(out Stop);
      join.Initialize(out PersonArrived);
      join.Initialize(out PersonDeparted);

      join.When(ProcessMessage).And(HowLong).Do(delegate(Pair<Pair<FloorNum, Dir>, Buffer<WaitTime>> p)
      {
        howLong(p.Fst.Fst, p.Fst.Snd, p.Snd);
      });

      join.When(ProcessMessage).And(Arrived).Do(delegate(Pair<Floor, Buffer<Action>> p)
      {
        UnderViewLock(delegate
        {
          mStopButtonBackColors[p.Fst.mNum] = Color.Silver;
        });
        arrived(p.Fst, p.Snd);
      });

      join.When(ProcessMessage).And(Stop).Do(delegate(FloorNum floor)
      {
        UnderViewLock(delegate
        {
          mStopButtonBackColors[floor] = Color.Yellow;
        });
        Stops = StopAt(floor, Now, Stops);
      });

      join.When(ProcessMessage).And(PersonArrived).Do(delegate(Pair<Person, Ack> p)
      {
        mPeople.Add(p.Fst);
        p.Snd.Send();
      });

      join.When(ProcessMessage).And(PersonDeparted).Do(delegate(Person person)
      {
        mPeople.Remove(person);
      });

      for (int i = 0; i < Program.NUMFLOORS; i++) {
        Button stop = new Button();
        stop.Location = new Point((2 + (1 + 2 * mNum)) * Program.COLUMNWIDTH, (Program.NUMFLOORS - i) * Program.COLUMNHEIGHT);
        stop.Text = i.ToString();
        stop.Size = new Size(Program.COLUMNWIDTH,Program.COLUMNHEIGHT);
        stop.BackColor = Color.Silver;
        int _i = i;
        stop.Click += new EventHandler(delegate { this.Stop(_i); });
        mStopButtons[i] = stop;
        mStopButtonBackColors[i] = Color.Silver;
      }
    }

    private void arrived(Floor floor, Buffer<Action> b) {
      Now = floor.mNum;
      if (Stops.Null()) {
        b.Put(Action.Wait);
        return;
      }
      if (Stops.Hd() == Now) {
        Stops = Stops.Tl();
        b.Put(Action.Stop);
        foreach (Person p in mPeople) { p.Stopped(floor); };
        return;
      }
      if (Stops.Hd() > Now) {
        b.Put(Action.Up);
        return;
      }
      if (Stops.Hd() < Now) {
        b.Put(Action.Down);
        return;
      }
    }

    private void howLong(FloorNum floor, Dir dir, Buffer<WaitTime> b) {
      Time t = WaitTime(dir, floor, Now, Stops);
      Reservation reservation = new Reservation();
      b.Put(new WaitTime(t, reservation));
      if (reservation.Get()) {
        Stops = Insert(dir, floor, Now, Stops);
      }
    }

    private Lst<FloorNum> Insert(Dir dir, FloorNum floor, FloorNum now, Lst<FloorNum> stop) {
      return Insert(dir, floor, now, new Nil<FloorNum>(), stop);
    }

    private Lst<FloorNum> Insert(Dir dir, FloorNum floor, FloorNum now, Lst<FloorNum> before, Lst<FloorNum> stop) {
      if (stop.Null()) { return before.Cons(floor).Reverse(); }
      FloorNum next = stop.Hd();
      Lst<FloorNum> after = stop.Tl();
      if (floor == next) { return before.ReverseAppend(stop); }
      if (floor == now) { return before.ReverseAppend(stop); }
      if (dir == Dir.Up && now < floor && floor < next) { return before.ReverseAppend(stop.Cons(floor)); }
      if (dir == Dir.Down && next < floor && floor < now) { return before.ReverseAppend(stop.Cons(floor)); }
      return Insert(dir, floor, next, before.Cons(next), after);
    }

    private Time WaitTime(Dir dir, FloorNum floor, FloorNum now, Lst<FloorNum> stop) {
      return WaitTime(dir, floor, now, 0, stop);
    }

    private Time WaitTime(Dir dir, FloorNum floor, FloorNum now, Time t, Lst<FloorNum> stops) {
      if (stops.Null()) { return t + Math.Abs(floor - now); }
      FloorNum next = stops.Hd();
      Lst<FloorNum> after = stops.Tl();
      if (floor == next) { return t + Math.Abs(floor - now); }
      if (dir == Dir.Up && now <= floor && floor <= next) { return t + floor - now; }
      if (dir == Dir.Down && now >= floor && floor >= next) { return t + now - floor; }
      return WaitTime(dir, floor, next, t + Math.Abs(now - next) + StopTime, after);
    }

    private Lst<FloorNum> StopAt(FloorNum floor, FloorNum now, Lst<FloorNum> stops) {
      return StopAt(floor, Lst<FloorNum>.nil, stops.Cons(now)).Tl(); //NB: we take the tail to avoid restopping at now.
    }

    private Lst<FloorNum> StopAt(FloorNum floor, Lst<FloorNum> before, Lst<FloorNum> stops) {
      if (stops.Null()) { return before.Cons(floor).Reverse(); }
      if (floor == stops.Hd()) { return before.ReverseAppend(stops); }
      if (!stops.Tl().Null()) {
        FloorNum X = stops.Hd(); FloorNum Y = stops.Tl().Hd(); Lst<FloorNum> afterXY = stops.Tl().Tl();
        if (X < floor && floor < Y) { return before.ReverseAppend(afterXY.Cons(Y).Cons(floor).Cons(X)); }
        if (Y < floor && floor < X) { return before.ReverseAppend(afterXY.Cons(Y).Cons(floor).Cons(X)); }
      }
      return StopAt(floor, before.Cons(stops.Hd()), stops.Tl());
    }

    public override void RedrawUnderViewLock(TimeSpan t, Graphics g) {
      for (int i = 0; i < Program.NUMFLOORS; i++) {
        if (mStopButtons[i].BackColor != mStopButtonBackColors[i])
          mStopButtons[i].BackColor = mStopButtonBackColors[i];
      }
    }

  }

  public enum Action { Wait, Stop, Up, Down };

  public enum CabinState { Waiting, Stopping, GoingUp, GoingDown };

  public class Cabin : Sprite {
    CabinState mState;

    readonly int mNum;

    Lift mLift;
    List<Floor> mFloors;

    public Cabin(int num, Lift lift, List<Floor> floors) {
      this.mNum = num;
      this.mLift = lift;
      this.mFloors = floors;

      join.When(ProcessMessage).Do(CabinLoop);

    }

    FloorNum mFloor = 0;

    private void CabinLoop() {
      Buffer<Action> b = new Buffer<Action>();
      while (true) {
        mLift.Arrived(Helpers.pair(mFloors[mFloor], b));
        switch (b.Get()) {
          case Action.Wait:
            UnderViewLock(delegate { mState = CabinState.Waiting; });
            break;
          case Action.Stop:
            mFloors[mFloor].Arrived(mLift);
            UnderViewLock(delegate { mState = CabinState.Stopping; });
            WaitForEndTransition();
            break;
          case Action.Up:
            UnderViewLock(delegate { 
               nextY = (mFloor + 1) * Program.COLUMNHEIGHT;
               mState = CabinState.GoingUp;
            });
            WaitForEndTransition();
            mFloor++;
            break;
          case Action.Down:
            UnderViewLock(delegate {
              nextY = (mFloor - 1) * Program.COLUMNHEIGHT;
              mState = CabinState.GoingDown; 
            });
            WaitForEndTransition();
            mFloor--;
            break;
          default: Debug.Assert(false);
            break;
        }
      }
    }

    int Y;
    int nextY;

    int timestopped;

    public override void RedrawUnderViewLock(TimeSpan timepassed, Graphics g) {
      int speed = Program.SPEED;
      int tm = (int)timepassed.TotalMilliseconds;
      int distance = speed * ((int)Math.Round((double)tm / Program.INTERVAL));
      switch (mState) {
        case CabinState.Waiting:
          Y = nextY;
          g.FillRectangle(Brushes.Black, LiftRectangle());
          break;
        case CabinState.GoingUp:
          Y += distance;
          if (Y > nextY) {
            Y = nextY;
            EndTransition();
          };
          g.FillRectangle(Brushes.Green, LiftRectangle());
          break;
        case CabinState.GoingDown:
          Y -= distance;
          if (Y < nextY) {
            Y = nextY;
            EndTransition();
          };
          g.FillRectangle(Brushes.Red, LiftRectangle());
          break;
        case CabinState.Stopping:
          Y = nextY;
          timestopped += distance;
          if (timestopped > 5 * Program.COLUMNHEIGHT) {
            timestopped = 0;
            EndTransition();

          };
          g.DrawRectangle(Pens.Black, LiftRectangle());
          break;
      }
    }
    private Rectangle LiftRectangle() {
      return new Rectangle(new Point((2 + 2 * mNum) * Program.COLUMNWIDTH, (
          Program.NUMFLOORS * Program.COLUMNHEIGHT) - Y), new Size(Program.COLUMNWIDTH, Program.COLUMNHEIGHT));
    }
  }

  public enum PersonState { Waiting, Exiting, Entering, Travelling }


  public class Person : Sprite {
    private readonly int mNum;
    private Dir mDir;
    private FloorNum mNext;

    Brush mBrush;
    PersonState State;

    public Asynchronous.Channel<Floor> GotoFloor;
    public Asynchronous.Channel<Pair<Lift, Dir>> Arrived;
    public Asynchronous.Channel<Floor> Stopped;

    private Asynchronous.Channel<Floor> OnFloor;
    private Asynchronous.Channel<Lift> InLift;


    Random random;

    private void ChooseDir(Floor floor) {
      mDir = (floor.mNum == 0) ? Dir.Up :
                (floor.mNum == Program.NUMFLOORS - 1) ? Dir.Down
                : (Dir)random.Next(0, 2);
      if (mDir == Dir.Up) { floor.Up(); }
      if (mDir == Dir.Down) { floor.Down(); }
    }

    private void ChooseFloor(Floor floor, Lift lift) {
      if (mDir == Dir.Up) mNext = random.Next(floor.mNum + 1, Program.NUMFLOORS);
      if (mDir == Dir.Down) mNext = random.Next(0, floor.mNum);
      lift.Stop(mNext);
    }

    public Person(int num) {
      random = new Random(num);
      mNum = num;
      mBrush = mBrushes[num % mBrushes.Length];

      join.Initialize(out GotoFloor);
      join.Initialize(out Stopped);
      join.Initialize(out Arrived);

      join.Initialize(out OnFloor);
      join.Initialize(out InLift);

      join.When(ProcessMessage).And(GotoFloor).Do(delegate(Floor floor)
      {
        UnderViewLock(delegate
        {
          Y = (Program.NUMFLOORS - floor.mNum) * Program.COLUMNHEIGHT;
        });
        OnFloor(floor);
        Ack a = new Ack();
        floor.PersonArrived(Helpers.pair(this, a));
        a.Receive();
        ChooseDir(floor);
      });

      join.When(ProcessMessage).And(OnFloor).And(Arrived).Do(delegate(Floor floor, Pair<Lift, Dir> p)
      {
        Lift lift = p.Fst;
        Dir dir = p.Snd;
        if (dir == mDir) {
          floor.PersonDeparted(this);
          UnderViewLock(delegate
          {
            State = PersonState.Entering;
            X = 0;
            nextX =  (2 + 2 * lift.mNum) * Program.COLUMNWIDTH;
            speed = (int)Math.Round((double)(Program.SPEED * 2 * nextX / (1.0 * Program.STOPTIME * Program.COLUMNHEIGHT)));
          });
          WaitForEndTransition();
          UnderViewLock(delegate 
          {
            X = nextX;
            State = PersonState.Travelling;
          });
          Ack ack = new Ack();
          lift.PersonArrived(Helpers.pair(this, ack));
          ack.Receive();
          InLift(lift);
          ChooseFloor(floor, lift);
        }
        else OnFloor(floor);
      });

      join.When(ProcessMessage).And(InLift).And(Arrived).Do(delegate(Lift lift, Pair<Lift, Dir> p) { InLift(lift); });

      join.When(ProcessMessage).And(InLift).And(Stopped).Do(delegate(Lift lift, Floor stop)
      {
        if (stop.mNum == mNext) {

          lift.PersonDeparted(this);
          UnderViewLock(delegate
          {
            State = PersonState.Exiting;
            nextX = 0;
            Y = (Program.NUMFLOORS - stop.mNum) * Program.COLUMNHEIGHT;
          });
          WaitForEndTransition();
          UnderViewLock(delegate {
            X = 0;
            State = PersonState.Waiting;
          });
          OnFloor(stop);
          Thread.Sleep(1000 * random.Next(1, 10));
          Ack ack = new Ack();
          stop.PersonArrived(Helpers.pair(this, ack));
          ack.Receive();
          ChooseDir(stop);
        }
        else InLift(lift);
      });

   
      XOffSet = (mNum % 16) * 2;
      YOffSet = mNum / 16 * 2;
      X = 0;
      nextX = 0;
    }

    int speed;
    int X;
    int nextX;
    int Y;
    int XOffSet;
    int YOffSet;

    public override void RedrawUnderViewLock(TimeSpan timepassed, Graphics g) {
      int tm = (int)timepassed.TotalMilliseconds;
      int distance = speed * ((int)Math.Round((double)tm / Program.INTERVAL));
      
      switch (State) {
        case PersonState.Waiting:
          g.FillEllipse(mBrush, PersonRectangle());
          break;
        case PersonState.Travelling:
          // nothing to draw;
          break;
        case PersonState.Exiting:
          X -= distance;
          if (X < nextX) {
            X = nextX;
            EndTransition();
          };
          g.FillEllipse(mBrush, PersonRectangle());
          break;
        case PersonState.Entering:
          X += distance;
          if (X > nextX) {
            X = nextX;
            EndTransition();
          };
          g.FillEllipse(mBrush, PersonRectangle());
          break;
      }
    }

    public static Brush[] mBrushes = { 
     Brushes.MediumAquamarine,Brushes.MediumBlue, Brushes.MediumOrchid, Brushes.MediumPurple,
     Brushes.MediumSeaGreen, Brushes.MediumSlateBlue, Brushes.MediumTurquoise, Brushes.MediumVioletRed
    };

    private Rectangle PersonRectangle() {
      return new Rectangle(new Point(X + XOffSet, Y + YOffSet), new Size(Program.COLUMNWIDTH / 4,Program.COLUMNHEIGHT));
    }

  }

}

