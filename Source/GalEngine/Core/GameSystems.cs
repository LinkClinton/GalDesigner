using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class WindowInfo
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public Size Size { get; set; }
    }

    public class GameStartInfo
    {
        public string Name { get; set; }
        public WindowInfo Window { get; set; }
        public GpuAdapter Adapter { get; set; }
    }

    public static class GameSystems
    {  
        private static PresentRender PresentRender { get; set; }
        public static EngineWindow EngineWindow { get; private set; }
        public static GpuDevice GpuDevice { get; private set; }
        
        public static string GameName { get; private set; }
        public static bool IsExist { get; set; }
        
        
        private static void InitializeRuntime(GameStartInfo gameStartInfo)
        {
            if (gameStartInfo.Adapter == null)
            {
                var adapters = GpuAdapter.EnumerateGraphicsAdapter();

                LogEmitter.Assert(adapters.Count > 0, LogLevel.Error, "[Initialize Graphics Device Failed without Support Adapter] from [GameSystems]");

                GpuDevice = new GpuDevice(adapters[0]);
            }
            else GpuDevice = new GpuDevice(gameStartInfo.Adapter);

            EngineWindow = new EngineWindow(
                gameStartInfo.Window.Name,
                gameStartInfo.Window.Icon,
                gameStartInfo.Window.Size);
            EngineWindow.Show();

            PresentRender = new PresentRender(GpuDevice, EngineWindow.Handle, EngineWindow.Size);
        }

        public static void Initialize(GameStartInfo gameStartInfo)
        {
            GameName = gameStartInfo.Name;

            IsExist = true;
            
            InitializeRuntime(gameStartInfo);

            Gui.Initialize();

            LogEmitter.Apply(LogLevel.Information, "[Initialize Game Systems Finish] from [GameSystems]");
        }
        
        public static void RunLoop()
        {
            while (IsExist is true)
            {
                //update time
                Time.Update();

                if (EngineWindow != null && EngineWindow.IsExisted != false)
                    EngineWindow.Update(Time.DeltaSeconds);

                if (EngineWindow != null && EngineWindow.IsExisted == false)
                    IsExist = false;
                
                //update time
                InputListener.Update();
                Gui.Update(Time.DeltaSeconds);
                
                //draw time
                Gui.Draw();

                //present time
                PresentRender.BeginDraw();

                Gui.Present(PresentRender);

                PresentRender.EndDraw(false);

                InputListener.Clear();
            }
        }
    }
}
