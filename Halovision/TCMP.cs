using lucidcode.LucidScribe.TCMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    namespace TCMP
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase, lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin
        {
            public override string Name
            {
                get
                {
                    return "TCMP";
                }
            }

            public override bool Initialize()
            {
                return Device.Initialize();
            }

            private static String Morse = "";
            Dictionary<char, String> Code = new Dictionary<char, String>()
            {
                {'A' , ".-"},
                {'B' , "-..."},
                {'C' , "-.-."},
                {'D' , "-.."},
                {'E' , "."},
                {'F' , "..-."},
                {'G' , "--."},
                {'H' , "...."},
                {'I' , ".."},
                {'J' , ".---"},
                {'K' , "-.-"},
                {'L' , ".-.."},
                {'M' , "--"},
                {'N' , "-."},
                {'O' , "---"},
                {'P' , ".--."},
                {'Q' , "--.-"},
                {'R' , ".-."},
                {'S' , "..."},
                {'T' , "-"},
                {'U' , "..-"},
                {'V' , "...-"},
                {'W' , ".--"},
                {'X' , "-..-"},
                {'Y' , "-.--"},
                {'Z' , "--.."},
                {'0' , "-----"},
                {'1' , ".----"},
                {'2' , "..----"},
                {'3' , "...--"},
                {'4' , "....-"},
                {'5' , "....."},
                {'6' , "-...."},
                {'7' , "--..."},
                {'8' , "---.."},
                {'9' , "----."},
            };

            List<int> history = new List<int>();
            Boolean FirstTick = false;
            Boolean SpaceSent = true;
            int TicksSinceSpace = 0;
            Boolean Started = false;
            int PreliminaryTicks = 0;

            public override double Value
            {
                get
                {
                    if (!Device.TCMP) { return 0; }

                    double tempValue = Device.GetVision();
                    if (tempValue > 999) { tempValue = 999; }
                    if (tempValue < 0) { tempValue = 0; }

                    if (!Started)
                    {
                        PreliminaryTicks++;
                        if (PreliminaryTicks > 10)
                        {
                            Started = true;
                        }

                        return 0;
                    }

                    int signalLength = 0;
                    int dotHeight = Device.GetDotThreshold();
                    int dashHeight = Device.GetDashThreshold();

                    String signal = "";

                    if (!FirstTick && (tempValue > dotHeight))
                    {
                        history.Add(Convert.ToInt32(tempValue));
                    }

                    if (!FirstTick && history.Count > 0)
                    {
                        history.Add(Convert.ToInt32(tempValue));
                    }

                    if (FirstTick && (tempValue > dotHeight))
                    {
                        FirstTick = false;
                    }

                    if (!SpaceSent & history.Count == 0)
                    {
                        TicksSinceSpace++;
                        if (TicksSinceSpace > 40)
                        {
                            // Send the space key
                            Morse = " ";
                            SendKeys.Send(" ");
                            SpaceSent = true;
                            TicksSinceSpace = 0;
                        }
                    }

                    if (!FirstTick && history.Count > 40)
                    {
                        int nextOffset = 0;
                        do
                        {
                            int peakValue = 0;
                            for (int i = nextOffset; i < history.Count; i++)
                            {
                                for (int x = i; x < history.Count; x++)
                                {
                                    if (history[x] > peakValue)
                                    {
                                        peakValue = history[x];
                                    }

                                    if (history[x] < dotHeight / 4)
                                    {
                                        nextOffset = x + 1;
                                        break;
                                    }

                                    if (x == history.Count - 1)
                                    {
                                        nextOffset = -1;
                                    }
                                }

                                if (peakValue >= dashHeight)
                                {
                                    signal += "-";
                                    signalLength++;
                                    break;
                                }
                                else if (peakValue >= dotHeight)
                                {
                                    signal += ".";
                                    signalLength++;
                                    break;
                                }

                                if (i >= history.Count - 1)
                                {
                                    nextOffset = -1;
                                }

                            }

                            if (nextOffset < 0 | nextOffset == history.Count)
                            {
                                break;
                            }

                        } while (true);

                        history.RemoveAt(0);

                        // Check if the signal is morse
                        try
                        {
                            // Make sure that we have a signal
                            if (signal != "")
                            {
                                var myValue = Code.First(x => x.Value == signal);
                                Morse = myValue.Key.ToString();
                                SendKeys.Send(myValue.Key.ToString());
                                signal = "";
                                history.Clear();
                                SpaceSent = false;
                                TicksSinceSpace = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            String err = ex.Message;
                        }
                    }

                    if (history.Count > 0)
                    { return 888; }

                    return 0;
                }
            }

            string lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin.MorseCode
            {
                get
                {
                    String temp = Morse;
                    Morse = "";
                    return temp;
                }
            }

            public override void Dispose()
            {
                Device.Dispose();
            }

        }
    }
}
