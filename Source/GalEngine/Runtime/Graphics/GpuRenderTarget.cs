using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Runtime.Graphics
{
    public class GpuRenderTarget : IDisposable
    {
        private SharpDX.Direct3D11.RenderTargetView mRenderTarget;

        protected GpuDevice GpuDevice { get; }

        internal SharpDX.Direct3D11.RenderTargetView RenderTarget { get => mRenderTarget; }

        public Size Size { get; }
        
        public GpuRenderTarget(GpuDevice device, GpuSwapChain swapChain)
        {
            GpuDevice = device;

            //get back buffer and set render target desc
            var backTexture = swapChain.SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0);
            var renderTargetDesc = new SharpDX.Direct3D11.RenderTargetViewDescription()
            {
                 Format = GpuConvert.ToPixelFormat(swapChain.PixelFormat),
                 Texture2D = new SharpDX.Direct3D11.RenderTargetViewDescription.Texture2DResource()
                 {
                     MipSlice = 0
                 },
                 Dimension = SharpDX.Direct3D11.RenderTargetViewDimension.Texture2D
            };

            mRenderTarget = new SharpDX.Direct3D11.RenderTargetView(GpuDevice.Device, backTexture, renderTargetDesc);

            //set size
            Size = new Size(swapChain.Size.Width, swapChain.Size.Height);
        }

        public GpuRenderTarget(GpuDevice device, GpuTexture2D texture)
        {
            GpuDevice = device;
            
            //set render target desc
            var renderTargetDesc = new SharpDX.Direct3D11.RenderTargetViewDescription()
            {
                Format = GpuConvert.ToPixelFormat(texture.PixelFormat),
                Texture2D = new SharpDX.Direct3D11.RenderTargetViewDescription.Texture2DResource()
                {
                    MipSlice = 0,
                },
                Dimension = SharpDX.Direct3D11.RenderTargetViewDimension.Texture2D
            };

            mRenderTarget = new SharpDX.Direct3D11.RenderTargetView(GpuDevice.Device, texture.Resource, renderTargetDesc);

            //set size
            Size = new Size(texture.Size.Width, texture.Size.Height);
        }

        ~GpuRenderTarget() => Dispose();

        public void Dispose()
        {
            //we can dispose it any times, because we only dispose resource really at the first time
            SharpDX.Utilities.Dispose(ref mRenderTarget);
        }
    }
}
