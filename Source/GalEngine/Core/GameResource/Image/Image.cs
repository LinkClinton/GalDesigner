﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class Image : IDisposable
    {
        private GpuDevice mDevice;

        private GpuTexture2D mTexture;
        private GpuRenderTarget mRenderTarget;
        private GpuResourceUsage mResourceUsage;

        internal GpuTexture2D GpuTexture => mTexture;
        internal GpuRenderTarget GpuRenderTarget => mRenderTarget == null ? mRenderTarget = new GpuRenderTarget(mDevice, mTexture) : mRenderTarget;
        internal GpuResourceUsage GpuResourceUsage => mResourceUsage == null ? mResourceUsage = new GpuResourceUsage(mDevice, mTexture) : mResourceUsage;

        public Size Size => mTexture != null ? mTexture.Size : new Size(0, 0);
        public int SizeInBytes => mTexture.SizeInBytes;
        public PixelFormat PixelFormat => (PixelFormat)mTexture.PixelFormat;

        public Image(Size size, PixelFormat pixelFormat) : this(size, pixelFormat, GameSystems.GpuDevice)
        {

        }

        public Image(Size size, PixelFormat pixelFormat, byte[] data) : this(size, pixelFormat)
        {
            mTexture.Update(data);
        }
        
        public Image(Size size, PixelFormat pixelFormat, GpuDevice device)
        {
            mDevice = device;

            //do not need to create texture, because the size is zero
            if (size.Width == 0 || size.Height == 0) return;

            mTexture = new GpuTexture2D(size, (GpuPixelFormat)pixelFormat, device,
               new GpuResourceInfo(GpuBindUsage.ShaderResource | GpuBindUsage.RenderTarget));
        }

        ~Image() => Dispose();

        public void CopyFromImage(Point2 destination, Image texture, Rectangle region)
        {
            //do not need copy, because the size is zero
            if (region.Left == region.Right || region.Top == region.Bottom) return;

            mTexture.CopyFromGpuTexture2D(destination, texture.mTexture, region);
        }

        public void Dispose()
        {
            Utility.Dispose(ref mResourceUsage);
            Utility.Dispose(ref mRenderTarget);
            Utility.Dispose(ref mTexture);
        }
    }
}