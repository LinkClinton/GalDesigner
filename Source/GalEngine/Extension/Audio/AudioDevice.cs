using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Extension.Audio
{
    public static class AudioDevice
    {
        private static readonly SharpDX.XAudio2.XAudio2 mEngine;
        private static readonly SharpDX.XAudio2.MasteringVoice mDevice;

        internal static SharpDX.XAudio2.XAudio2 XAudio => mEngine;
        internal static SharpDX.XAudio2.MasteringVoice MasteringVoice => mDevice;

        static AudioDevice()
        {
            mEngine = new SharpDX.XAudio2.XAudio2();
            mDevice = new SharpDX.XAudio2.MasteringVoice(mEngine);
        }

        public static void Terminate()
        {
            mDevice.Dispose();
            mEngine.Dispose();
        }
    }
}
