using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using AForge;
using AForge.Imaging;
using System.Drawing.Drawing2D;
namespace CamArtASCII
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        public static uint CurrentResolution = 0;
        public static Form1 form = (Form1)Application.OpenForms["Form1"];
        private static Bitmap img;
        private VideoCapabilities[] videoCapabilities;
        private string[] AsciiChars = { "#", "@", "$", "%", "&", "8", "?", "[", "]", "{", "}", "(", ")", "|", "+", ",", ":", "_", "-", "^", ".", "&nbsp;" };
        private StringBuilder html = new StringBuilder();
        private void Start()
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            FinalFrame = new VideoCaptureDevice(CaptureDevice[0].MonikerString);
            videoCapabilities = FinalFrame.VideoCapabilities;
            FinalFrame.VideoResolution = videoCapabilities[videoCapabilities.Length - 1];
            FinalFrame.NewFrame += FinalFrame_NewFrame;
            FinalFrame.Start();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Start();
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e.KeyData);
        }
        private void OnKeyDown(Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                const string message = "• Author: Michaël André Franiatte.\n\r\n\r• Contact: michael.franiatte@gmail.com.\n\r\n\r• Publisher: https://github.com/michaelandrefraniatte.\n\r\n\r• Copyrights: All rights reserved, no permissions granted.\n\r\n\r• License: Not open source, not free of charge to use.";
                const string caption = "About";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
        }
        private void task(Bitmap img)
        {
            Bitmap bmp = ResizeImage(img, 100, 35);
            html.Clear();
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color col = bmp.GetPixel(x, y);
                    int index = (int)((0.2126 * col.R + 0.7152 * col.G + 0.0722 * col.B) * (AsciiChars.Length - 1) / 255);
                    html.Append(AsciiChars[index]);
                }
                html.Append("<br>");
            }
            webBrowser1.DocumentText = "<pre style=\"font-size: 10px;\">" + html.ToString() + "</pre>";
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            img = (Bitmap)eventArgs.Frame.Clone();
            Task.Run(() => task(img));
        }
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            return new Bitmap(image, width, height);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            if (FinalFrame.IsRunning == true)
                FinalFrame.Stop();
            TimeEndPeriod(1);
        }
    }
}