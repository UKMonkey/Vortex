using System;
using System.Collections.Generic;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Renderer.Camera;

namespace Vortex.Renderer.WorldRenderers
{
    internal interface IWorldRenderer : IDisposable
    {
        void Render(float range, float angle, float minrange, float rotation,
            IEnumerable<Entity> entities, AnyCamera camera, Matrix projectionTransform);
        void Prepare(BasicCamera camera);
        void WriteToConsole();
    }
}