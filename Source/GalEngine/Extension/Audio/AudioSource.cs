using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Extension.Audio
{
    public class AudioSource : IDisposable
    {
        private const int mBufferCount = 3;

        private SharpDX.XAudio2.SourceVoice mSourceVoice;

        private Task mStreamBufferTask;
        private SharpDX.DataBuffer[] mStreamBuffer;
        private int mCurrentBuffer;

        private AudioQueue mAudioQueue;
        
        private void SubmitAudioBuffer()
        {
            var streamData = mAudioQueue.ReadAudio();

            if (streamData == null) return;

            if (mStreamBuffer[mCurrentBuffer] == null || 
                mStreamBuffer[mCurrentBuffer].Size != streamData.Length)
            {
                mStreamBuffer[mCurrentBuffer]?.Dispose();
                mStreamBuffer[mCurrentBuffer] = new SharpDX.DataBuffer(streamData.Length);
            }

            mStreamBuffer[mCurrentBuffer].Set(0, streamData);

            var buffer = new SharpDX.XAudio2.AudioBuffer(mStreamBuffer[mCurrentBuffer])
            {
                Flags = SharpDX.XAudio2.BufferFlags.None
            };

            mSourceVoice.SubmitSourceBuffer(buffer, null);

            mCurrentBuffer = (mCurrentBuffer + 1) % mBufferCount;
        }

        public WaveFormat WaveFormat { get; }

        public AudioSource(WaveFormat format)
        {
            WaveFormat = format;

            mSourceVoice = new SharpDX.XAudio2.SourceVoice(
                AudioDevice.XAudio,
                new SharpDX.Multimedia.WaveFormat(
                    WaveFormat.SampleRate,
                    WaveFormat.Bits,
                    WaveFormat.Channels));

            mStreamBuffer = new SharpDX.DataBuffer[mBufferCount];
            mCurrentBuffer = 0;

            mSourceVoice.BufferEnd += (IntPtr obj) =>
              {
                  mStreamBufferTask?.Wait();

                  mStreamBufferTask?.Dispose();
                  mStreamBufferTask = new Task(SubmitAudioBuffer);
                  mStreamBufferTask.Start();
              };
        }

        public virtual void SubmitAudioQueue(AudioQueue queue)
        {
            Stop();

            mSourceVoice.FlushSourceBuffers();

            mCurrentBuffer = 0;
            mAudioQueue = queue.Clone() as AudioQueue;
        }

        public virtual void Start()
        {
            SubmitAudioBuffer();
            SubmitAudioBuffer();

            mSourceVoice.Start();
        }

        public virtual void Stop()
        {
            mSourceVoice.Stop();

            mStreamBufferTask?.Wait();
            mStreamBufferTask?.Dispose();
        }

        public virtual void Dispose()
        {
            SharpDX.Utilities.Dispose(ref mSourceVoice);
        }
    }
}
