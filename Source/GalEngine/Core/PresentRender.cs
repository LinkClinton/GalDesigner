using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class PresentRender
    {
        private struct Vertex
        {
            public System.Numerics.Vector3 Position;
            public System.Numerics.Vector2 TexCoord;
        }

        private struct Transform
        {
            public System.Numerics.Matrix4x4 World;
            public System.Numerics.Matrix4x4 Projection;
        }

        private struct RenderConfig
        {
            public System.Numerics.Vector4 Opacity;
        }

        private IntPtr mHandle;
        private GpuDevice mDevice;
        private GpuSwapChain mSwapChain;

        private readonly GpuBlendState mBlendState;
        private readonly GpuPixelShader mDrawPixelShader;
        private readonly GpuPixelShader mMaskPixelShader;
        private readonly GpuVertexShader mVertexShader;
        private readonly GpuInputLayout mInputLayout;

        private readonly GpuBuffer mVertexBuffer;
        private readonly GpuBuffer mIndexBuffer;

        private readonly GpuBuffer mTransformBuffer;
        private readonly GpuBuffer mRenderConfigBuffer;
        private readonly GpuSamplerState mGpuSamplerState;

        private readonly int mTextureSlot = 0;
        private readonly int mSamplerStateSlot = 0;
        private readonly int mTransformBufferSlot = 0;
        private readonly int mRenderConfigBufferSlot = 0;

        public PresentRender(GpuDevice device, IntPtr handle, Size size)
        {
            mHandle = handle;
            mDevice = device;
            mSwapChain = new GpuSwapChain(handle, size, GpuPixelFormat.R8G8B8A8Unknown, mDevice);

            mBlendState = new GpuBlendState(mDevice, new RenderTargetBlendDescription()
            {
                AlphaBlendOperation = GpuBlendOperation.Add,
                BlendOperation = GpuBlendOperation.Add,
                DestinationAlphaBlend = GpuBlendOption.InverseSourceAlpha,
                DestinationBlend = GpuBlendOption.InverseSourceAlpha,
                SourceAlphaBlend = GpuBlendOption.SourceAlpha,
                SourceBlend = GpuBlendOption.SourceAlpha,
                IsBlendEnable = true
            });

            //compile shader
            mVertexShader = new GpuVertexShader(mDevice, GpuVertexShader.Compile(Properties.Resources.PresentVertexShader));
            mDrawPixelShader = new GpuPixelShader(mDevice, GpuPixelShader.Compile(Properties.Resources.PresentDrawPixelShader));
            mMaskPixelShader = new GpuPixelShader(mDevice, GpuPixelShader.Compile(Properties.Resources.PresentMaskPixelShader));
            
            //create input layout, we only need to render texture
            mInputLayout = new GpuInputLayout(mDevice, new InputElement[]
            {
                new InputElement("POSITION", 0, 12),
                new InputElement("TEXCOORD", 0, 8)
            }, mVertexShader);

            //init vertex and index data
            Vertex[] vertices = new Vertex[]
            {
                new Vertex() { Position = new System.Numerics.Vector3(0, 0, 0), TexCoord = new System.Numerics.Vector2(0, 0) },
                new Vertex() { Position = new System.Numerics.Vector3(0, 1, 0), TexCoord = new System.Numerics.Vector2(0, 1) },
                new Vertex() { Position = new System.Numerics.Vector3(1, 1, 0), TexCoord = new System.Numerics.Vector2(1, 1) },
                new Vertex() { Position = new System.Numerics.Vector3(1, 0, 0), TexCoord = new System.Numerics.Vector2(1, 0) }
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                2, 3 ,0
            };

            //create vertex and index buffer
            mVertexBuffer = new GpuBuffer(
                Utility.SizeOf<Vertex>() * vertices.Length,
                Utility.SizeOf<Vertex>(),
                mDevice, GpuResourceInfo.VertexBuffer());

            mIndexBuffer = new GpuBuffer(
                Utility.SizeOf<uint>() * indices.Length,
                Utility.SizeOf<uint>(),
                mDevice, GpuResourceInfo.IndexBuffer());

            mVertexBuffer.Update(vertices);
            mIndexBuffer.Update(indices);

            //create constant buffer
            //transform buffer is used for vertex shader to do transform
            //render config buffer is used for pixel shader to render with opacity
            mTransformBuffer = new GpuBuffer(
                Utility.SizeOf<Transform>(),
                Utility.SizeOf<Transform>(),
                mDevice, GpuResourceInfo.ConstantBuffer());

            mRenderConfigBuffer = new GpuBuffer(
                Utility.SizeOf<RenderConfig>(),
                Utility.SizeOf<RenderConfig>(),
                mDevice, GpuResourceInfo.ConstantBuffer());

            mGpuSamplerState = new GpuSamplerState(mDevice);
        }

        public void ReSize(Size newSize)
        {
            Utility.Dispose(ref mSwapChain);

            mSwapChain = new GpuSwapChain(mHandle, newSize, GpuPixelFormat.R8G8B8A8Unknown, mDevice);
        }

        public void BeginDraw()
        {
            //reset graphics pipeline and set render target
            mDevice.Reset();
            mDevice.SetRenderTarget(mSwapChain.RenderTarget);
            mDevice.ClearRenderTarget(mSwapChain.RenderTarget, new Colorf(1, 1, 1, 1));

            //set graphics pipeline
            mDevice.SetBlendState(mBlendState);

            mDevice.SetInputLayout(mInputLayout);
            mDevice.SetVertexBuffer(mVertexBuffer);
            mDevice.SetIndexBuffer(mIndexBuffer);
            mDevice.SetPrimitiveType(GpuPrimitiveType.TriangleList);

            mDevice.SetVertexShader(mVertexShader);
            mDevice.SetPixelShader(mDrawPixelShader);

            mDevice.SetViewPort(new Rectanglef(0, 0, mSwapChain.Size.Width, mSwapChain.Size.Height));
        }

        public void EndDraw(bool sync)
        {
            Present(sync);
        }

        public void Draw(Image texture, Rectangle area, float opacity = 1.0f)
        {
            //init the data we need uo upload
            Transform transform = new Transform();
            RenderConfig renderConfig = new RenderConfig();

            transform.World = Matrix4x4.CreateTranslation(new System.Numerics.Vector3(area.Left, area.Top, 0));
            transform.World = Matrix4x4.CreateScale(new System.Numerics.Vector3(area.Right - area.Left, area.Bottom - area.Top, 1)) * transform.World;
            transform.Projection = Matrix4x4.CreateOrthographicOffCenter(0, mSwapChain.Size.Width, 0, mSwapChain.Size.Height, 0, 1);

            renderConfig.Opacity = new System.Numerics.Vector4(opacity);

            //upload it to gpu memory
            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            //set resource to shader
            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.VertexShader);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.PixelShader);
            mDevice.SetSamplerState(mGpuSamplerState, mSamplerStateSlot, GpuShaderType.PixelShader);
            mDevice.SetResourceUsage(texture.GpuResourceUsage, mTextureSlot, GpuShaderType.PixelShader);

            //draw
            mDevice.DrawIndexed(6, 0, 0);
        }

        public void Mask(Image texture, Rectangle area, float opacity = 1.0f)
        {
            //init the data we need uo upload
            Transform transform = new Transform();
            RenderConfig renderConfig = new RenderConfig();

            transform.World = Matrix4x4.CreateTranslation(new System.Numerics.Vector3(area.Left, area.Top, 0));
            transform.World = Matrix4x4.CreateScale(new System.Numerics.Vector3(area.Right - area.Left, area.Bottom - area.Top, 1)) * transform.World;
            transform.Projection = Matrix4x4.CreateOrthographicOffCenter(0, mSwapChain.Size.Width, 0, mSwapChain.Size.Height, 0, 1);

            renderConfig.Opacity = new System.Numerics.Vector4(opacity);

            //upload it to gpu memory
            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            //change shader
            mDevice.SetPixelShader(mMaskPixelShader);

            //set resource to shader
            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.VertexShader);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.PixelShader);
            mDevice.SetSamplerState(mGpuSamplerState, mSamplerStateSlot, GpuShaderType.PixelShader);
            mDevice.SetResourceUsage(texture.GpuResourceUsage, mTextureSlot, GpuShaderType.PixelShader);

            //draw
            mDevice.DrawIndexed(6, 0, 0);

            //reset shader
            mDevice.SetPixelShader(mDrawPixelShader);
        }

        public void Present(bool sync)
        {
            mSwapChain.Present(sync);
        }
    }
}
