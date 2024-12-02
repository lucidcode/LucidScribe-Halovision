using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public static class Device
    {
        static bool Initialized;
        static bool InitError;
        static int Variance;
        static List<int> Variances = new List<int>() { 0 };

        public static EventHandler<EventArgs> VisionChanged;
        static Process VisionProcess;

        static int TossThreshold;
        static bool DetectREM = true;
        static int TossHalfLife;
        static int TossValue;
        static int EyeMoveMin;
        static int EyeMoveMax;
        static int IdleTicks;
        static int DashThreshold;
        static int DotThreshold;
        static bool TCMP;
        static bool Auralize;
        static WaveType WaveForm;

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

        static void loadVisionForm()
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "lucidcode.LucidScribe.Plugin.Halovision.exe";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            VisionProcess = Process.Start(processStartInfo);
            VisionProcess.OutputDataReceived += VisionProcess_OutputDataReceived;
            VisionProcess.ErrorDataReceived += VisionProcess_ErrorDataReceived;
            VisionProcess.BeginErrorReadLine();
            VisionProcess.BeginOutputReadLine();
        }

        private static void VisionProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var parts = e.Data.Split(':');
            if (parts.Length == 1)
            {
                var value = 0;

                try
                {
                    value = Convert.ToInt32(e.Data);
                }
                catch (Exception ex) { }

                Variance = value;
                Variances.Add(value);

                if (Variances.Count > 4) Variances.RemoveAt(0);
                if (VisionChanged != null)
                {
                    VisionChanged((object)value, null);
                }

                return;
            }

            if (parts[0] == "TossThreshold")
            {
                TossThreshold = Convert.ToInt32(parts[1]);
            }
            if (parts[0] == "DetectREM") DetectREM = parts[1] == "True" ? true : false;
            if (parts[0] == "TossHalfLife") TossHalfLife = Convert.ToInt32(parts[1]);
            //if (parts[0] == "TossValue") TossValue = Convert.ToInt32(parts[1]);
            if (parts[0] == "EyeMoveMin") EyeMoveMin = Convert.ToInt32(parts[1]);
            if (parts[0] == "EyeMoveMax") EyeMoveMax = Convert.ToInt32(parts[1]);
            if (parts[0] == "IdleTicks") IdleTicks = Convert.ToInt32(parts[1]);
            if (parts[0] == "DashThreshold") DashThreshold = Convert.ToInt32(parts[1]);
            if (parts[0] == "DotThreshold") DotThreshold = Convert.ToInt32(parts[1]);
            if (parts[0] == "TCMP") TCMP = parts[1] == "True" ? true : false;
            if (parts[0] == "Auralize") Auralize = parts[1] == "True" ? true : false;
            if (parts[0] == "WaveForm")
            {
                switch (parts[1])
                {
                    case "Sin":
                        WaveForm = WaveType.Sin;
                        break;
                    case "SawTooth":
                        WaveForm = WaveType.SawTooth;
                        break;
                    case "Sqaure":
                        WaveForm = WaveType.Square;
                        break;
                    case "Triangle":
                        WaveForm = WaveType.Triangle;
                        break;
                    case "Sweep":
                        WaveForm = WaveType.Sweep;
                        break;
                    case "Pink":
                        WaveForm = WaveType.Pink;
                        break;
                    default:
                        WaveForm = WaveType.Triangle;
                        break;
                }
            }
        }

        private static void VisionProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
        }

        public static void Dispose()
        {
            if (Initialized)
            {
                VisionProcess.CloseMainWindow();
                VisionProcess.Close();
                Initialized = false;
            }
        }

        public static int GetVariances()
        {
            if (Variances.Count == 0) return 0;

            try
            {
                int value = Variances.Sum() / (Variances.Count);
                return value;
            }
            catch  (Exception ex)
            {
                return 0;
            }
        }

        public static int GetVariance()
        {
            return Variance;
        }

        public static int GetTossThreshold()
        {
            return TossThreshold;
        }

        public static bool GetDetectREM()
        {
            return DetectREM;
        }

        public static int GetTossHalfLife()
        {
            return TossHalfLife;
        }

        public static int GetTossValue()
        {
            return TossValue;
        }

        public static void SetTossValue(int value)
        {
            TossValue = value;
        }

        public static int GetEyeMoveMin()
        {
            return EyeMoveMin;
        }

        public static int GetEyeMoveMax()
        {
            return EyeMoveMax;
        }

        public static int GetIdleTicks()
        {
            return IdleTicks;
        }

        public static int GetDashThreshold()
        {
            return DashThreshold;
        }

        public static int GetDotThreshold()
        {
            return DotThreshold;
        }

        public static bool GetTCMP
        {
            get
            {
                return TCMP;
            }
        }

        public static bool GetAuralize
        {
            get
            {
                return Auralize;
            }
        }

        public static WaveType GetWaveForm
        {
            get
            {
                return WaveForm;
            }
        }
    }

    namespace EyeMin
    {
        public class PluginHandler : Interface.LucidPluginBase
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
        public class PluginHandler : Interface.LucidPluginBase
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
        public class PluginHandler : Interface.LucidPluginBase
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
                    history.Add(Device.GetVariances());
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
                        if (Device.GetAuralize)
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
        public class PluginHandler : Interface.LucidPluginBase
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
                waveForm = Device.GetWaveForm;
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
                    double vision = Device.GetVariances();

                    if (Device.GetAuralize)
                    {
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

                if (waveForm != Device.GetWaveForm)
                {
                    waveForm = Device.GetWaveForm;
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
        public class PluginHandler : Interface.ILluminatedPlugin
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
        public class PluginHandler : Interface.LucidPluginBase
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

                    if (Device.GetDetectREM())
                    {
                        history.Add(Device.GetVariances());
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
                            if (Device.GetAuralize) PluckString();
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
        public class PluginHandler : Interface.LucidPluginBase
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
        public class PluginHandler : Interface.LucidPluginBase
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
