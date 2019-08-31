using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Runtime.Graphics
{
    using Debug = System.Diagnostics.Debug;

    public class GpuDevice : IDisposable
    {
        private GpuVertexShader mVertexShader;
        private GpuPixelShader mPixelShader;
        private GpuInputLayout mInputLayout;

        private SharpDX.Direct3D11.Device mDevice;
        private SharpDX.Direct3D11.DeviceContext mImmediateContext;

        internal SharpDX.Direct3D11.Device Device => mDevice;
        internal SharpDX.Direct3D11.DeviceContext ImmediateContext => mImmediateContext;
        
        public GpuAdapter Adapter { get; }

        public GpuDevice(GpuAdapter adapter)
        {
            //set member to null
            mVertexShader = null;
            mPixelShader = null;
            mInputLayout = null;

            //set the adapter
            Adapter = adapter;

#if DEBUG
            //creation flag, we use debug flag
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.Debug;
#else
            var creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.None;
#endif
            //fetuares level: 11_0, 12_0
            var fetuares = new SharpDX.Direct3D.FeatureLevel[2]
            {
                 SharpDX.Direct3D.FeatureLevel.Level_11_0,
                 SharpDX.Direct3D.FeatureLevel.Level_12_0
            };

            try
            {
                //create device with current adapter
                mDevice = new SharpDX.Direct3D11.Device(Adapter.Adapter, creationFlags, fetuares);
            }
            catch (SharpDX.SharpDXException)
            {
                //try without debug layer
                creationFlags = SharpDX.Direct3D11.DeviceCreationFlags.None;

                mDevice = new SharpDX.Direct3D11.Device(Adapter.Adapter, creationFlags, fetuares);

                LogEmitter.Apply(LogLevel.Warning, "[Enable Graphics Debug Layer Failed]");
            }
      
            mImmediateContext = Device.ImmediateContext;
            
            LogEmitter.Apply(LogLevel.Information, "[Initialize Graphics Device with {0}]", adapter.Description);
            LogEmitter.Apply(LogLevel.Information, "[Graphics Device Feature Level = {0}]", Device.FeatureLevel);
        }

        ~GpuDevice() => Dispose();

        public void Reset()
        {
            //clear all state and reset it

            mVertexShader = null;
            mPixelShader = null;
            mInputLayout = null;

            ImmediateContext.ClearState();
        }

        public void ClearRenderTarget(GpuRenderTarget renderTarget, Colorf color)
        {
            //clear render target using color
            //x = red, y = green, z = blue, w = alpha
            ImmediateContext.ClearRenderTargetView(renderTarget.RenderTarget,
                new SharpDX.Mathematics.Interop.RawColor4(color.Red, color.Green, color.Blue, color.Alpha));
        }

        public void ClearRenderTarget(Image renderTarget, Colorf color)
        {
            //clear render target using color
            //x = red, y = green, z = blue, w = alpha
            ImmediateContext.ClearRenderTargetView(renderTarget.GpuRenderTarget.RenderTarget,
                new SharpDX.Mathematics.Interop.RawColor4(color.Red, color.Green, color.Blue, color.Alpha));
        }

        public void SetViewPort(Rectanglef viewPort)
        {
            //set view port

            var viewPortF = new SharpDX.Mathematics.Interop.RawViewportF()
            {
                X = viewPort.Left,
                Y = viewPort.Top,
                Width = viewPort.Right - viewPort.Left,
                Height = viewPort.Bottom - viewPort.Top,
                MinDepth = 0.0f,
                MaxDepth = 1.0f
            };

            ImmediateContext.Rasterizer.SetViewport(viewPortF);
        }

        public void SetRenderTarget(GpuRenderTarget renderTarget)
        {
            //set render target
            ImmediateContext.OutputMerger.SetRenderTargets(renderTarget.RenderTarget);
        }

        public void SetRenderTarget(Image image)
        {
            //set render target
            SetRenderTarget(image.GpuRenderTarget);
        }

        public void SetInputLayout(GpuInputLayout inputLayout)
        {
            //set input layout
            mInputLayout = inputLayout;

            //set input layout Direct3D instance to pipeline
            ImmediateContext.InputAssembler.InputLayout = mInputLayout.InputLayout;
        }

        public void SetVertexShader(GpuVertexShader vertexShader)
        {
            //set vertex shader
            mVertexShader = vertexShader;

            //set vertex shader Direct3D instance to pipeline
            ImmediateContext.VertexShader.SetShader(mVertexShader.VertexShader, null, 0);
        }

        public void SetPixelShader(GpuPixelShader pixelShader)
        {
            //set pixel shader
            mPixelShader = pixelShader;

            //set pixel shader Direct3D instance to pipeline
            ImmediateContext.PixelShader.SetShader(mPixelShader.PixelShader, null, 0);
        }

        public void SetBuffer(GpuBuffer buffer, int register, GpuShaderType target = GpuShaderType.All)
        {
            //set buffer Direct3D instance to pipeline's shader
            //we use target shader to flag which shader the buffer will set to

            //test if the buffer can be set
            Debug.Assert(GpuConvert.HasBindUsage(buffer.ResourceInfo.BindUsage, GpuBindUsage.ConstantBuffer) == true);

            //we can use "&" to make sure if we need set buffer to vertex shader
            if ((target & GpuShaderType.VertexShader) != GpuShaderType.None)
                ImmediateContext.VertexShader.SetConstantBuffer(register, buffer.Resource as SharpDX.Direct3D11.Buffer);

            //we can use "&" to make sure if we need set buffer to pixel shader
            if ((target & GpuShaderType.PixelShader) != GpuShaderType.None)
                ImmediateContext.PixelShader.SetConstantBuffer(register, buffer.Resource as SharpDX.Direct3D11.Buffer);
        }

        public void SetResourceUsage(GpuResourceUsage resourceUsage, int register, GpuShaderType target = GpuShaderType.All)
        {
            //set resource Direct3D instance to pipeline's shader
            //we use target shader to flag which shader the resource will set to
            
            //we can use "&" to make sure if we need set buffer to vertex shader
            if ((target & GpuShaderType.VertexShader) != GpuShaderType.None)
                ImmediateContext.VertexShader.SetShaderResource(register, resourceUsage.ShaderResource);

            //we can use "&" to make sure if we need set buffer to pixel shader
            if ((target & GpuShaderType.PixelShader) != GpuShaderType.None)
                ImmediateContext.PixelShader.SetShaderResource(register, resourceUsage.ShaderResource);
        }

        public void SetSamplerState(GpuSamplerState samplerState, int register, GpuShaderType target = GpuShaderType.All)
        {
            //set sampler state Direct3D instance to pipeline's shader
            //we use target shader to flag which shader the resource will set to

            //we can use "&" to make sure if we need set sampler state to vertex shader
            if ((target & GpuShaderType.VertexShader) != GpuShaderType.None)
                ImmediateContext.VertexShader.SetSampler(register, samplerState.SamplerState);

            //we can use "&" to make sure if we need set sampler state to pixel shader
            if ((target & GpuShaderType.PixelShader) != GpuShaderType.None)
                ImmediateContext.PixelShader.SetSampler(register, samplerState.SamplerState);
        }

        public void SetVertexBuffer(GpuBuffer buffer)
        {
            //set vertex buffer to input stage

            //test if the buffer can be set
            Debug.Assert(GpuConvert.HasBindUsage(buffer.ResourceInfo.BindUsage, GpuBindUsage.VertexBufferr) == true);

            //create buffer binding
            var bufferBinding = new SharpDX.Direct3D11.VertexBufferBinding(
                buffer.Resource as SharpDX.Direct3D11.Buffer,
                buffer.ElementSize, 0);

            //set vertex buffer
            ImmediateContext.InputAssembler.SetVertexBuffers(0, bufferBinding);
        }

        public void SetIndexBuffer(GpuBuffer buffer)
        {
            //set index buffer to input stage

            //test if the buffer can be set
            Debug.Assert(GpuConvert.HasBindUsage(buffer.ResourceInfo.BindUsage, GpuBindUsage.IndexBuffer) == true);

            //set index buffer
            ImmediateContext.InputAssembler.SetIndexBuffer(buffer.Resource as SharpDX.Direct3D11.Buffer,
                 SharpDX.DXGI.Format.R32_UInt, 0);
        }

        public void SetPrimitiveType(GpuPrimitiveType primitiveType)
        {
            //set primitive type
            ImmediateContext.InputAssembler.PrimitiveTopology = GpuConvert.ToPrimitiveType(primitiveType);
        }

        public void SetBlendState(GpuBlendState blendState)
        {
            //set blend state
            //note: if you change the blend state, you need to reset the state
            ImmediateContext.OutputMerger.BlendState = blendState.BlendState;
        }

        public void SetRasterizerState(GpuRasterizerState rasterizerState)
        {
            //set rasterizerState
            //note: if you change the rasterizer state, you need to reset the state
            ImmediateContext.Rasterizer.State = rasterizerState.RasterizerState;
        }

        public void DrawIndexed(int indexCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
            //draw indexed
            ImmediateContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }

        public void DrawIndexedInstanced(int indexCount, int instanceCount, int startIndexLocation = 0, int baseVertexLocation = 0)
        {
            //drwa indexed instanced
            ImmediateContext.DrawIndexedInstanced(indexCount, instanceCount, startIndexLocation, baseVertexLocation, 0);
        }

        public void Dispose()
        {
            //we can dispose it any times, because we only dispose resource really at the first time
            SharpDX.Utilities.Dispose(ref mImmediateContext);
            SharpDX.Utilities.Dispose(ref mDevice);
        }
    }
}
