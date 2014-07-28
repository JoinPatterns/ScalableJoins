//------------------------------------------------------------------------------
// <copyright file="Main.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLED, INCLUDING BUT NOT LIMITED TO THE
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
using System.Diagnostics;
using System.Linq;


public class PhilView : Form
{
    
    private System.Windows.Forms.Timer timer;
    private IViewable[] sprites;
    private float angle;
    private Table Table;

    const int bmpSize = 600;
    const int halfSize = bmpSize / 2;
    const int tableRadius = 100;
    private Button GoButton;
    private ProgressBar ProgressBar;
    private Label Time;

    Bitmap offscreen = new Bitmap(bmpSize, bmpSize);


    public PhilView(bool scalable)
    {

        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.DoubleBuffer, true);

        Size = new Size(600, 620);
       
        Graphics g = Graphics.FromImage(offscreen);
        g.Clear(Color.Black);

        Text = (scalable) ? "SCALABLE" : "LOCKBASED";

        var howmany = Constants.howmany;

        Table = new Table(howmany, stopwatch, scalable);

        sprites = new IViewable[2 * howmany];

        for (int i = 0; i < 2 * howmany; i += 2) {
          sprites[i] = new Fork(i / 2, Table);
        }
        for (int i = 1; i < 2 * howmany; i += 2) {
          sprites[i] = new Phil(i / 2, Table);
        }
        angle = 360f / sprites.Length;


    }

    public static Stopwatch stopwatch = new Stopwatch();

    protected override void OnLoad(EventArgs e)
    {
        this.InitializeComponent();

        ProgressBar.Minimum = 0;
        ProgressBar.Maximum = Constants.maxhelpings;
        ProgressBar.Value = 0;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 40;
        timer.Tick += new EventHandler(OnTimer);
        timer.Start();

    }

    protected override void OnClosed(EventArgs e)
    {
        timer.Stop();
        base.OnClosed(e);
    }

    private void OnTimer(object sender, EventArgs pe)
    {
        DrawToOffscreen();
        Invalidate();
    }

    void DrawToOffscreen()
    {
        Graphics g = Graphics.FromImage(offscreen);
        ProgressBar.Value = Math.Min(ProgressBar.Maximum,Constants.maxhelpings - Table.helpings);
        Time.Text = String.Format("{0,10}ms",stopwatch.ElapsedMilliseconds);
        
        using (g)
        {
            g.Clear(Color.Black);
            g.TranslateTransform(halfSize, halfSize);
            g.FillEllipse(Brushes.DarkSlateBlue, -tableRadius, -tableRadius, tableRadius * 2, tableRadius * 2);
           
            float foodRadius = Convert.ToSingle(Math.Sqrt(Table.helpings / Constants.ratio / Math.PI));
            g.FillEllipse(Brushes.Green, -foodRadius, -foodRadius, foodRadius * 2, foodRadius * 2);
    
            foreach (IViewable sprite in sprites)
            {
              sprite.Redraw(g);
                g.RotateTransform(angle, MatrixOrder.Prepend);
            }
        }
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
       
        Graphics g = pe.Graphics;
        g.FillRectangle(Brushes.White, this.ClientRectangle);
        g.DrawImage(offscreen, 0, 0);

    }

    

    [STAThread]
    public static void Main(string[] args) {
      if (args.Length == 0) {
        var thisexe = new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase).LocalPath;
        var arguments = new string[] { "/L", "/S" };
        var procs =
        arguments.Select(arg => {
          var psi = new ProcessStartInfo(thisexe, arg);
          psi.CreateNoWindow = true;
          psi.UseShellExecute = false;
          var process = new Process();
          process.StartInfo = psi;
          process.Start();
          return process;
        });
        foreach (var proc in procs.ToList()) {
          proc.WaitForExit();
        }
        return;
      }
      bool SCALABLE = false;
      if (args.Length > 0) {
        foreach (var arg in args) {
          if (arg == "/S") SCALABLE = true;
          if (arg == "/L") SCALABLE = false;
        };

        
        Application.Run(new PhilView(SCALABLE));

      }
    }

    private void InitializeComponent()
    {
      this.GoButton = new System.Windows.Forms.Button();
      this.ProgressBar = new System.Windows.Forms.ProgressBar();
      this.Time = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // GoButton
      // 
      this.GoButton.Location = new System.Drawing.Point(12, 11);
      this.GoButton.Name = "GoButton";
      this.GoButton.Size = new System.Drawing.Size(75, 23);
      this.GoButton.TabIndex = 0;
      this.GoButton.Text = "GO!";
      this.GoButton.UseVisualStyleBackColor = true;
      this.GoButton.Click += new System.EventHandler(this.PhilView_Click);
      // 
      // ProgressBar
      // 
      this.ProgressBar.Location = new System.Drawing.Point(7, 41);
      this.ProgressBar.Name = "ProgressBar";
      this.ProgressBar.Size = new System.Drawing.Size(573, 23);
      this.ProgressBar.TabIndex = 1;
      // 
      // Time
      // 
      this.Time.AutoSize = true;
      this.Time.BackColor = System.Drawing.Color.Black;
      this.Time.Font = new System.Drawing.Font("Lucida Console", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Time.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
      this.Time.Location = new System.Drawing.Point(98, 11);
      this.Time.Name = "Time";
      this.Time.Size = new System.Drawing.Size(75, 21);
      this.Time.TabIndex = 2;
      this.Time.Text = "00000";
      this.Time.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // PhilView
      // 
      this.ClientSize = new System.Drawing.Size(584, 582);
      this.Controls.Add(this.Time);
      this.Controls.Add(this.ProgressBar);
      this.Controls.Add(this.GoButton);
      this.Name = "PhilView";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private void PhilView_Click(object sender, EventArgs e)
    {
        this.GoButton.Enabled = false;
        stopwatch.Start();
        foreach (var sprite in sprites)
        {
            var p = sprite as Phil;
            if (p != null) p.GetALife();
        }
    }


}
class Constants
{
    public const int maxhelpings = 1000000;
    public const float ratio = maxhelpings / 10000;
    public  static int howmany = Math.Max(2,System.Environment.ProcessorCount - 1);
   
    public const bool sleep =  false;
    public const bool animate = false;
    

}

public interface IViewable {
  void Redraw(Graphics g);
}
 
public class Table
{
    public  bool[] ForkFree = new bool[Constants.howmany];
    private readonly Asynchronous.Channel[] Forks;
    public readonly Synchronous<bool>.Channel<Action>[] Hungry;
    public readonly Synchronous.Channel[] Done;


    public volatile static int helpings = Constants.maxhelpings;
    
    public Table(int size, Stopwatch stopwatch,bool scalable)
    {
        ForkFree = new bool[size];

        Join j = (scalable) ?
                     Join.Create<Join.Scalable>(3 * size)
                   : Join.Create<Join.LockBased>(3 * size);

        j.Initialize(out Forks,size);
        j.Initialize(out Hungry,size);
        j.Initialize(out Done,size);
       
        // specify the problem - hungry philosopher i requires neighbouring forks to eat
        for (int i = 0; i<size;i++)
        {
          ForkFree[i] = true;
          var leftFork = Forks[i];
          var rightFork = Forks[(i + 1) % size];
          var _i = i;
          j.When(Hungry[i]).And(leftFork).And(rightFork).Do(eat =>
            {
                var remaining = Interlocked.Decrement(ref helpings);
                
                ForkFree[_i] = ForkFree[(_i + 1) % size] = false;

                eat();
                
                ForkFree[_i] = ForkFree[(_i + 1) % size] = true;

                leftFork();
                
                rightFork();
                return (remaining >= 0);
            });
        }
        j.When(Done).Do(
          () =>
            stopwatch.Stop()
        );
        
        // set the table
        foreach (var Fork in Forks) Fork();
       
    }
}

public class Fork : IViewable {
  private static Bitmap forkgif = Helper.LoadBitmap("fork.png");
  private Table Table;
  private int index;
  public Fork(int index, Table Table){
      this.index = index;
      this.Table = Table;
  }

  void IViewable.Redraw(Graphics g) {
     if (Table.ForkFree[this.index]) 
         g.DrawImage(forkgif, -7, -120, 

        forkgif.Width / Convert.ToSingle(Math.Sqrt(Constants.howmany/2)), forkgif.Height /
        Convert.ToSingle(Math.Sqrt(Constants.howmany/2)) );
  }
  
}

public class Phil : IViewable {
 

  static int halfheight = 10;
  static int halfwidth = 10;

  
  const int minpos = 100;
  const int maxpos = 180;
  
  

  enum State
  {
    Thinking,
    Eating
  }
 
    
  private Random rg; 
  
  private int pos;
  
  private volatile State state;

  public readonly Asynchronous.Channel GetALife;
  private readonly Table Table;
  volatile int helpings;

  private int id;
  public Phil(int id,  Table Table) {
    this.id = id;
    this.Table = Table;
    Join j = Join.Create(1);
    j.Initialize(out GetALife);

    j.When(GetALife).Do(getalife);
   
    rg = new Random(id); /* each phil get its own (thread unsafe) rg */
    pos = maxpos;
    state = State.Thinking;

  } 

  private void getalife() {
    Thread.CurrentThread.IsBackground = true;
    bool foodleft = true;
    while (foodleft) {
      if (Constants.sleep) Thread.Sleep(rg.Next(3000) + 1000);
      foodleft = Table.Hungry[id](() =>
      {
        helpings++;
        state = State.Eating;
        Thread.SpinWait(10000);
      });
      state = State.Thinking;
      Thread.SpinWait(10000);
      if (Constants.sleep) Thread.Sleep(rg.Next(3000) + 2000);
    }
    Table.Done[id](); 
  }


  void IViewable.Redraw(Graphics g)
  {
      switch (state)
      {
          case State.Thinking:
              {
                  pos = maxpos;
                  break;
              }
          case State.Eating:
              {
                  pos = minpos;
                  break;
              }
      }

      var radius = 10.0f + Convert.ToSingle(Math.Sqrt(helpings / Constants.ratio / Math.PI));
      g.FillEllipse((state == State.Eating) ? Brushes.Green : Brushes.Red, -halfwidth, -pos - 2 * halfheight, radius, radius);
      return;
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
