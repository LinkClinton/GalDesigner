using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Runtime.Graphics
{
    public class GpuSwapChain : IDisposable
    {
        private SharpDX.DXGI.SwapChain mSwapChain;

        protected GpuDevice GpuDevice { get; }

        internal SharpDX.DXGI.SwapChain SwapChain => mSwapChain;

        public Size Size { get; }
        public GpuPixelFormat PixelFormat { get; }
        public GpuRenderTarget RenderTarget { get; }

        public GpuSwapChain(
            IntPtr handle, 
            Size size, 
            GpuPixelFormat pixelFormat,
            GpuDevice device)
        {
            //size property
            Size = size;
            PixelFormat = pixelFormat;
            GpuDevice = device;

            //get factory
            using (var factory = GpuDevice.Adapter.Adapter.GetParent<SharpDX.DXGI.Factory>())
            {
                //set swapchain desc
                var swapChainDesc = new SharpDX.DXGI.SwapChainDescription()
                {
                    BufferCount = 1,
                    Flags = SharpDX.DXGI.SwapChainFlags.None,
                    IsWindowed = true,
                    ModeDescription = new SharpDX.DXGI.ModeDescription()
                    {
                        Format = GpuConvert.ToPixelFormat(PixelFormat),
                        Height = Size.Height,
                        Width = Size.Width,
                        RefreshRate = new SharpDX.DXGI.Rational(60, 1),
                        Scaling = SharpDX.DXGI.DisplayModeScaling.Unspecified,
                        ScanlineOrdering = SharpDX.DXGI.DisplayModeScanlineOrder.Unspecified
                    },
                    OutputHandle = handle,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput
                };

                mSwapChain = new SharpDX.DXGI.SwapChain(factory, GpuDevice.Device, swapChainDesc);

                //report error, if create swapchain failed
                LogEmitter.Assert(mSwapChain != null, LogLevel.Error, 
                    "[Create SwapChain Failed] [Width = {0}] [Height = {1}] [Format = {2}]", Size.Width, Size.Height, PixelFormat);

                RenderTarget = new GpuRenderTarget(GpuDevice, this);
            }
        }

        ~GpuSwapChain() => Dispose();
        
        public void Present(bool sync)
        {
            mSwapChain.Present(sync ? 1 : 0, SharpDX.DXGI.PresentFlags.None);
        }

        public void Dispose()
        {
            SharpDX.Utilities.Dispose(ref mSwapChain);
        }
    }
}
