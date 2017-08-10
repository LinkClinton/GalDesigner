﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presenter
{
    public class CanvasBrush : CanvasResource
    {
        private SharpDX.Direct2D1.Brush brush;

        public CanvasBrush(float red, float green, float blue, float alpha = 1)
        {
            brush = new SharpDX.Direct2D1.SolidColorBrush(Canvas.ID2D1DeviceContext,
                new SharpDX.Mathematics.Interop.RawColor4(red, green, blue, alpha));
        }

        internal SharpDX.Direct2D1.Brush ID2D1Brush => brush;

        ~CanvasBrush() => SharpDX.Utilities.Dispose(ref brush);
    }

    public static partial class Canvas
    {
        public static void DrawLine(float startPosX, float startPosY, float endPosX, float endPosY,
            CanvasBrush brush, float strokeWidth = 1.0f)
        {
            ID2D1DeviceContext.DrawLine(new SharpDX.Mathematics.Interop.RawVector2(startPosX, startPosY),
                new SharpDX.Mathematics.Interop.RawVector2(endPosX, endPosY),
                brush.ID2D1Brush, strokeWidth);
        }

        public static void DrawRectangle(float left, float top, float right, float bottom,
            CanvasBrush brush, float strokeWidth = 1.0f)
        {
            ID2D1DeviceContext.DrawRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(left, top, right, bottom),
                brush.ID2D1Brush, strokeWidth);
        }

        public static void DrawEllipse(float centerPosX, float centerPosY, float radiusX, float radiusY,
            CanvasBrush brush, float strokeWidth = 1.0f)
        {
            ID2D1DeviceContext.DrawEllipse(new SharpDX.Direct2D1.Ellipse(
                new SharpDX.Mathematics.Interop.RawVector2(centerPosX, centerPosY), radiusX, radiusY),
                brush.ID2D1Brush, strokeWidth);
        }
    }
}