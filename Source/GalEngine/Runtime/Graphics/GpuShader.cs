using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine.Runtime.Graphics
{
    public enum GpuShaderType : uint
    {
        None = 0,
        VertexShader = 1,
        PixelShader = 2,
        All = 3
    }

    public abstract class GpuShader
    {
#if DEBUG
        protected static SharpDX.D3DCompiler.ShaderFlags ShaderFlags => 
            SharpDX.D3DCompiler.ShaderFlags.Debug |
            SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
#else
         protected static SharpDX.D3DCompiler.ShaderFlags ShaderFlags => SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel2;
#endif
        protected GpuDevice GpuDevice { get; }
        
        public byte[] ByteCode { get; }

        public GpuShader(GpuDevice device, byte[] byteCode)
        {
            ByteCode = byteCode;
            GpuDevice = device;
        }
    }
}
