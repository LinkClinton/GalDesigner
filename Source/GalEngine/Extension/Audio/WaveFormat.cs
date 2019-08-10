using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Extension.Audio
{
    public class WaveFormat
    {
        public int SampleRate { get; }
        public int Bits { get; }
        public int Channels { get; }

        public WaveFormat(int sampleRate, int bits, int channels)
        {
            SampleRate = sampleRate;
            Bits = bits;
            Channels = channels;
        }

    

        public static WaveFormat FromFile(string fileName)
        {
            //decode the audio file
            var decoder = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName);

            //get format
            var waveFormat = new WaveFormat(
                decoder.WaveFormat.SampleRate,
                decoder.WaveFormat.BitsPerSample,
                decoder.WaveFormat.Channels);

            decoder.Dispose();

            return waveFormat;
        }
    }
}
