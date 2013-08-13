using System.Collections.Generic;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Renderer.Camera;
using Vortex.World.Observable;

namespace Vortex.Renderer.WorldRenderers
{
    internal abstract class BaseWorldRenderer : IWorldRenderer
    {
        protected IObservableArea ObservableArea { get; set; }

        protected BaseWorldRenderer(IObservableArea observableArea)
        {
            ObservableArea = observableArea;
        }

        public abstract void Dispose();
        public abstract void Render(float range, float angle, float minrange, float rotation,
            IEnumerable<Entity> entities, AnyCamera camera, Matrix projectionTransform);
        public abstract void Prepare(BasicCamera camera);
        public virtual void WriteToConsole() { }
    }
}