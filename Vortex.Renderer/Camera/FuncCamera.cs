using System;
using Psy.Graphics;
using SlimMath;

namespace Vortex.Renderer.Camera
{
    public class FuncCamera : BasicCamera
    {
        private readonly Func<Vector3> _positionCallback;

        public FuncCamera(GraphicsContext graphicsContext, Func<Vector3> positionCallback)
            : base(graphicsContext)
        {
            _positionCallback = positionCallback;
        }

        protected override Vector3 GetPosition()
        {
            return _positionCallback();
        }
    }
}