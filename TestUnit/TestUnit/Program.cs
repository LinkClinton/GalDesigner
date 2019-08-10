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

            var stream = new AudioStream(@"C:\Users\LinkC\source\love.mp3");
           
            AudioQueue audioQueue = new AudioQueue();

            audioQueue.Add(stream, 0, stream.Length);

            AudioSource audioSource = new AudioSource(stream.WaveFormat);

            audioSource.SubmitAudioQueue(audioQueue);    
            audioSource.Start();



            GameSystems.RunLoop();

            AudioDevice.Terminate();
        }
    }
}
