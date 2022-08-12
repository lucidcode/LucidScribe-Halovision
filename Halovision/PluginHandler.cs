using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public static class Device
    {
        static bool Initialized;
        static bool InitError;
        static int Value = 0;

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
            Value = value;
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
            return Value;
        }

        public static int GetTossThreshold()
        {
            return visionForm.TossThreshold;
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
                    history.Add(Convert.ToInt32(Device.GetVision()));
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

                    Device.SetTossValue(tossValue);
                    return tossValue;
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }
        }
    }

    namespace Vision
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            SoundPlayer sound = new SoundPlayer();

            public override string Name
            {
                get
                {
                    return "Halovision";
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
                    double vision = Device.GetVision();
                    if (vision > 999) { vision = 999; }
                    if (vision < 0) { vision = 0; }

                    if (Device.Auralize && vision > 0) {
                        Auralize(vision);
                    }
                    
                    return vision;
                }
            }

            private void Auralize(double frequency)
            {
                var header = new WaveHeader();
                var format = new FormatChunk();
                var austioChunk = new DataChunk();
                var sineData = new SineGenerator(frequency);

                austioChunk.AddSampleData(sineData.Data, sineData.Data);
                header.FileLength += format.Length() + austioChunk.Length();

                var soundBytes = new List<byte>();
                soundBytes.AddRange(header.GetBytes());
                soundBytes.AddRange(format.GetBytes());
                soundBytes.AddRange(austioChunk.GetBytes());

                sound.Stream = new MemoryStream(soundBytes.ToArray());
                sound.Play();
            }

            public override void Dispose()
            {
                Device.Dispose();
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

                    history.Add(Convert.ToInt32(Device.GetVision()));
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
                    return intBlinks * 100;
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
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
