using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Runtime.Graphics
{
    public class GpuTexture2D : GpuResource
    {
        private int mRowPitch;

        public GpuPixelFormat PixelFormat { get; }
        public Size Size { get; }

        public GpuTexture2D(
            Size size,
            GpuPixelFormat pixelFormat, 
            GpuDevice device,
            GpuResourceInfo resourceInfo) :
            base(device, size.Width * size.Height * GpuConvert.SizeOfInBytes(pixelFormat), resourceInfo)
        {
            Size = new Size(size.Width, size.Height);
            PixelFormat = pixelFormat;

            mRowPitch = Size.Width * GpuConvert.SizeOfInBytes(PixelFormat);

            mResource = new SharpDX.Direct3D11.Texture2D(GpuDevice.Device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = GpuConvert.ToBindUsage(ResourceInfo.BindUsage),
                CpuAccessFlags = GpuConvert.ToCpuAccessFlag(ResourceInfo.CpuAccessFlag),
                Format = GpuConvert.ToPixelFormat(PixelFormat),
                Width = Size.Width,
                Height = Size.Height,
                MipLevels = 1,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = GpuConvert.ToHeapType(ResourceInfo.HeapType)
            });
        }

        public void CopyFromGpuTexture2D(Point2 destination, GpuTexture2D source, Rectangle region)
        {
            if (region.Right - region.Left == 0 || region.Bottom - region.Top == 0) return;

            var resourceRegion = new SharpDX.Direct3D11.ResourceRegion(
                region.Left, region.Top, 0,
                region.Right, region.Bottom, 1);

            GpuDevice.ImmediateContext.CopySubresourceRegion(source.mResource, 0, resourceRegion,
                mResource, 0, destination.X, destination.Y);
        }

        public override void Update<T>(params T[] data)
        {
            GpuDevice.ImmediateContext.UpdateSubresource(data, Resource, 0, mRowPitch);
        }

        public override void Update(params byte[] data)
        {
            GpuDevice.ImmediateContext.UpdateSubresource(data, Resource, 0, mRowPitch);
        }
    }
}
