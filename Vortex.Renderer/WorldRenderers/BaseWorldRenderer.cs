using System.Collections.Generic;
using SlimMath;
using Vortex.Interface;
using Vortex.Interface.EntityBase;
using Vortex.Renderer.Camera;
using Vortex.World.Observable;

namespace Vortex.Renderer.WorldRenderers
{
    internal abstract class BaseWorldRenderer : IWorldRenderer
    {
        protected IObservableArea ObservableArea { get; set; }
        protected int LevelOfInterest { get { return _engine.LevelOfInterest; } }

        private readonly IClient _engine;

        protected BaseWorldRenderer(IObservableArea observableArea, IClient engine)
        {
            ObservableArea = observableArea;
            _engine = engine;
        }

        public abstract void Dispose();
        public abstract void Render(float range, float angle, float minrange, float rotation,
            IEnumerable<Entity> entities, AnyCamera camera, Matrix projectionTransform);
        public abstract void Prepare(BasicCamera camera);
        public virtual void WriteToConsole() { }
    }
}