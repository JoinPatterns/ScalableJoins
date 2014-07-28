//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
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
using System.Windows.Forms;
using System.Drawing;
using System.Media;

namespace LiftController
{
  static class Program
  {
    static SoundPlayer muzak;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Simulation form = new Simulation();
      form.Setup();

      try {
        muzak = new SoundPlayer("..\\..\\muzak.wav");
        muzak.PlayLooping();
      } 
      catch{};

      Application.Run(form);

    }

    public const int NUMFLOORS = 12;

    public const int COLUMNWIDTH = 64;

    public const int COLUMNHEIGHT = 32;

    public const int NUMLIFTS = 3;

    public const int NUMPEOPLE = 10;

    public const int STOPTIME = 5;

    public const double INTERVAL = 40.0;

    public const int SPEED = 2;

  }
}