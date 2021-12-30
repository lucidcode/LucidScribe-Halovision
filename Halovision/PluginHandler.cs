using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public static class Device
    {
        static bool Initialized;
        static bool InitError;
        static Thread liveThread;
        static int m_dblValue = 0;
        static int m_dblREMValue = 0;

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
            m_dblValue = value;
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
                visionForm.Disconnect();
                Initialized = false;
            }
        }

        public static Double GetVision()
        {
            return m_dblValue;
        }

        public static Double GetREM()
        {
            return m_dblREMValue;
        }
    }

    namespace Vision
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase
        {
            private double m_dblValue = 256;

            public override string Name
            {
                get
                {
                    return "Halovision";
                }
            }

            public override bool Initialize()
            {
                try
                {
                    return Device.Initialize();
                }
                catch (Exception ex)
                {
                    throw (new Exception("The '" + Name + "' plugin failed to initialize: " + ex.Message));
                }
            }

            public override double Value
            {
                get
                {
                    double tempValue = Device.GetVision();
                    if (tempValue > 999) { tempValue = 999; }
                    if (tempValue < 0) { tempValue = 0; }
                    return tempValue;
                }
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
                try
                {
                    return Device.Initialize();
                }
                catch (Exception ex)
                {
                    throw (new Exception("The '" + Name + "' plugin failed to initialize: " + ex.Message));
                }
            }

            List<int> m_arrHistory = new List<int>();

            public override double Value
            {
                get
                {
                    // Update the mem list
                    m_arrHistory.Add(Convert.ToInt32(Device.GetVision()));
                    if (m_arrHistory.Count > 768) { m_arrHistory.RemoveAt(0); }

                    // Check for blinks
                    int intBlinks = 0;
                    bool boolBlinking = false;

                    int intBelow = 0;
                    int intAbove = 0;

                    bool boolDreaming = false;
                    for (int i = 0; i < m_arrHistory.Count; i++)
                    {
                        Double dblValue = m_arrHistory[i];

                        // Check if the last 10 or next 10 were 1000
                        int lastOrNextOver1000 = 0;
                        for (int l = i; l > 0 & l > i - 10; l--)
                        {
                            if (m_arrHistory[l] > 999)
                            {
                                lastOrNextOver1000++;
                            }
                        }
                        for (int n = i; n < m_arrHistory.Count & n < i + 10; n++)
                        {
                            if (m_arrHistory[n] > 999)
                            {
                                lastOrNextOver1000++;
                            }
                        }

                        if (lastOrNextOver1000 == 0)
                        {
                            if (dblValue > 8 & dblValue < 999)
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

                        if (lastOrNextOver1000 > 10)
                        {
                            intBlinks = 0;
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
                            if (intBelow >= 4)
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
                            MessageBox.Show("2");
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
}
