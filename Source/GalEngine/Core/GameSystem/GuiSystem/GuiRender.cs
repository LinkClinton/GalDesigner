using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

using GalEngine.Runtime.Graphics;

namespace GalEngine
{
    public class GuiRender
    {
        private enum GuiRenderMode
        {
            Color = 0,
            Image = 1,
            Text = 2
        }

        private struct GuiVertex
        {
            public Vector3f Position;
            public Vector2f Texcoord;
        }

        private struct GuiRenderConfig
        {
            public Colorf Color;
            public Vector4 Config;
        }

        private struct GuiTransform
        {
            public Matrix4x4 World;
            public Matrix4x4 Project;
        }

        private readonly GpuDevice mDevice;

        private readonly GpuBlendState mBlendState;
        private readonly GpuInputLayout mInputLayout;
        private readonly GpuPixelShader mPixelShader;
        private readonly GpuVertexShader mVertexShader;

        private readonly GpuSamplerState mSamplerState;

        private readonly int mTextureSlot = 0;
        private readonly int mSamplerStateSlot = 0;
        private readonly int mTransformBufferSlot = 0;
        private readonly int mRenderConfigBufferSlot = 1;

        private GpuBuffer mSquareVertexBuffer;
        private GpuBuffer mSquareIndexBuffer;

        private GpuBuffer mRectangleVertexBuffer;
        private GpuBuffer mRectangleIndexBuffer;

        private GpuBuffer mTransformBuffer;
        private GpuBuffer mRenderConfigBuffer;

        private Matrix4x4 mProject;

        private Stack<Matrix4x4> mTransforms;

        public GpuDevice Device => mDevice;

        public Matrix4x4 Transform => mTransforms.Count == 0 ? Matrix4x4.Identity : mTransforms.Peek();

        private void InitializeSquareBuffer()
        {
            //init render object vertex buffer and index buffer
            //init square vertex data
            GuiVertex[] squareVertices = new GuiVertex[]
            {
                new GuiVertex() { Position = new Vector3f(0, 0, 0), Texcoord = new Vector2f(0, 0) },
                new GuiVertex() { Position = new Vector3f(0, 1, 0), Texcoord = new Vector2f(0, 1) },
                new GuiVertex() { Position = new Vector3f(1, 1, 0), Texcoord = new Vector2f(1, 1) },
                new GuiVertex() { Position = new Vector3f(1, 0, 0), Texcoord = new Vector2f(1, 0) }
            };

            //init square index data
            uint[] squareIndices = new uint[] { 0, 1, 2, 0, 2, 3 };

            //init square buffer and update
            mSquareVertexBuffer = new GpuBuffer(
                Utility.SizeOf<GuiVertex>() * squareVertices.Length,
                Utility.SizeOf<GuiVertex>() * 1,
                mDevice,
                GpuResourceInfo.VertexBuffer());

            mSquareIndexBuffer = new GpuBuffer(
                Utility.SizeOf<uint>() * squareIndices.Length,
                Utility.SizeOf<uint>() * 1,
                mDevice,
                GpuResourceInfo.IndexBuffer());

            mSquareVertexBuffer.Update(squareVertices);
            mSquareIndexBuffer.Update(squareIndices);
        }
        private void InitializeConstantBuffer()
        {
            //init shader buffer
            mTransformBuffer = new GpuBuffer(
                Utility.SizeOf<GuiTransform>(),
                Utility.SizeOf<GuiTransform>(),
                mDevice,
                GpuResourceInfo.ConstantBuffer());

            mRenderConfigBuffer = new GpuBuffer(
                Utility.SizeOf<GuiRenderConfig>(),
                Utility.SizeOf<GuiRenderConfig>(),
                mDevice,
                GpuResourceInfo.ConstantBuffer());
        }
        private void InitializeRectangleBuffer()
        {
            //init rectangle index data
            uint[] rectangleIndices = new uint[]
            {
                0, 4, 1, 1, 4, 5,
                0, 3, 4, 3, 7, 4,
                3, 6, 7, 2, 6, 3,
                2, 1, 6, 1, 5, 6
            };

            //init rectangle vertex and index buffer
            mRectangleVertexBuffer = new GpuBuffer(
                Utility.SizeOf<GuiVertex>() * 8,
                Utility.SizeOf<GuiVertex>() * 1,
                mDevice,
                GpuResourceInfo.VertexBuffer());

            mRectangleIndexBuffer = new GpuBuffer(
                Utility.SizeOf<uint>() * 24,
                Utility.SizeOf<uint>() * 1,
                mDevice,
                GpuResourceInfo.IndexBuffer());

            mRectangleIndexBuffer.Update(rectangleIndices);
        }

        private void DrawImage(Rectanglef rectangle, Image image, Colorf color, GuiRenderMode mode)
        {
            //fill rectangle with texture
            //the result color's alpha is equal texture.alpha * opacity

            //do not render image
            if (image.GpuTexture == null) return;

            var transform = new GuiTransform();
            var renderConfig = new GuiRenderConfig() { Color = color, Config = new Vector4((int)mode) };

            //1.scale the rectangle
            transform.World = Matrix4x4.CreateScale(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top, 1.0f);
            //2.translate it
            transform.World *= Matrix4x4.CreateTranslation(rectangle.Left, rectangle.Top, 0.0f);
            //3.keep transform matrix data
            transform.World *= Transform;

            //set projection matrix
            transform.Project = mProject;

            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            mDevice.SetVertexBuffer(mSquareVertexBuffer);
            mDevice.SetIndexBuffer(mSquareIndexBuffer);

            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.All);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.All);
            mDevice.SetResourceUsage(image.GpuResourceUsage, mTextureSlot, GpuShaderType.All);

            mDevice.DrawIndexed(6, 0, 0);
        }

        public GuiRender(GpuDevice device)
        {
            //gui render is a simple render to render gui object
            //gui render can provide some simple object draw function
            //we can replace it to our render and we can use it in the gui system render function
            mDevice = device;

            //default transform is I
            mTransforms = new Stack<Matrix4x4>();

            //init blend state
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

            //init vertex shader, for all draw command we use same vertex shader
            mVertexShader = new GpuVertexShader(mDevice, GpuVertexShader.Compile(Properties.Resources.GuiRenderShader, "vs_main"));

            //init pixel shader, we will choose the best pixel shader for different draw command
            mPixelShader = new GpuPixelShader(mDevice, GpuPixelShader.Compile(Properties.Resources.GuiRenderShader, "ps_main"));

            //init sampler state
            mSamplerState = new GpuSamplerState(mDevice);

            //init input layout
            //Position : float3
            //Texcoord : float2
            mInputLayout = new GpuInputLayout(mDevice, new InputElement[]
            {
                new InputElement("POSITION", 0, 12),
                new InputElement("TEXCOORD", 0, 8)
            }, mVertexShader);


            InitializeSquareBuffer();
            InitializeConstantBuffer();
            InitializeRectangleBuffer();
        }

        public virtual void BeginDraw(Image image)
        {
            //begin draw and we need set the render target before we draw anything

            //reset device and set render target
            mDevice.Reset();
            mDevice.SetRenderTarget(image.GpuRenderTarget);

            //set blend state
            mDevice.SetBlendState(mBlendState);

            //set input layout ,vertex shader and primitive type
            mDevice.SetInputLayout(mInputLayout);
            mDevice.SetPixelShader(mPixelShader);
            mDevice.SetVertexShader(mVertexShader);
            mDevice.SetPrimitiveType(GpuPrimitiveType.TriangleList);

            //set view port
            mDevice.SetViewPort(new Rectanglef(0, 0, image.Size.Width, image.Size.Height));

            mDevice.SetSamplerState(mSamplerState, mSamplerStateSlot, GpuShaderType.All);

            //set the project matrix, need set null when we end draw
            mProject = Matrix4x4.CreateOrthographicOffCenter(
                0, image.Size.Width,
                0, image.Size.Height, 0, 1);
        }

        public virtual void Clear(Image image, Colorf clear)
        {
            mDevice.ClearRenderTarget(
                renderTarget: image.GpuRenderTarget,
                color: clear);
        }

        public virtual void EndDraw()
        {
            mProject = Matrix4x4.Identity;
        }

        public virtual void PushTransform(Transform transform)
        {
            mTransforms.Push(
                (mTransforms.Count == 0 ? Matrix4x4.Identity : mTransforms.Peek()) * 
                transform.Matrix);
        }

        public virtual void PopTransform()
        {
            mTransforms.Pop();
        }

        public virtual void ClearTransform()
        {
            mTransforms.Clear();
        }

        public virtual void DrawLine(Point2f start, Point2f end, Colorf color, float padding = 2.0f)
        {
            //draw line with start and end position
            //padding means the width of line
            //color.Alpha means the opacity of line

            //first, we compute the matrix of world transform
            var transform = new GuiTransform();
            var vector = new System.Numerics.Vector2(end.X - start.X, end.Y - start.Y);

            //1.compute the scale component, x-component is euqal the length of line, y-component is equal the padding
            transform.World = Matrix4x4.CreateScale(vector.Length(), padding, 1.0f);
            //2.compute the angle of rotate, we only rotate it at the z-axis
            transform.World *= Matrix4x4.CreateRotationZ((float)Math.Atan2(vector.Y, vector.X), new System.Numerics.Vector3(0, padding * 0.5f, 0));
            //3.compute the translation, the position of start, but the y is start.y - padding * 0.5f
            transform.World *= Matrix4x4.CreateTranslation(new System.Numerics.Vector3(start.X, start.Y - padding * 0.5f, 0));
            //4.keep transform matrix data
            transform.World *= Transform;

            //set project matrix
            transform.Project = mProject;

            //second, we set the render config
            var renderConfig = new GuiRenderConfig() { Color = color, Config = new Vector4(0) };

            //update buffer
            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            //set buffer and shader
            mDevice.SetVertexBuffer(mSquareVertexBuffer);
            mDevice.SetIndexBuffer(mSquareIndexBuffer);

            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.All);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.All);

            //draw
            mDevice.DrawIndexed(6, 0, 0);
        }

        public virtual void DrawRectangle(Rectanglef rectangle, Colorf color, float padding = 2.0f)
        {
            //draw rectangle with color
            //padding means the width of edge
            //color.Alpha means the opacity of edge

            //read rectangle data
            var outSide = rectangle;

            //inside rectangle will smaller than outside
            var inSide = new Rectanglef(
                outSide.Left + padding,
                outSide.Top + padding,
                outSide.Right - padding,
                outSide.Bottom - padding);

            //first, we need compute the vertex buffer for our rectangle
            //we do not need tex coord of vertex
            //we can compute the index buffer at init the render
            GuiVertex[] vertics = new GuiVertex[]
            {
                new GuiVertex() { Position = new Vector3f(outSide.Left,  outSide.Top, 0 ) },
                new GuiVertex() { Position = new Vector3f(outSide.Right, outSide.Top, 0) },
                new GuiVertex() { Position = new Vector3f(outSide.Right, outSide.Bottom, 0) },
                new GuiVertex() { Position = new Vector3f(outSide.Left,  outSide.Bottom, 0) },
                new GuiVertex() { Position = new Vector3f(inSide.Left,   inSide.Top, 0) },
                new GuiVertex() { Position = new Vector3f(inSide.Right,  inSide.Top, 0) },
                new GuiVertex() { Position = new Vector3f(inSide.Right,  inSide.Bottom, 0) },
                new GuiVertex() { Position = new Vector3f(inSide.Left,   inSide.Bottom, 0) }
            };

            //update vertex buffer
            mRectangleVertexBuffer.Update(vertics);

            //second, update constant buffer
            var transform = new GuiTransform() { World = Transform, Project = mProject };
            var renderConfig = new GuiRenderConfig() { Color = color, Config = new Vector4(0) };

            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            //set buffer and shader
            mDevice.SetVertexBuffer(mRectangleVertexBuffer);
            mDevice.SetIndexBuffer(mRectangleIndexBuffer);

            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.All);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.All);
            
            //draw
            mDevice.DrawIndexed(24, 0, 0);
        }

        public virtual void DrawImage(Rectanglef rectangle, Image image, Colorf color)
        {
            DrawImage(rectangle, image, color, GuiRenderMode.Image);
        }

        public virtual void DrawImage(Rectanglef rectangle, Image image, float opacity = 1.0f)
        {
            DrawImage(rectangle, image, new Colorf(1.0f, 1.0f, 1.0f, opacity));
        }

        public virtual void DrawText(Point2f position, Text text, Colorf color)
        {
            DrawImage(new Rectanglef(
                position.X,
                position.Y,
                position.X + text.Size.Width,
                position.Y + text.Size.Height), text.Image, color, GuiRenderMode.Text);
        }

        public virtual void DrawText(Point2f position, RowText text, Colorf color)
        {
            DrawImage(new Rectanglef(
                position.X,
                position.Y,
                position.X + text.Size.Width,
                position.Y + text.Size.Height), text.Image, color, GuiRenderMode.Text);
        }

        public virtual void DrawText(Rectanglef rectangle, Text text, Colorf color)
        {
            DrawImage(rectangle, text.Image, color, GuiRenderMode.Text);
        }

        public virtual void FillRectangle(Rectanglef rectangle, Colorf color)
        {
            //fill rectangle
            //color.alpha means the opacity of rectangle

            var transform = new GuiTransform();
            var renderConfig = new GuiRenderConfig() { Color = color, Config = new Vector4(0) };

            //1.scale the rectangle
            transform.World = Matrix4x4.CreateScale(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top, 1.0f);
            //2.translate it
            transform.World *= Matrix4x4.CreateTranslation(rectangle.Left, rectangle.Top, 0.0f);
            //3.keep transform matrix data
            transform.World *= Transform;

            //set projection matrix
            transform.Project = mProject;

            mTransformBuffer.Update(transform);
            mRenderConfigBuffer.Update(renderConfig);

            mDevice.SetVertexBuffer(mSquareVertexBuffer);
            mDevice.SetIndexBuffer(mSquareIndexBuffer);

            mDevice.SetBuffer(mTransformBuffer, mTransformBufferSlot, GpuShaderType.All);
            mDevice.SetBuffer(mRenderConfigBuffer, mRenderConfigBufferSlot, GpuShaderType.All);

            mDevice.DrawIndexed(6, 0, 0);
        }
    }
}
