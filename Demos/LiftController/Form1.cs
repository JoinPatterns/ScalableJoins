//------------------------------------------------------------------------------
// <copyright file="Form1.cs" company="Microsoft">
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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Media;
using Microsoft.Research.Joins;

namespace LiftController
{
  public partial class Simulation : Form
  {
    public Simulation()
    {
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.DoubleBuffer, true);
      Graphics g = Graphics.FromImage(offscreen);
      using (g) { g.Clear(Color.Transparent); }

      InitializeComponent();
  }

    public Bitmap offscreen = new Bitmap((2+2*Program.NUMLIFTS) * Program.COLUMNWIDTH,(Program.NUMFLOORS+1)*Program.COLUMNHEIGHT);

    protected override void  OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      Graphics g = e.Graphics;
      g.DrawImage(offscreen, 0, 0);
     
    }

    public void Setup()
    {
      Simulation form = this;
      
      int NUMLIFTS = Program.NUMLIFTS;
      int NUMFLOORS = Program.NUMFLOORS;
      int COLUMNHEIGHT = Program.COLUMNHEIGHT;
      int NUMPEOPLE = Program.NUMPEOPLE;

      form.Size = new Size((2 + 2 * NUMLIFTS + 1) * Program.COLUMNWIDTH, (NUMFLOORS + 3) * COLUMNHEIGHT);

      Font Wingdings = new System.Drawing.Font("Wingdings", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
      List<Lift> lifts = new List<Lift>();
      List<Cabin> cabins = new List<Cabin>();
      List<Floor> floors = new List<Floor>();
      List<Person> people = new List<Person>();

    

      for (int i = 0; i < NUMLIFTS; i++)
      {
        Lift lift = new Lift(i);
        lifts.Add(lift);
        form.Controls.AddRange(lift.mStopButtons);
      
        Cabin cabin = new Cabin(i, lift, floors);
        cabins.Add(cabin);

        form.Load += new EventHandler(delegate { lift.Start(); });
        form.Load += new EventHandler(delegate { cabin.Start(); });
      }


      for (int i = 0; i < NUMFLOORS; i++)
      {

        Floor floor = new Floor(lifts,i);
        floors.Add(floor);
        form.Controls.Add(floor.mDownButton);
        form.Controls.Add(floor.mUpButton);
        
        form.Load += new EventHandler(delegate { floor.Start(); });
      }

      Random r = new Random();
      for (int i = 0; i < NUMPEOPLE; i++)
      { 
        Person p = new Person(i);
        people.Add(p);
        form.Load += new EventHandler(delegate {
          p.Start(); 
          p.GotoFloor(floors[r.Next(0,NUMFLOORS)]);
        });
      }

      Timer timer = new Timer(); ;
      timer.Interval = 40;
      DateTime lastRepaint = DateTime.Now;
      timer.Tick += (EventHandler)delegate
      {
        DateTime timeCalled = DateTime.Now;
        TimeSpan delta = timeCalled - lastRepaint;
        lastRepaint = timeCalled;
        Graphics g = Graphics.FromImage(offscreen);
        using (g)
        {
          g.Clear(Color.Transparent);

          foreach (Lift lift in lifts)
          {
            lift.Redraw(new Pair<TimeSpan, Graphics>(delta, g));
          };

          foreach (Floor floor in floors)
          {
            floor.Redraw(new Pair<TimeSpan, Graphics>(delta, g));
          };

          foreach (Cabin cabin in cabins)
          {
            cabin.Redraw(new Pair<TimeSpan, Graphics>(delta, g));
          };
          foreach (Person person in people)
          {
            person.Redraw(new Pair<TimeSpan, Graphics>(delta, g));
          }

          form.Invalidate();
        }
      };
      
      form.Load += new EventHandler(delegate
      {
        lastRepaint = DateTime.Now;
        timer.Start();

      });

    }
    
  }
}