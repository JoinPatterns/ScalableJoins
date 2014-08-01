using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Research.Joins;


namespace Boids {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {


    public const int NumBoids = 400;
    
    public Synchronous<Data[]>.Channel<Data>[] Boids;
    private ModelVisual3D[] BoidModels;
    private Boid[] boids;
    private Data[] CurrentData;
   
    private Rect3D Space = new Rect3D(0, 0, 0, 600, 200, 800);
    private Synchronous<Data[]>.Channel Tick;
    private const double ZoomFactor = 10.0;
    private DispatcherTimer Timer = new DispatcherTimer();

    private void Window_Loaded(object sender, EventArgs e) {
      int Xmargin = (int)Math.Round(Space.SizeX / 10.0);
      int Ymargin = (int)Math.Round(Space.SizeY / 10.0);
      int Zmargin = (int)Math.Round(Space.SizeZ / 10.0);
      Rect3D aviary = new Rect3D((double)Xmargin, (double)Ymargin, (double)Zmargin, Space.SizeX - (2 * Xmargin), Space.SizeY - (2 * Ymargin), Space.SizeZ - (2 * Zmargin));
      Random random = new Random();
      Vector3D place = new Vector3D(Space.SizeX / 2.0, Space.SizeY / 2.0, Space.SizeZ / 2.0);
      for (var i = 0; i < NumBoids; i++) {
        boids[i] = new Boid(aviary, place, NumBoids, i, this);
        boids[i].position = new Vector3D((double)random.Next(Xmargin, (int)Math.Round((double)(Space.SizeX - (2 * Xmargin)))), (double)random.Next(Ymargin, (int)Math.Round((double)(Space.SizeY - (2 * Ymargin)))), (double)random.Next(Zmargin, (int)Math.Round((double)(Space.SizeZ - (2 * Zmargin)))));
        boids[i].velocity = new Vector3D(0.0, 0.1, 0.0);
      }

      foreach (Boid boid in boids) {
        boid.Start();
      }
    }


    public static bool SCALABLE = false;
    public static bool SYNCHRONOUS = true;

    public MainWindow() {
      var args = System.Environment.GetCommandLineArgs();
        if (args != null)
            foreach (var arg in args)
            {
                if (arg == "/S") SCALABLE = true;
                if (arg == "/A") SYNCHRONOUS = false;
            };

      this.Title = (SCALABLE) ? "SCALABLE" : "LOCKBASED";

      this.Title += (SYNCHRONOUS) ? " SYNCHRONOUS" : " ASYNCHRONOUS";
      BoidModels = new ModelVisual3D[NumBoids];
      Timer = new DispatcherTimer();
      boids = new Boid[NumBoids];
      Space = new Rect3D(0.0, 0.0, 0.0, 600.0, 200.0, 800.0);
      CurrentData = new Data[NumBoids];
      InitializeComponent();
      
      InitJoin();

      // The color combinations to use for boids.  At least one combination is necessary,
      // but more can be added to get more variations.
      var colorCombinations = new Tuple<Color, Color>[]
            {
                Tuple.Create(Colors.SeaGreen, Colors.Silver),
                Tuple.Create(Colors.Pink, Colors.Purple),
                Tuple.Create(Colors.Yellow, Colors.Gold),
                Tuple.Create(Colors.Red, Colors.Tomato),
                Tuple.Create(Colors.Blue,Colors.BlueViolet),
                Tuple.Create(Colors.Green,Colors.LightGreen),
                Tuple.Create(Colors.Aqua,Colors.Aquamarine)
            };

      for (var i = 0; i < NumBoids; i++) {
        BoidModels[i] = new ModelVisual3D();
        var content = (System.Windows.Media.Media3D.GeometryModel3D)boidMain.Content.Clone();
        content.BackMaterial = new DiffuseMaterial(new SolidColorBrush(colorCombinations[i % colorCombinations.Length].Item2));
        content.Material = new DiffuseMaterial(new  SolidColorBrush(colorCombinations[i % colorCombinations.Length].Item1));
        BoidModels[i].Content = content;
        BoidModels[i].Transform = new TranslateTransform3D(CurrentData[i].position);
        viewport3D.Children.Add(BoidModels[i]);
      }

      
      System.Threading.ThreadPool.QueueUserWorkItem(_ => {
        while (true) {
         var d = Tick();
         Dispatcher.Invoke((Action<Data[]>) Animate,DispatcherPriority.Input, d);
        }
      });
    }

    public Synchronous.Channel[] Sync;
    public Asynchronous.Channel Running, NotRunning;
    public Synchronous.Channel PauseResume;

    public void InitJoin() {


      var join = (MainWindow.SCALABLE) ? 
          Join.Create<Join.Scalable>(4 + NumBoids + NumBoids)
        : Join.Create<Join.LockBased>(4 + NumBoids + NumBoids);
    
      join.Initialize(out Boids, NumBoids);
      join.Initialize(out Tick);
      join.Initialize(out Running);
      join.Initialize(out NotRunning);
      join.Initialize(out PauseResume);
      join.Initialize(out Sync, NumBoids);
      for (int i = 0; i < NumBoids; i++)
      {   // dummy channels to add contention
          join.When(Sync[i]).Do(() => { ;});
      }
      // rendezvous pattern
      join.When(Tick).And(Running).And(Boids).Do(data => { Running(); return data; });

      // rendezvous pattern
      join.When(PauseResume).And(Running).Do(() => NotRunning());
      join.When(PauseResume).And(NotRunning).Do(() => { ResetStats(); Running(); });
      Running();

     
      var d = new Data[NumBoids];
    }

    private void HandleMouseWheel(object sender, MouseWheelEventArgs e) {
      if (e.Delta > 0) {
        camMain.Position = Point3D.Add(camMain.Position, (Vector3D)(camMain.LookDirection * 10.0));
      }
      else {
        camMain.Position = Point3D.Subtract(camMain.Position, (Vector3D)(camMain.LookDirection * 10.0));
      }
    }

    Stopwatch stopwatch = new Stopwatch();
   
    long frames = 0;
   
    private void Animate(Data[] d) {
      if (frames == 0) stopwatch.Start();
      frames++;
      ShowStats();
     
      Vector3D unitY = new Vector3D(0.0, 1.0, 0.0);
      for (var index = 0; index < NumBoids; index++) {
        Data data = d[index];
        Transform3DGroup group = new Transform3DGroup();
        Vector3D velocity = data.velocity;
        velocity.Normalize();
        BoidModels[index].Transform = null;
        double theta = Math.Acos(Vector3D.DotProduct(velocity, unitY) / (velocity.Length * unitY.Length)) * (180.0/Math.PI);
        double len = data.velocity.Length;
        ScaleTransform3D s = new ScaleTransform3D(2.0, 2.0, 2.0);
        Vector3D vectord3 = new Vector3D(0.0, 1.0, 0.0);
        AxisAngleRotation3D rotation = new AxisAngleRotation3D(Vector3D.CrossProduct(vectord3, velocity), theta);
        RotateTransform3D rt = new RotateTransform3D(rotation);
        TranslateTransform3D tt = new TranslateTransform3D(data.position - data.velocity);
        group.Children.Add(s);
        group.Children.Add(rt);
        group.Children.Add(tt);
        BoidModels[index].Transform = group;
      }
      var delay = (int)this.FPS.Value; 
      //if (delay > 0) 
          System.Threading.Thread.Sleep(delay);
    }

    private void ScatterGatherClick(object sender, RoutedEventArgs e) {
      foreach (var b in boids) b.Toggle();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e) {
      camMain.Position = Point3D.Add(camMain.Position, camMain.LookDirection * ZoomFactor);

    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e) {
      camMain.Position = Point3D.Subtract(camMain.Position, camMain.LookDirection * ZoomFactor);
    }
    private void ShowStats()
    {
        Stats.Content = String.Format("frames/s: {0:00.00}",(frames / stopwatch.Elapsed.TotalSeconds));
    }
    private void ResetStats()
    {
        frames = 0;
        stopwatch.Restart();
        ShowStats();
    }
    private void FPS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ResetStats();
       
    }

    public volatile bool AddContentionChecked = false;
    
    private void AddContention_Checked(object sender, RoutedEventArgs e)
    {
       
        AddContentionChecked = true;
        ResetStats();
    }

    private void AddContention_Unchecked(object sender, RoutedEventArgs e)
    {
        AddContentionChecked = false;
        ResetStats();
    }

    private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
    {
        PauseResume();
    }








  }

  public class Boid {
    // Fields
    private Vector3D Alignment;
    private double Attraction = 1.0;
    private readonly Rect3D Aviary;
    private Vector3D Cohesion;
   
    private int MinDistance = 50;
    private int Nearby;
    private readonly int NumBoids;
    
    private readonly Vector3D Place;
    public Vector3D position;
    
    private Vector3D Separation;
    private const double SpeedLimit = 1.5;
    public Asynchronous.Channel Start;
    public Asynchronous.Channel Toggle;
    public Asynchronous.Channel Lock;
    public Vector3D velocity;

    // Events
    public MainWindow MainWindow;
    public int Index;

    // Methods
    public Boid(Rect3D Aviary, Vector3D Place, int NumBoids, int Index, MainWindow MainWindow) {
      this.Aviary = Aviary;
      this.NumBoids = NumBoids;
      this.Place = Place;
      this.Index = Index;
      this.MainWindow = MainWindow;

      var j = (MainWindow.SCALABLE) ? Join.Create<Join.Scalable>(3)
                                    : Join.Create<Join.LockBased>(3);
      j.Initialize(out Start);
      j.Initialize(out Toggle);
      j.Initialize(out Lock);
      if (MainWindow.SYNCHRONOUS) 
          j.When(Start).Do(CaseStartSync);
      else
          j.When(Start).Do(CaseStartAsync);
      j.When(Toggle).And(Lock).Do(CaseToggle);
      Lock();
 
    }
    public Vector3D BoundPosition() {
      Vector3D vectord2 = new Vector3D();
      if (position.X < Aviary.X) {
        vectord2.X = SpeedLimit;
      }
      else if (position.X > (Aviary.X + Aviary.SizeX)) {
        vectord2.X = -SpeedLimit;
      }
      if (position.Y < Aviary.Y) {
        vectord2.Y = SpeedLimit;
      }
      else if (position.Y > (Aviary.Y + Aviary.SizeY)) {
        vectord2.Y = -SpeedLimit;
      }
      if (position.Z < Aviary.Z) {
        vectord2.Z = SpeedLimit;
        return vectord2;
      }
      if (position.Z > (Aviary.Z + Aviary.SizeZ)) {
        vectord2.Z = -SpeedLimit;
      }
      return vectord2;
    }


    public void Update() {
      if (Nearby > 0) {
        Cohesion = Cohesion / Nearby;
        Cohesion = Attraction * (Cohesion - position) / 100.0;
        Alignment = Alignment / Nearby;
        Alignment = (Alignment - velocity) / 8.0;
      }
      velocity = velocity + Cohesion;
      velocity = velocity + Separation;
      velocity = velocity + Alignment;
      velocity = velocity + BoundPosition();
      velocity = velocity + Tendency();
      LimitVelocity();
      position = position + velocity;

      Nearby = 0;
      Cohesion = new Vector3D(0.0, 0.0, 0.0);
      Separation = new Vector3D(0.0, 0.0, 0.0);
      Alignment = new Vector3D(0.0, 0.0, 0.0);
    }

    public void ProcessBoid(Data boid) {
      Vector3D vectord = boid.position - position;
      double num = Math.Acos(Vector3D.DotProduct(velocity, boid.position - position) / (velocity.Length * vectord.Length)) * (180.0/Math.PI);
      vectord = boid.position - position;
      if ((vectord.Length < MinDistance) & (num < 135.0)) {
        Nearby = Nearby + 1;
        Cohesion += boid.position;
        vectord = boid.position - position;
        if (vectord.Length < 10.0) {
          Separation -= boid.position - position;
        }
        Alignment += boid.velocity;
      }
    }

    
    private void CaseStartSync() {
      System.Threading.Thread.CurrentThread.IsBackground = true;

      var data = MainWindow.Boids[Index](new Data(position, velocity));
      while (true) {
        for (int i = 0; i < NumBoids; i++ ) {
              if (MainWindow.AddContentionChecked) MainWindow.Sync[Index]();
              if (Index != i) ProcessBoid(data[i]);
        };
        Update();
        data = MainWindow.Boids[Index](new Data(position, velocity));
      }
    }
  
    private async void CaseStartAsync()
    {
        //System.Threading.Thread.CurrentThread.IsBackground = true;

        var data = await  MainWindow.Boids[Index].Send(new Data(position, velocity));
        while (true)
        {
            for (int i = 0; i < NumBoids; i++)
            {
                if (MainWindow.AddContentionChecked) await(MainWindow.Sync[Index].Send());
                if (Index != i) ProcessBoid(data[i]);
            };
            Update();
            data = await (MainWindow.Boids[Index].Send(new Data(position, velocity)));
        }
    }
 
    public void CaseToggle() {
      if (Attraction == 1.0) {
        Attraction = -2.0;
      }
      else {
        Attraction = 1.0;
      }
      Lock();
    }




    public void LimitVelocity() {
      if (velocity.Length > SpeedLimit) {
        velocity = (Vector3D)((velocity / velocity.Length) * SpeedLimit);
      }
    }

    public Vector3D Tendency() {
      Vector3D vectord = Place - position;
      vectord.Normalize();
      return (Vector3D)(vectord / 100.0);
    }

  }



  public struct Data {
    public Vector3D velocity;
    public Vector3D position;
    public Data(Vector3D p, Vector3D v) {
      position = p;
      velocity = v;
    }
  }



  class Startup {


    [STAThread]
    static void Main(string[] args) {
    if (args.Length == 0) {
        var thisexe = new System.Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase).LocalPath;
        var arguments = new string[] { "/L" , "/S" , "/A" };
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

      App app = new App();
      app.MainWindow = new MainWindow();
      app.MainWindow.Show();
      app.Run();
    }
  }
 

}


