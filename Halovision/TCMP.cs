using lucidcode.LucidScribe.TCMP;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    namespace TCMP
    {
        public class PluginHandler : lucidcode.LucidScribe.Interface.LucidPluginBase, lucidcode.LucidScribe.TCMP.ITransConsciousnessPlugin
        {
            SoundPlayer dotSoundPlayer;
            SoundPlayer dashSoundPlayer;

            public override string Name
            {
                get
                {
                    return "TCMP";
                }
            }

            public override bool Initialize()
            {
                dotSoundPlayer = new SoundPlayer(Properties.Resources.dot);
                dashSoundPlayer = new SoundPlayer(Properties.Resources.dash);

                return Device.Initialize();
            }

            private static String Morse = "";
            Dictionary<char, String> Code = new Dictionary<char, String>()
            {
                {'a' , ".-"},
                {'b' , "-..."},
                {'c' , "-.-."},
                {'d' , "-.."},
                {'e' , "."},
                {'f' , "..-."},
                {'g' , "--."},
                {'h' , "...."},
                {'i' , ".."},
                {'j' , ".---"},
                {'k' , "-.-"},
                {'l' , ".-.."},
                {'m' , "--"},
                {'n' , "-."},
                {'o' , "---"},
                {'p' , ".--."},
                {'q' , "--.-"},
                {'r' , ".-."},
                {'s' , "..."},
                {'t' , "-"},
                {'u' , "..-"},
                {'v' , "...-"},
                {'w' , ".--"},
                {'x' , "-..-"},
                {'y' , "-.--"},
                {'z' , "--.."},
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
            List<int> soundHistory = new List<int>();
            Boolean SpaceSent = true;
            int TicksSinceSpace = 0;
            Boolean Started = false;
            int PreliminaryTicks = 0;

            public override double Value
            {
                get
                {
                    if (!Device.TCMP) { return 0; }

                    int visionValue = Device.GetVision();
                    if (visionValue > 999) { visionValue = 999; }
                    if (visionValue < 0) { visionValue = 0; }

                    if (!Started)
                    {
                        // Ignore any spike during startup
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

                    soundHistory.Add(visionValue);
                    if (soundHistory.Count > 6)
                    {
                        int peakValue = 0;

                        for (int i = 0; i < soundHistory.Count; i++)
                        {
                            if (soundHistory[i] > peakValue)
                            {
                                peakValue = soundHistory[i];
                            }
                        }

                        if (soundHistory[soundHistory.Count - 1] < dotHeight / 4 && soundHistory[soundHistory.Count - 2] < dotHeight / 4)
                        {
                            if (peakValue >= dashHeight)
                            {
                                dashSoundPlayer.Play();
                                soundHistory.Clear();
                            }
                            else if (peakValue >= dotHeight)
                            {
                                dotSoundPlayer.Play();
                                soundHistory.Clear();
                            }
                        }
                    }

                    if ((visionValue >= dotHeight) || history.Count > 0)
                    {
                        history.Add(visionValue);
                    }

                    if (!SpaceSent & history.Count == 0)
                    {
                        TicksSinceSpace++;
                        if (TicksSinceSpace > 34)
                        {
                            // Send the space key
                            Morse = " ";
                            SendKeys.Send(" ");
                            SpaceSent = true;
                            TicksSinceSpace = 0;
                            history.Clear();
                        }
                    }

                    if (history.Count > 34)
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

                                    if (history[x] < dotHeight / 4 && history[x - 1] < dotHeight / 4)
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
                                var letter = myValue.Key.ToString();
                                SendKeys.Send(letter);
                                signal = "";
                                history.Clear();
                                SpaceSent = false;
                                TicksSinceSpace = 0;

                                if (Device.Auralize)
                                {
                                    SpeakLetter(letter);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            String err = ex.Message;
                        }
                    }

                    if (history.Count > 0)
                    { return 820; }

                    return 0;
                }
            }

            private void SpeakLetter(String letter)
            {
                try
                {
                    MemoryStream mp3file = GetResourceStream(letter);
                    Mp3FileReader mp3reader = new Mp3FileReader(mp3file);
                    var waveOut = new WaveOutEvent();
                    waveOut.Init(mp3reader);
                    waveOut.Play();
                }
                catch (Exception ex)
                {

                }
            }

            private MemoryStream GetResourceStream(String letter)
            {
                switch (letter.ToLower())
                {
                    case "a": return new MemoryStream(Properties.Resources.a);
                    case "b": return new MemoryStream(Properties.Resources.b);
                    case "c": return new MemoryStream(Properties.Resources.c);
                    case "d": return new MemoryStream(Properties.Resources.d);
                    case "e": return new MemoryStream(Properties.Resources.e);
                    case "f": return new MemoryStream(Properties.Resources.f);
                    case "g": return new MemoryStream(Properties.Resources.g);
                    case "h": return new MemoryStream(Properties.Resources.h);
                    case "i": return new MemoryStream(Properties.Resources.i);
                    case "j": return new MemoryStream(Properties.Resources.j);
                    case "k": return new MemoryStream(Properties.Resources.k);
                    case "l": return new MemoryStream(Properties.Resources.l);
                    case "m": return new MemoryStream(Properties.Resources.m);
                    case "n": return new MemoryStream(Properties.Resources.n);
                    case "o": return new MemoryStream(Properties.Resources.o);
                    case "p": return new MemoryStream(Properties.Resources.p);
                    case "q": return new MemoryStream(Properties.Resources.q);
                    case "r": return new MemoryStream(Properties.Resources.r);
                    case "s": return new MemoryStream(Properties.Resources.s);
                    case "t": return new MemoryStream(Properties.Resources.t);
                    case "u": return new MemoryStream(Properties.Resources.u);
                    case "v": return new MemoryStream(Properties.Resources.v);
                    case "w": return new MemoryStream(Properties.Resources.w);
                    case "x": return new MemoryStream(Properties.Resources.x);
                    case "y": return new MemoryStream(Properties.Resources.y);
                    case "z": return new MemoryStream(Properties.Resources.z);
                    default: return new MemoryStream(Properties.Resources.a);
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
