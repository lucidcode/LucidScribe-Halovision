﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public static class Device
    {
        static bool Initialized;
        static bool InitError;
        static List<int> Readings = new List<int>() { 0 };

        public static EventHandler<EventArgs> VisionChanged;

        static VisionForm visionForm;
        public static bool Initialize()
        {
            if (!Initialized & !InitError)
            {
                try
                {
                    loadVisionForm();
                }
                catch (Exception ex)
                {
                    if (!InitError)
                    {
                        MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    InitError = true;
                }

                Initialized = true;
            }
            return true;
        }

        private static void VisionForm_ValueChanged(int value)
        {
            Readings.Add(value);

            if (Readings.Count > 10) Readings.RemoveAt(0);

            if (VisionChanged != null)
            {
                VisionChanged((object)value, null);
            }
        }

        static void loadVisionForm()
        {
            visionForm = new VisionForm();
            visionForm.Reconnect += new VisionForm.ReconnectHanlder(visionForm_Reconnect);
            visionForm.ValueChanged += VisionForm_ValueChanged;
            visionForm.Show();
        }

        static void visionForm_Reconnect()
        {
            visionForm.Close();

            Thread.Sleep(128);
            Application.DoEvents();

            Thread.Sleep(1024);
            Application.DoEvents();

            loadVisionForm();
        }

        public static void Dispose()
        {
            if (Initialized)
            {
                visionForm.Close();
                Initialized = false;
            }
        }

        public static int GetVision()
        {            
            int value = Readings.Sum() / (Readings.Count  / 2);
            return value;
        }

        public static int GetTossThreshold()
        {
            return visionForm.TossThreshold;
        }

        public static bool DetectREM()
        {
            return visionForm.DetectREM;
        }

        public static int GetTossHalfLife()
        {
            return visionForm.TossHalfLife;
        }

        public static int GetTossValue()
        {
            return visionForm.TossValue;
        }

        public static void SetTossValue(int value)
        {
            visionForm.TossValue = value;
        }

        public static int GetEyeMoveMin()
        {
            return visionForm.EyeMoveMin;
        }

        public static int GetEyeMoveMax()
        {
            return visionForm.EyeMoveMax;
        }

        public static int GetIdleTicks()
        {
            return visionForm.IdleTicks;
        }

        public static int GetDashThreshold()
        {
            return visionForm.DashThreshold;
        }

        public static int GetDotThreshold()
        {
            return visionForm.DotThreshold;
        }

        public static bool TCMP
        {
            get
            {
                return visionForm.TCMP;
            }
        }

        public static bool Auralize
        {
            get
            {
                return visionForm.Auralize;
            }
        }

        public static WaveType WaveForm
        {
            get
            {
                return visionForm.WaveForm;
            }
        }
    }

    namespace EyeMin
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            public override string Name
            {
                get
                {
                    return "Eye Move Min";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    return Device.GetEyeMoveMin();
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }
        }
    }

    namespace EyeMax
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            public override string Name
            {
                get
                {
                    return "Eye Move Max";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    return Device.GetEyeMoveMax();
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }
        }
    }

    namespace Toss
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            List<int> history = new List<int>();

            public override string Name
            {
                get
                {
                    return "Toss";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    history.Add(Device.GetVision());
                    if (history.Count > 1000) { history.RemoveAt(0); }

                    int tossValue = 0;
                    int tossThreshold = Device.GetTossThreshold();
                    int tossHalfLife = Device.GetTossHalfLife();
                    for (int i = 0; i < history.Count; i++)
                    {
                        if (history[i] > tossThreshold)
                        {
                            tossValue = 999;
                        }
                        tossValue = tossValue - tossHalfLife;
                    }

                    if (tossValue < 0)
                    {
                        tossValue = 0;
                    }

                    if (tossValue == 999 - tossHalfLife)
                    {
                        if (Device.Auralize)
                        {
                            PlayGlitch();
                        }
                    }

                    Device.SetTossValue(tossValue);
                    return tossValue;
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }

            private void PlayGlitch()
            {
                try
                {
                    MemoryStream mp3file = new MemoryStream(Properties.Resources.glitch);
                    Mp3FileReader mp3reader = new Mp3FileReader(mp3file);
                    var waveOut = new WaveOutEvent();
                    waveOut.Init(mp3reader);
                    waveOut.Play();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    namespace Vision
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            private IWavePlayer player;
            private WaveProvider waveProvider;
            private WaveType waveForm;
            private WaveOutEvent waveOutEvent;

            public override string Name
            {
                get
                {
                    return "Halovision";
                }
            }

            public override bool Initialize()
            {
                waveForm = Device.WaveForm;
                waveProvider = new WaveProvider(waveForm);

                waveOutEvent = new WaveOutEvent();
                waveOutEvent.NumberOfBuffers = 2;
                waveOutEvent.DesiredLatency = 100;
                player = waveOutEvent;
                player.Init(new SampleToWaveProvider(waveProvider));

                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    double vision = Device.GetVision();

                    if (Device.Auralize) {
                        if (player.PlaybackState != PlaybackState.Playing)
                        {
                            player.Play();
                        }
                        Auralize(vision);
                    }
                    else
                    {
                        if (player.PlaybackState == PlaybackState.Playing)
                        {
                            player.Pause();
                        }
                    }

                    return vision;
                }
            }

            private void Auralize(double frequency)
            {
                if (waveForm == WaveType.Sin)
                {
                    if (frequency > 0) frequency += 256;
                }

                if (waveForm != Device.WaveForm)
                {
                    waveForm = Device.WaveForm;
                    waveProvider = new WaveProvider(waveForm);
                    player.Stop();
                    player = waveOutEvent;
                    player.Init(new SampleToWaveProvider(waveProvider));
                    player.Play();
                }
                waveProvider.Frequency = frequency / 2;
            }

            public override void Dispose()
            {
                if (player != null)
                {
                    player.Dispose();
                    player = null;
                }
                Device.Dispose();
            }
        }
    }

    namespace RAW
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.ILluminatedPlugin
        {

            public string Name
            {
                get
                {
                    return "Halovision RAW";
                }
            }

            public bool Initialize()
            {
                try
                {
                    bool initialized = Device.Initialize();
                    Device.VisionChanged += VisionChanged;
                    return initialized;
                }
                catch (Exception ex)
                {
                    throw (new Exception("The '" + Name + "' plugin failed to initialize: " + ex.Message));
                }
            }

            public event Interface.SenseHandler Sensed;
            public void VisionChanged(object sender, EventArgs e)
            {
                if (ClearTicks)
                {
                    ClearTicks = false;
                    TickCount = "";
                }
                int value = (int)sender;
                if (value > 999) value = 999;
                TickCount += value + ",";

                if (ClearBuffer)
                {
                    ClearBuffer = false;
                    BufferData = "";
                }
                BufferData += value + ",";
            }

            public void Dispose()
            {
                Device.VisionChanged -= VisionChanged;
                Device.Dispose();
            }

            public Boolean isEnabled = false;
            public Boolean Enabled
            {
                get
                {
                    return isEnabled;
                }
                set
                {
                    isEnabled = value;
                }
            }

            public Color PluginColor = Color.White;
            public Color Color
            {
                get
                {
                    return Color;
                }
                set
                {
                    Color = value;
                }
            }

            private Boolean ClearTicks = false;
            public String TickCount = "";
            public String Ticks
            {
                get
                {
                    ClearTicks = true;
                    return TickCount;
                }
                set
                {
                    TickCount = value;
                }
            }

            private Boolean ClearBuffer = false;
            public String BufferData = "";
            public String Buffer
            {
                get
                {
                    ClearBuffer = true;
                    return BufferData;
                }
                set
                {
                    BufferData = value;
                }
            }

            int lastHour;
            public int LastHour
            {
                get
                {
                    return lastHour;
                }
                set
                {
                    lastHour = value;
                }
            }
        }
    }

    namespace REM
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            public override string Name
            {
                get
                {
                    return "vREM";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            List<int> history = new List<int>();
            int previousValue = 0;

            public override double Value
            {
                get
                {
                    // Update the mem list
                    if (Device.GetTossValue() > 0)
                    {
                        history.Clear();
                        return 0;
                    }

                    int eyeMoveMin = Device.GetEyeMoveMin();
                    int eyeMoveMax = Device.GetEyeMoveMax();
                    int idleTicks = Device.GetIdleTicks();

                    if (Device.DetectREM())
                    {
                        history.Add(Device.GetVision());
                    }
                    else
                    {
                        history.Add(0);
                    }
                    
                    if (history.Count > 768) { history.RemoveAt(0); }

                    // Check for blinks
                    int intBlinks = 0;
                    bool boolBlinking = false;

                    int intBelow = 0;
                    int intAbove = 0;

                    bool boolDreaming = false;
                    for (int i = 0; i < history.Count; i++)
                    {
                        double value = history[i];

                        bool overMax = false;
                        for (int l = i; l > 0 & l > i - 10; l--)
                        {
                            if (history[l] > eyeMoveMax)
                            {
                                overMax = true;
                                break;
                            }
                        }
                        for (int n = i; n < history.Count & n < i + 10; n++)
                        {
                            if (history[n] > eyeMoveMax)
                            {
                                overMax = true;
                                break;
                            }
                        }

                        if (!overMax)
                        {
                            if (value > eyeMoveMin & value < eyeMoveMax)
                            {
                                intAbove += 1;
                                intBelow = 0;
                            }
                            else
                            {
                                intBelow += 1;
                                intAbove = 0;
                            }
                        }

                        if (!boolBlinking)
                        {
                            if (intAbove >= 1)
                            {
                                boolBlinking = true;
                                intBlinks += 1;
                                intAbove = 0;
                                intBelow = 0;
                            }
                        }
                        else
                        {
                            if (intBelow >= idleTicks)
                            {
                                boolBlinking = false;
                                intBelow = 0;
                                intAbove = 0;
                            }
                            else
                            {
                                if (intAbove >= 12)
                                {
                                    // reset
                                    boolBlinking = false;
                                    intBlinks = 0;
                                    intBelow = 0;
                                    intAbove = 0;
                                }
                            }
                        }

                        if (intBlinks > 8)
                        {
                            boolDreaming = true;
                            break;
                        }

                        if (intAbove > 12)
                        { // reset
                            boolBlinking = false;
                            intBlinks = 0;
                            intBelow = 0;
                            intAbove = 0; ;
                        }
                        if (intBelow > 256)
                        { // reset
                            boolBlinking = false;
                            intBlinks = 0;
                            intBelow = 0;
                            intAbove = 0; ;
                        }
                    }

                    if (boolDreaming)
                    { return 888; }

                    if (intBlinks > 10) { intBlinks = 10; }

                    if (previousValue != intBlinks)
                    {
                        if (previousValue < intBlinks)
                        {
                            if (Device.Auralize) PluckString();
                        }
                        previousValue = intBlinks;
                    }

                    return intBlinks * 100;
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }

            private void PluckString()
            {
                try
                {
                    MemoryStream mp3file = new MemoryStream(Properties.Resources.guitar);
                    Mp3FileReader mp3reader = new Mp3FileReader(mp3file);
                    var waveOut = new WaveOutEvent();
                    waveOut.Init(mp3reader);
                    waveOut.Play();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    namespace Dot
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            public override string Name
            {
                get
                {
                    return "Dot";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    return Device.GetDotThreshold();
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }
        }
    }

    namespace Dash
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            public override string Name
            {
                get
                {
                    return "Dash";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            public override double Value
            {
                get
                {
                    return Device.GetDashThreshold();
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }
        }
    }

}
