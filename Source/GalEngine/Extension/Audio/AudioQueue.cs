using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Extension.Audio
{
    public class AudioQueue : ICloneable
    {
        protected struct AudioProperty
        {
            public long Begin;
            public long End;
            public IAudioStream AudioStream;
        }

        protected List<AudioProperty> mAudioList;
        protected int mCurrentAudioIdentity;
        protected long mCurrentAudioAddress;

        protected internal byte[] ReadAudio()
        {
            //if we finish the audio queue, so we return null
            if (mCurrentAudioIdentity == mAudioList.Count) return null;

            var currentAudio = mAudioList[mCurrentAudioIdentity];

            //get the begin of current stream and count of current stream
            long position = Utility.Max(mCurrentAudioAddress, currentAudio.Begin);
            long count = Utility.Min(currentAudio.End - position, currentAudio.AudioStream.StreamBufferSize);

            //the begin of next stream
            mCurrentAudioAddress = position + count;

            //if the begin of next stream is the past of end, we need go to next audio
            //and we need to set the address to zero
            mCurrentAudioIdentity = mCurrentAudioAddress >= currentAudio.End ?
                mCurrentAudioIdentity + 1 : mCurrentAudioIdentity;
            mCurrentAudioAddress = mCurrentAudioAddress >= currentAudio.End ?
                0 : mCurrentAudioAddress;

            return currentAudio.AudioStream.Read(position, (int)count);
        }

        public AudioQueue()
        {
            mAudioList = new List<AudioProperty>();
            mCurrentAudioAddress = 0;
            mCurrentAudioIdentity = 0;
        }

        public void Add(IAudioStream audioStream, long begin, long end)
        {
            mAudioList.Add(new AudioProperty()
            {
                Begin = begin,
                End = end,
                AudioStream = audioStream
            });
        }

        public void SetCurrentAudio(int index)
        {
            mCurrentAudioAddress = 0;
            mCurrentAudioIdentity = index;
        }

        public void Remove(int index)
        {
            mAudioList.RemoveAt(index);
        }

        public void Reset()
        {
            mCurrentAudioAddress = 0;
            mCurrentAudioIdentity = 0;
        }

        public virtual object Clone()
        {
            return new AudioQueue()
            {
                mAudioList = mAudioList,
                mCurrentAudioAddress = mCurrentAudioAddress,
                mCurrentAudioIdentity = mCurrentAudioIdentity
            };
        }
    }
}
