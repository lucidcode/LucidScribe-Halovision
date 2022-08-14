using NAudio.Wave;
using System;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    internal class SineWaveProvider : ISampleProvider
    {
        private readonly float[] waveTable;
        private double phase;
        private double currentPhaseStep;
        private double targetPhaseStep;
        private double frequency;
        private double phaseStepDelta;
        private bool seekFreq;

        public SineWaveProvider(int sampleRate = 44100)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            waveTable = new float[sampleRate];
            for (int index = 0; index < sampleRate; ++index)
                waveTable[index] = (float)Math.Sin(2 * Math.PI * (double)index / sampleRate);
            Frequency = 1000f;
            Volume = 1f;
            PortamentoTime = 0.1; 
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
