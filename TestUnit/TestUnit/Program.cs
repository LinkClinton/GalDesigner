using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalEngine;
using GalEngine.Extension.Audio;

namespace TestUnit
{
    class Program
    {
        static void Main(string[] args)
        {
            GameSystems.Initialize(new GameStartInfo
            {
                Window = new WindowInfo()
                {
                    Name = "TestUnit",
                    Size = new Size(1920, 1080)
                }
            });


            var stream0 = new AudioStream(@"C:\Users\LinkC\source\love.mp3");
            var stream1 = new AudioStream(@"C:\Users\LinkC\source\free.wav");

            AudioQueue audioQueue0 = new AudioQueue();
            AudioQueue audioQueue1 = new AudioQueue();


            audioQueue0.Add(stream0, 0, stream0.Length);
            audioQueue1.Add(stream1, 0, stream1.Length);


            AudioSource audioSource0 = new AudioSource(stream0.WaveFormat);
            AudioSource audioSource1 = new AudioSource(stream1.WaveFormat);

            audioSource0.SubmitAudioQueue(audioQueue0);
            audioSource1.SubmitAudioQueue(audioQueue1);

            audioSource0.Start();
            audioSource1.Start();


            GameSystems.RunLoop();

            AudioDevice.Terminate();
        }
    }
}
