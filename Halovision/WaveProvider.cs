using NAudio.Wave;
using System;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    internal class WaveProvider : ISampleProvider
    {
        private readonly float[] waveTable;
        private double phase;
        private double currentPhaseStep;
        private double targetPhaseStep;
        private double frequency;
        private double phaseStepDelta;
        private bool seekFreq;
        private Random random = new Random();

        public WaveProvider(WaveType waveType)
        {
            int sampleRate = 44100;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            waveTable = new float[sampleRate];
            double[] pinkNoiseBuffer = new double[7];
            for (int index = 0; index < sampleRate; ++index)
            {
                switch (waveType)
                {
                    case WaveType.Sin:
                        waveTable[index] = (float)Math.Sin(2 * Math.PI * index / sampleRate);
                        break;
                    case WaveType.Square:
                        waveTable[index] = ((float)Math.Sin(2 * Math.PI * index / sampleRate)) > 0 ? 1 : -1;
                        break;
                    case WaveType.Triangle:
                        waveTable[index] = (float)(Math.Asin(Math.Sin((2 * Math.PI * index / sampleRate))) * (2.0 / Math.PI));
                        break;
                    case WaveType.SawTooth:
                        waveTable[index] = (float)index / sampleRate;
                        break;

                    case WaveType.Sweep:
                        waveTable[index] = (float)(Math.Sin(2 * Math.PI * (double)index / sampleRate) +
                            Math.Sin(4 * Math.PI * (double)index / sampleRate) +
                            Math.Sin(6 * Math.PI * (double)index / sampleRate) +
                            Math.Sin(8 * Math.PI * (double)index / sampleRate));
                        break;

                    case WaveType.Pink:
                        double white = 2 * random.NextDouble() - 1;
                        pinkNoiseBuffer[0] = 0.99886 * pinkNoiseBuffer[0] + white * 0.0555179;
                        pinkNoiseBuffer[1] = 0.99332 * pinkNoiseBuffer[1] + white * 0.0750759;
                        pinkNoiseBuffer[2] = 0.96900 * pinkNoiseBuffer[2] + white * 0.1538520;
                        pinkNoiseBuffer[3] = 0.86650 * pinkNoiseBuffer[3] + white * 0.3104856;
                        pinkNoiseBuffer[4] = 0.55000 * pinkNoiseBuffer[4] + white * 0.5329522;
                        pinkNoiseBuffer[5] = -0.7616 * pinkNoiseBuffer[5] - white * 0.0168980;
                        double pink = pinkNoiseBuffer[0] + pinkNoiseBuffer[1] + pinkNoiseBuffer[2] + pinkNoiseBuffer[3] + pinkNoiseBuffer[4] + pinkNoiseBuffer[5] + pinkNoiseBuffer[6] + white * 0.5362;
                        pinkNoiseBuffer[6] = white * 0.115926;
                        waveTable[index] = (float)(pink / 5);
                        break;
                    default:
                        waveTable[index] = 0;
                        break;
                }
            }

            Frequency = 1000f;
            Volume = 0.5f;
            PortamentoTime = 0.02; 
        }

        public double PortamentoTime { get; set; }

        public double Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
                seekFreq = true;
            }
        }

        public float Volume { get; set; }

        public WaveFormat WaveFormat { get; private set; }

        public int Read(float[] buffer, int offset, int count)
        {
            if (seekFreq)
            {
                targetPhaseStep = waveTable.Length * (frequency / WaveFormat.SampleRate);
                phaseStepDelta = (targetPhaseStep - currentPhaseStep) / (WaveFormat.SampleRate * PortamentoTime);
                seekFreq = false;
            }
            var vol = Volume;
            for (int n = 0; n < count; ++n)
            {
                int waveTableIndex = (int)phase % waveTable.Length;
                buffer[n + offset] = waveTable[waveTableIndex] * vol;
                phase += currentPhaseStep;
                if (phase > waveTable.Length)
                    phase -= waveTable.Length;
                if (currentPhaseStep != targetPhaseStep)
                {
                    currentPhaseStep += phaseStepDelta;
                    if (phaseStepDelta > 0.0 && currentPhaseStep > targetPhaseStep)
                        currentPhaseStep = targetPhaseStep;
                    else if (phaseStepDelta < 0.0 && currentPhaseStep < targetPhaseStep)
                        currentPhaseStep = targetPhaseStep;
                }
            }
            return count;
        }
    }
}
