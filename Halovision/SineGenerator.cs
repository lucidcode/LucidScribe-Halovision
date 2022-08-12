using System;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    internal class SineGenerator
    {
        private readonly double _frequency;
        private short[] _dataBuffer;

        public short[] Data { get { return _dataBuffer; } }

        public SineGenerator(double frequency)
        {
            _frequency = frequency;
            GenerateData();
        }

        private void GenerateData()
        {
            UInt32 sampleRate = 44100;

            uint bufferSize = sampleRate / 10;
            _dataBuffer = new short[bufferSize];

            int amplitude = 32760;

            double timePeriod = (Math.PI * 2 * _frequency) /
               (sampleRate);

            for (uint index = 0; index < bufferSize - 1; index++)
            {
                _dataBuffer[index] = Convert.ToInt16(amplitude *
                   Math.Sin(timePeriod * index));
            }
        }
    }
}
