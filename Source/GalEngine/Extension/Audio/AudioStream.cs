using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Extension.Audio
{
    public interface IAudioStream
    {
        long StreamBufferSize { get; set; }

        long Position { get; set; }

        long Length { get; }

        WaveFormat WaveFormat { get; }

        int Read(byte[] buffer, int offset, int count);

        byte[] Read(long position, int count);
    }
 
    public class AudioStream : IAudioStream, IDisposable
    {
        private SharpDX.DataStream mDataStream;

        public long StreamBufferSize { get; set; }

        public long Position { get => mDataStream.Position; set => mDataStream.Position = value; }

        public long Length => mDataStream.Length;

        public WaveFormat WaveFormat { get; }

        public AudioStream(string fileName, long streamBufferSize = 65536)
        {
            var decoder = CSCore.Codecs.CodecFactory.Instance.GetCodec(fileName);
            StreamBufferSize = streamBufferSize;

            WaveFormat = new WaveFormat(
                decoder.WaveFormat.SampleRate,
                decoder.WaveFormat.BitsPerSample,
                decoder.WaveFormat.Channels);

            byte[] data = new byte[decoder.Length];

            decoder.Read(data, 0, data.Length);
            decoder.Dispose();

            mDataStream = SharpDX.DataStream.Create(data, true, false);
        }

        public int Read(byte[] buffer, int offset, int count)
          => mDataStream.Read(buffer, offset, count);

        public byte[] Read(long position, int count)
        {
            byte[] streamData = new byte[count];

            mDataStream.Position = position;
            mDataStream.Read(streamData, 0, streamData.Length);

            return streamData;
        }

        public void Dispose()
        {
            mDataStream.Dispose();
        }
    }
}
