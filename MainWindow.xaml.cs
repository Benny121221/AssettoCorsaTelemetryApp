﻿using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AssettoCorsaTelemetryApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const int sleepTime = 50;
		const int bufferSize = 10_000;
		double[] dataGas;
		double[] dataBrake;
		double[] dataGear;
		double[] dataSteer;
		int index = 0;
		DispatcherTimer updateTimer;
		DispatcherTimer renderTimer;
		ScottPlot.PlottableSignal sigplotGas;
		ScottPlot.PlottableSignal sigplotBrake;
		ScottPlot.PlottableSignal sigplotGear;
		ScottPlot.PlottableSignal sigplotSteer;

		public MainWindow()
		{
			InitializeComponent();
			PhysicsData.InitializePhysics();
			plotFrameGas.plt.Title("Throttle");
			plotFrameBrake.plt.Title("Brake");
			plotFrameGear.plt.Title("Gear");
			plotFrameSteer.plt.Title("Steer");
		}

		private void StartLogging_Click(object sender, RoutedEventArgs e)
		{
			index = 0;
			dataGas = new double[bufferSize];
			dataBrake= new double[bufferSize];
			dataGear= new double[bufferSize];
			dataSteer= new double[bufferSize];

			plotFrameGas.plt.Clear();
			sigplotGas = plotFrameGas.plt.PlotSignal(dataGas);
			sigplotBrake = plotFrameBrake.plt.PlotSignal(dataBrake);
			sigplotGear = plotFrameGear.plt.PlotSignal(dataGear);
			sigplotSteer= plotFrameSteer.plt.PlotSignal(dataSteer);

			updateTimer = new DispatcherTimer();
			updateTimer.Interval = TimeSpan.FromMilliseconds(sleepTime);
			updateTimer.Tick += Update;
			updateTimer.Start();

			renderTimer = new DispatcherTimer();
			renderTimer.Interval = TimeSpan.FromMilliseconds(500);
			renderTimer.Tick += RenderSignal;
			renderTimer.Start();
		}

		private void RenderSignal(object sender, EventArgs e)
		{
			sigplotGas.maxRenderIndex = index;
			sigplotBrake.maxRenderIndex = index;
			sigplotGear.maxRenderIndex = index;
			sigplotSteer.maxRenderIndex = index;

			plotFrameGas.plt.Axis(0, index - 1);
			plotFrameBrake.plt.Axis(0, index - 1);
			plotFrameGear.plt.Axis(0, index - 1);
			plotFrameSteer.plt.Axis(0, index - 1);

			plotFrameGas.Render();
			plotFrameBrake.Render();
			plotFrameGear.Render();
			plotFrameSteer.Render();
		}

		private void StopLogging_Click(object sender, RoutedEventArgs e)
		{
			updateTimer.Stop();
			renderTimer.Stop();
		}

		private void Update(object sender, EventArgs e)
		{
			if (index > bufferSize) {
				index = 0;
			}
			PhysicsData.PhysicsMemoryMap x = PhysicsData.GetPhysics();
			dataGas[index] = x.gas;
			dataBrake[index] = x.brake;
			dataGear[index] = x.gear - 1;
			dataSteer[index] = x.steerAngle;

			index++;
			Thread.Sleep(sleepTime);
		}
	}
}
