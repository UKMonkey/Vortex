using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core;
using SlimMath;
using Vortex.Interface.EntityBase.Properties;
using Psy.Core.Console;
using Psy.Graphics;
using Vortex.Interface.EntityBase;
using Vortex.Renderer.Blood;
using Vortex.Renderer.Camera;
using Vortex.Renderer.Weather;
using Vortex.Renderer.WorldRenderers;
using Vortex.Renderer.WorldRenderers.ShadowedRenderer;
using Vortex.World.Interfaces;

namespace Vortex.Renderer
{
    public class View : IDisposable
    {
        private const float SmoothZoomStep = 2.0f;
        private const int DefaultZoomDistance = 20;
        private const int MaximumZoomOutDistance = 27;
        private const int MinimumZoomOutDistance = 5;

        private readonly GraphicsContext _graphicsContext;
        private RainRenderer _rainRenderer;
        private readonly LightningRenderer _lightningRenderer;
        private readonly MeshCollisionRenderer _meshCollisionRenderer;
        private Matrix _perspectiveMatrix;
        private readonly RenderResult _renderResult;
        private World.World _world;
        private IWorldRenderer _worldRenderer;
        public readonly RayTraceRenderer RayTraceRenderer;
        public readonly BloodRenderer BloodRenderer;
        private bool _renderCollisionMesh;
        public AnyCamera Camera { get; private set; }
        private readonly EntityNameplateRenderer _entityNameplateRenderer;
        private readonly MaterialCache _materialCache;
        public EntityCollection EntityCollectionSystem { get; set; }


        public float MinViewRange { get; set; }
        public float ViewRange { get; set; }
        public float ViewAngle { get; set; }
        public float ViewDirection { get; set; }

        public ICamera CameraPosition
        {
            get { return Camera.InnerCamera; }
            set { if (value != null) Camera.InnerCamera = value; }
        }

        public bool HasWorld
        {
            get { return _world != null; }
        }

        private float _renderedZoomDistance;
        private float _zoomDistance;
        private readonly ViewPerformanceMeasurements _viewPerformanceMeasurements;

        public float ZoomDistance
        {
            get { return _zoomDistance; }
            set
            {
                if (value > MaximumZoomOutDistance || value < MinimumZoomOutDistance)
                    return;
                _zoomDistance = value;
            }
        }

        public View(GraphicsContext graphicsContext, MaterialCache materialCache)
        {
            MinViewRange = 0;
            ViewRange = 0;
            ViewAngle = 0;
            ViewDirection = 0;

            _graphicsContext = graphicsContext;
            _materialCache = materialCache;

            _graphicsContext.LoadTextureAtlases("_atlases.adfm");

            ZoomDistance = DefaultZoomDistance;
            _renderedZoomDistance = ZoomDistance;

            _viewPerformanceMeasurements = new ViewPerformanceMeasurements();
            
            _renderCollisionMesh = false;
            _renderResult = new RenderResult();
            _perspectiveMatrix = Matrix.Identity;
            
            Camera = new AnyCamera(_graphicsContext, CreateManualCamera());

            _lightningRenderer = new LightningRenderer(_graphicsContext);
            _entityNameplateRenderer = new EntityNameplateRenderer(_graphicsContext);

            _meshCollisionRenderer = new MeshCollisionRenderer(_graphicsContext);
            RayTraceRenderer = new RayTraceRenderer(_graphicsContext);
            BloodRenderer = new BloodRenderer(_graphicsContext);

            StaticConsole.Console.RegisterFloat("cmesh", 
                () => _renderCollisionMesh ? 1 : 0, 
                delegate(float f) { _renderCollisionMesh = f > 0; });

            StaticConsole.Console.CommandBindings
                .Bind("vstat", "Renderer statistics", HandleVStatCommand);

            EntityCollectionSystem = GetEntities;
#if DEBUG
            StaticConsole.Console.CommandBindings
                .Bind("sdump", "Dump SlimDX info to file", HandleSDumpCommand);
#endif
        }

        private IEnumerable<Entity> GetEntities()
        {
            return _world.GetObservedEntities();
        }

        private static void HandleSDumpCommand(string[] parameters)
        {
            //SlimDXUtils.DumpObjectTable();
            StaticConsole.Console.AddLine("SlimDX data dumped to file");
        }

        private void HandleVStatCommand(params string[] parameters)
        {
            if (_worldRenderer == null)
                return;

            _worldRenderer.WriteToConsole();
            _viewPerformanceMeasurements.WriteToConsole();
        }

        public void Dispose()
        {
            if (_worldRenderer != null) _worldRenderer.Dispose();
            if (_rainRenderer != null) _rainRenderer.Dispose();
            if (_lightningRenderer != null) _lightningRenderer.Dispose();
            if (_meshCollisionRenderer != null) _meshCollisionRenderer.Dispose();
            if (RayTraceRenderer != null) RayTraceRenderer.Dispose();
            if (BloodRenderer != null) BloodRenderer.Dispose();
        }

        public void UseWorld(World.World world)
        {
            _world = world;
            var observableArea = world.GetMap().AddCamera(Camera);
            _worldRenderer = new Shadowed(_graphicsContext, observableArea, _materialCache);
            _rainRenderer = new RainRenderer(_graphicsContext, observableArea, _materialCache);
        }

        public void UnloadWorld()
        {
            _world = null;

            if (_worldRenderer != null)
                _worldRenderer.Dispose();

            _worldRenderer = null;
        }

        public ManualCamera CreateManualCamera()
        {
            return new ManualCamera(_graphicsContext, new Vector3(0, 0, 0));
        }

        public EntityFollowCamera CreateEntityFollowCamera(Entity entityToFollow)
        {
            return new EntityFollowCamera(_graphicsContext, entityToFollow);
        }

        public void InitializeRenderState()
        {
            if (_perspectiveMatrix == Matrix.Identity)
                SetPerspectiveMatrix();

            _graphicsContext.Projection = _perspectiveMatrix;
            _graphicsContext.View = Matrix.Identity;
            _graphicsContext.World = Matrix.Identity;
        }

        public void SetPerspectiveMatrix()
        {
            _perspectiveMatrix = MatrixUtils.GetPerspectiveFovLH(_graphicsContext);
        }

        public void Render()
        {
            if (_world == null)
                return;

            if (_worldRenderer == null)
                return;

            _renderResult.Reset();

            _renderResult.IncrementStateChange(3);

            _graphicsContext.MipFilter = TextureFilter.Anisotropic;
            _graphicsContext.MinFilter = TextureFilter.Anisotropic;
            _graphicsContext.MagFilter = TextureFilter.Anisotropic;

            var entities = EntityCollectionSystem().ToList();

            RenderWorld(entities);
            RenderCollisionMesh(entities);
            RendeEntityNameplates(entities);

            if (_world.IsRaining)
            {
                RenderRain();
            }

            /*
            var viewTransform = Camera.GetViewTransform();
            _device.SetTransform(TransformState.View, viewTransform);
            _entityNameplateRenderer.Render(this, entities);
            _device.SetTransform(TransformState.World, Matrix.Identity);
            
            BloodRenderer.Render();
            _lightningRenderer.Render();
            
             */

            // note: do not remove. required for ScreenToWorld stuff.
            
            //_device.SetTransform(TransformState.View, Camera.GetViewTransform());

            _graphicsContext.View = Camera.GetViewTransform();
            _graphicsContext.World = Matrix.Identity;

            //_device.SetTransform(TransformState.World, Matrix.Identity);
        }

        private void UpdateCamera()
        {
            Camera.Update();
            Camera.ZoomDistance = _renderedZoomDistance;
        }

        private void RenderWorld(IEnumerable<Entity> entities)
        {
            _viewPerformanceMeasurements.WorldRender.Begin();
            _worldRenderer.Render(ViewRange, ViewAngle, MinViewRange, ViewDirection,
                entities, Camera, _perspectiveMatrix);
            _viewPerformanceMeasurements.WorldRender.End();
        }

        private void RendeEntityNameplates(IEnumerable<Entity> entities)
        {
            _viewPerformanceMeasurements.NameplateRender.Begin();
            _entityNameplateRenderer.Render(this, entities);
            _viewPerformanceMeasurements.NameplateRender.End();
        }

        private void RenderRain()
        {
            _viewPerformanceMeasurements.RainRender.Begin();
            _rainRenderer.Render(Camera.GetViewTransform(), _perspectiveMatrix);
            _viewPerformanceMeasurements.RainRender.End();
        }

        private void RenderCollisionMesh(List<Entity> entities)
        {
            if (_renderCollisionMesh)
            {
                RayTraceRenderer.Render();

                var meshes = entities
                    .Where(e => e.Mesh != null)
                    .Select(e => e.Mesh);

                var viewTransform = Camera.GetViewTransform();
                _meshCollisionRenderer.Render(meshes, viewTransform, _perspectiveMatrix);

                // todo: re-enable this when we generate collision meshes for the ground.
                //_meshCollisionRenderer.Render(_observableAreaRenderer.ObservableArea.CollisionTester.Meshes);
            }
        }

        public void Update()
        {
            UpdateCamera();
            UpdateMap();
            UpdateRain();
            RayTraceRenderer.Update();
            UpdateZoomDistance();
        }

        private void UpdateRain()
        {
            if (_rainRenderer == null)
                return;

            _viewPerformanceMeasurements.RainUpdate.Begin();
            _rainRenderer.Update(Camera.Vector);
            _viewPerformanceMeasurements.RainUpdate.End();
        }

        private void UpdateZoomDistance()
        {
            // i have a feeling this a bit crap.
            var toZoom = 
                Math.Min(Math.Abs(_zoomDistance - _renderedZoomDistance), SmoothZoomStep);

            if (_renderedZoomDistance < _zoomDistance)
            {
                _renderedZoomDistance += toZoom;
            }
            else if (_renderedZoomDistance > _zoomDistance)
            {
                _renderedZoomDistance -= toZoom;
            }
        }

        private void UpdateMap()
        {
            if (_worldRenderer != null)
            {
                _worldRenderer.Prepare(Camera);
            }
        }
    }
}
