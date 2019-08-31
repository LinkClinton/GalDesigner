using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace GalEngine
{
    public class Transform
    {
        protected Matrix4x4 mMatrix;
        protected Matrix4x4 mInvert;
        
        public Matrix4x4 Matrix => mMatrix;
        
        public Transform()
        {
            mMatrix = Matrix4x4.Identity;
            mInvert = Matrix4x4.Identity;
        }

        public Transform(Matrix4x4 matrix)
        {
            SetTransform(matrix);
        }

        /// <summary>
        /// Apply Transform, Result is Transform.Matrix * Matrix
        /// </summary>
        /// <param name="matrix">Transform</param>
        public void ApplyTransform(Matrix4x4 matrix)
        {
            SetTransform(mMatrix * matrix);
        }

        /// <summary>
        /// Set Transform, Result is Transform = Matrix
        /// </summary>
        /// <param name="matrix">Transform</param>
        public virtual void SetTransform(Matrix4x4 matrix)
        {
            mMatrix = matrix;

            bool result = Matrix4x4.Invert(mMatrix, out mInvert);

            LogEmitter.Assert(result, LogLevel.Error, "[Invert Gui Transform Failed]");
        }
       
        /// <summary>
        /// Get Invert Transform
        /// </summary>
        /// <returns>Invert Transform</returns>
        public virtual Transform Invert()
        {
            return new Transform()
            {
                mMatrix = mInvert,
                mInvert = mMatrix
            };
        }

        /// <summary>
        /// Transform a point
        /// </summary>
        /// <param name="position">Point position</param>
        /// <returns>New point position</returns>
        public virtual Point2f TransformTo(Point2f position)
        {
            var result = System.Numerics.Vector2.Transform(
                    new System.Numerics.Vector2(position.X, position.Y),
                    mMatrix);

            return new Point2f(result.X, result.Y);
        }

        public static Transform operator *(Transform left, Transform right)
        {
            return new Transform(left.Matrix * right.Matrix);
        }
    }
}
