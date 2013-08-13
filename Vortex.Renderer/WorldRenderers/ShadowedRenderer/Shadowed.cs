using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core;
using Psy.Core.Console;
using Psy.Graphics;
using Psy.Graphics.Effects;
using Psy.Graphics.Models.Renderers;
using Psy.Graphics.VertexDeclarations;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.Renderer.Camera;
using Vortex.World.Observable;

namespace Vortex.Renderer.WorldRenderers.ShadowedRenderer
{
    class Shadowed : BaseWorldRenderer
    {
        private const int DefaultTriangleListLength = 30;
        private const int DefaultMaterialListLength = 10;
        private const float TextureScaleFactor = 8f;
        private const float TextureScale = (Chunk.ChunkWorldSize / TextureScaleFactor);
        private const int CubeMapSize = 256;

        public const bool RenderTerrain = true;

        private const float EntityRenderViewRadiusSquared = (Chunk.ChunkWorldSize * 1.2f) * (Chunk.ChunkWorldSize * 1.2f);
        private const float LightRenderViewRadiusSquared = (Chunk.ChunkWorldSize * 1.5f) * (Chunk.ChunkWorldSize * 1.5f);

        private bool _shouldGenerateGeometry;
        private readonly MaterialCache _materialCache;
        private readonly GraphicsContext _graphicsContext;
        private readonly List<MaterialVertexBatch> _materialVertexBatches;
        private ICubeTexture _cubeTexture;
        private int _renderOpSoloLight;
        private ISurface _defaultRenderTarget;
        private readonly Matrix _lightCameraProj;
        private ITexture _cubeMapDepthStencil;
        private AnyCamera _currentCamera;
        private Matrix _cameraProjectionTransform;
        private float _renderDepthSnapshot;

        private readonly IEffectHandle _worldMatHandle;
        private readonly IEffectHandle _worldViewProjMatHandle;

        private double _ambientPassLast;
        private double _ambientPassAvg;
        private double _depthPassLast;
        private double _depthPassAvg;
        private double _lightPassLast;
        private double _lightPassAvg;
        private double _lightCountLast;
        private double _actualLast;
        private double _actualAvg;
        private int _entityRenderCountFull;
        private int _entityRenderCountBasic;
        private List<Entity> _entityList;
        private Matrix _groundWorldMatrix;
        private double _depthPassAB;
        private double _depthPassBC;
        private double _depthPassCD;
        private double _depthPassDE;
        private double _depthPassVW;
        private double _depthPassWX;
        private double _depthPassXY;
        private double _depthPassYZ;
        private double _depthPassZO;

        private int _cameraDistanceCull;
        private int _lightDistanceCull;
        private int _entityFrustumCull;
        private readonly IEffect _fx;
        private readonly IEffectHandle _lightpositionHandle;
        private readonly IEffectHandle _lightdiffuseHandle;
        private readonly IEffectHandle _lightintensityHandle;
        private readonly IEffectHandle _cubeshadowmapHandle;
        private readonly IEffectHandle _cubicShadowMapTechniqueHandle;
        private readonly IEffectHandle _depthMapTechniqueHandle;
        private readonly IEffectHandle _ambientTechniqueHandle;
        private readonly IEffectHandle _ambientColourHandle;

        private readonly IEffectHandle _viewDistanceHandle;
        private readonly IEffectHandle _viewAngleHandle;
        private readonly IEffectHandle _viewingAngleHandle;
        private readonly IEffectHandle _minViewDistanceHandle;

        private ISurface _cubeMapDepthStencilSurface;
        private ISurface _defaultDepthStencilSurface;
        private readonly ModelInstanceRenderer _modelInstanceRenderer;
        private Vector2 _bottomLeft;

        public Shadowed(GraphicsContext graphicsContext, IObservableArea observableArea, MaterialCache materialCache)
            : base(observableArea)
        {
            _fx = graphicsContext.CreateEffect("omniShadow2.fx");

            _worldMatHandle = _fx.CreateHandle("worldMat");
            _worldViewProjMatHandle = _fx.CreateHandle("worldViewProjMat");
            _lightpositionHandle = _fx.CreateHandle("lightPosition");
            _lightdiffuseHandle = _fx.CreateHandle("lightDiffuse");
            _lightintensityHandle = _fx.CreateHandle("lightIntensity");
            _cubeshadowmapHandle = _fx.CreateHandle("cubeShadowMap");
            _cubicShadowMapTechniqueHandle = _fx.CreateHandle("cubicShadowMapping");
            _depthMapTechniqueHandle = _fx.CreateHandle("depthMap");
            _ambientTechniqueHandle = _fx.CreateHandle("ambient");
            _ambientColourHandle = _fx.CreateHandle("ambientColour");

            _viewDistanceHandle = _fx.CreateHandle("viewDistance");
            _minViewDistanceHandle = _fx.CreateHandle("minViewRange");
            _viewAngleHandle = _fx.CreateHandle("viewAngle");
            _viewingAngleHandle = _fx.CreateHandle("viewingAngle");

            _graphicsContext = graphicsContext;

            _shouldGenerateGeometry = false;

            _materialCache = materialCache;
            _materialVertexBatches = new List<MaterialVertexBatch>();

            _lightCameraProj = Matrix.PerspectiveFovLH(
                (float)(Math.PI / 2.0f), 1.0f, 0.5f, 25.0f);

            _modelInstanceRenderer = new ModelInstanceRenderer(_graphicsContext);
            ObservableArea.Updated += ObservableAreaUpdated;

            _renderOpSoloLight = 0; // render all lights.
            _renderDepthSnapshot = 0;

            StaticConsole.Console.RegisterFloat("snap",
                () => _renderDepthSnapshot,
                delegate(float f) { _renderDepthSnapshot = f; });

            StaticConsole.Console.RegisterFloat("r_lisolo",
                () => _renderOpSoloLight,
                delegate(float f) { _renderOpSoloLight = (int)f; });

            //Window.DevicePostReset += WindowOnDevicePostReset;
            //Window.DevicePreReset += WindowOnDevicePreReset;
        }
        /*
        private void WindowOnDevicePreReset()
        {
            DisposeCubeSurfaces();
            if (_defaultRenderTarget != null)
            {
                _defaultRenderTarget.Dispose();
                _defaultRenderTarget = null;
            }

            if (_defaultDepthStencilSurface != null)
            {
                _defaultDepthStencilSurface.Dispose();
                _defaultDepthStencilSurface = null;
            }

            if (_cubeMapDepthStencilSurface != null)
            {
                _cubeMapDepthStencilSurface.Dispose();
                _cubeMapDepthStencilSurface = null;
            }
        }

        private void WindowOnDevicePostReset()
        {
            InitializeSurfaces();
            _defaultRenderTarget = _graphicsContext.RenderTarget;
        }
        */
        private void DisposeCubeSurfaces()
        {
            if (_cubeMapDepthStencil != null)
            {
                _cubeMapDepthStencil.Dispose();
                _cubeMapDepthStencil = null;
            }
        }

        public override void Dispose()
        {
            DisposeMaterialVertexBatches();
            DisposeCubeSurfaces();
            //_modelRenderer.Dispose();
            _modelInstanceRenderer.Dispose();
        }

        /// <summary>
        /// Dump stats to console.
        /// </summary>
        public override void WriteToConsole()
        {
            var console = StaticConsole.Console;

            var extrapolatedLast = _ambientPassLast + ((_depthPassLast + _lightPassLast) * _lightCountLast);
            var extrapolatedAvg = _ambientPassAvg + ((_depthPassAvg + _lightPassAvg) * _lightCountLast);

            console.AddLine("RENDER STATISTICS");
            console.AddLine("==========================");
            console.AddLine(string.Format("Ambient: {0}/{1}", _ambientPassLast, _ambientPassAvg));
            console.AddLine(string.Format("Lights: {0}", _lightCountLast));
            console.AddLine(string.Format("DepthPass Per Light: {0}/{1}", _depthPassLast, _depthPassAvg));
            console.AddLine(string.Format("ShadowedPass Per Light: {0}/{1}", _lightPassLast, _lightPassAvg));
            console.AddLine(string.Format("Extrapolated: {0}/{1}", extrapolatedLast, extrapolatedAvg));
            console.AddLine(string.Format("ACTUAL: {0}/{1}", _actualLast, _actualAvg));
            console.AddLine(string.Format("Cull: CAM:{0} LIGHT:{1} FRUSTUM:{2}", _cameraDistanceCull, _lightDistanceCull, _entityFrustumCull));
            console.AddLine(string.Format("Entity render count: full:{0} basic:{1}", _entityRenderCountFull, _entityRenderCountBasic));
            console.AddLine(
                string.Format("DepthTime: AB:{0}, BC:{1}, CD:{2}, DE:{3}",
                _depthPassAB, _depthPassBC, _depthPassCD, _depthPassDE));
            console.AddLine(
                string.Format("DepthTime: VW:{0}, WX:{1}, XY:{2}, YZ:{3} ZO:{4}",
                _depthPassVW, _depthPassWX, _depthPassXY, _depthPassYZ, _depthPassZO));
        }

        void ObservableAreaUpdated(ObservableArea item)
        {
            _shouldGenerateGeometry = true;
        }

        private void InitializeSurfaces()
        {
            if (_cubeTexture == null)
            {
                _cubeTexture = _graphicsContext.CreateCubeTexture(CubeMapSize, UsageType.RenderTarget, FormatType.R32F);
            }

            if (_cubeMapDepthStencil == null)
            {
                _cubeMapDepthStencil = _graphicsContext.CreateTexture(CubeMapSize, CubeMapSize, UsageType.DepthStencil, FormatType.D16);

                _cubeMapDepthStencil.DebugName = "_cubeMapDepthStencil";
            }

            // todo
            if (_cubeMapDepthStencilSurface == null)
            {
                _cubeMapDepthStencilSurface = _cubeMapDepthStencil.Surface;
                _cubeMapDepthStencilSurface.DebugName = "_cubeMapDepthStencilSurface";
            }

            // todo
            if (_defaultDepthStencilSurface == null)
            {
                _defaultDepthStencilSurface = _graphicsContext.DepthStencilSurface;
            }

            if (_defaultRenderTarget == null)
            {
                _defaultRenderTarget = _graphicsContext.RenderTarget;
            }
        }

        public override void Render(float range, float angle, float minrange, float rotation,
            IEnumerable<Entity> entities, AnyCamera camera, Matrix projectionTransform)
        {
            _entityRenderCountFull = 0;
            _entityRenderCountBasic = 0;
            _cameraDistanceCull = 0;
            _lightDistanceCull = 0;
            _entityFrustumCull = 0;
            _depthPassAB = 0;
            _depthPassBC = 0;
            _depthPassCD = 0;
            _depthPassDE = 0;
            _depthPassVW = 0;
            _depthPassWX = 0;
            _depthPassXY = 0;
            _depthPassYZ = 0;
            _depthPassZO = 0;

            var actualStart = Timer.GetTime();

            if (_materialVertexBatches.Count == 0)
                return;

            InitializeSurfaces();
            var entityList = entities.ToList();
            _entityList = entityList;
            _currentCamera = camera;
            GetGroundWorldMatrix();

            _graphicsContext.RenderTarget = _defaultRenderTarget;

            _graphicsContext.ClearColour = new Color4(1, 1, 1, 1);
            _graphicsContext.Clear(1.0f);

            // might want to move these out of every render loop?
            while (rotation < 0)
                rotation += (float) (Math.PI*2);
            while (rotation > Math.PI * 2)
                rotation -= (float) (Math.PI*2);

            _fx.SetValue(_viewDistanceHandle, range);
            _fx.SetValue(_minViewDistanceHandle, minrange);
            _fx.SetValue(_viewAngleHandle, angle);
            _fx.SetValue(_viewingAngleHandle, rotation);

            var viewTransform = camera.GetViewTransformCameraCenteredSpace();
            _cameraProjectionTransform = projectionTransform;

            AmbientPass(viewTransform, projectionTransform, _fx);
            Shadowing(viewTransform, projectionTransform, _fx);
            SetFixedFunctionTransformations();

            var actualEnd = Timer.GetTime();

            _actualLast = actualEnd - actualStart;
            _actualAvg += _actualLast;
            _actualAvg /= 2;
        }

        private void SetFixedFunctionTransformations()
        {
            var worldMatrix = GetGroundWorldMatrix();

            _graphicsContext.World = worldMatrix;
            _graphicsContext.View = _currentCamera.GetViewTransform();
            _graphicsContext.Projection = _cameraProjectionTransform;
        }

        private void AmbientPass(Matrix viewTransform, Matrix projectionTransform, IEffect fx)
        {
            var ambientPassStart = Timer.GetTime();
            
            fx.Technique = _ambientTechniqueHandle;
            fx.Begin();
            fx.SetMatrix(_worldMatHandle, _groundWorldMatrix);
            fx.SetMatrix(_worldViewProjMatHandle, _groundWorldMatrix * viewTransform * projectionTransform);

            if (RenderTerrain)
            {
                foreach (var materialVertexBatch in _materialVertexBatches)
                {
                    fx.SetTexture("tex0", materialVertexBatch.TextureArea);

                    fx.SetValue(_ambientColourHandle,
                                materialVertexBatch.Material.Outside
                                    ? ObservableArea.OutsideLightingColour.OutsideLightingColour
                                    : new Color4(1.0f, 0.8f, 0.8f, 0.8f));

                    fx.BeginPass(0);
                    materialVertexBatch.Renderer.RenderForShader(
                        PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount);
                    fx.EndPass();
                }
            }

            fx.SetValue(_ambientColourHandle, new Color4(1.0f, 0.5f, 0.5f, 0.5f));
            RenderEntities(viewTransform, projectionTransform, fx);
            fx.End();

            var ambientPassEnd = Timer.GetTime();

            _ambientPassLast = ambientPassEnd - ambientPassStart;
            _ambientPassAvg += _ambientPassLast;
            _ambientPassAvg /= 2;
        }

        private void Shadowing(Matrix viewTransform, Matrix projectionTransform, IEffect fx)
        {
            int lightIdx = 0;

            _lightCountLast = ObservableArea.Lights.Count;

            foreach (var light in ObservableArea.Lights)
            {
                var lightDistance = (light.Position - _currentCamera.Vector).LengthSquared;
                if (lightDistance > LightRenderViewRadiusSquared)
                    continue;

                if (_renderOpSoloLight != 0 && _renderOpSoloLight != lightIdx)
                {
                    lightIdx++;
                    continue;
                }
                RenderLight(fx, light, viewTransform, projectionTransform);
                lightIdx++;
            }
        }

        private void RenderLight(IEffect fx, ILight light,
            Matrix viewTransform, Matrix projectionTransform)
        {
            var lightPos = (light.Position - _currentCamera.Vector);

            RenderDepth(fx, lightPos, light);
            RenderShadowedMeshes(fx, light, _entityList, viewTransform, projectionTransform);
        }

        private void RenderDepth(IEffect fx, Vector3 lightPos, ILight light)
        {

            var depthPassStart = Timer.GetTime();

            _graphicsContext.SingleChannelColourWrite = true;

            fx.SetValue(_lightpositionHandle, lightPos);
            fx.SetValue(_lightdiffuseHandle, light.Colour);
            fx.SetValue(_lightintensityHandle, light.Brightness * light.Brightness);
            fx.Technique = _depthMapTechniqueHandle;

            DepthPass(lightPos, light, fx, CubeMapFaceEnum.PositiveX);
            DepthPass(lightPos, light, fx, CubeMapFaceEnum.NegativeX);
            DepthPass(lightPos, light, fx, CubeMapFaceEnum.NegativeY);
            DepthPass(lightPos, light, fx, CubeMapFaceEnum.PositiveY);
            DepthPass(lightPos, light, fx, CubeMapFaceEnum.PositiveZ);
            DepthPass(lightPos, light, fx, CubeMapFaceEnum.NegativeZ);

            if (_renderDepthSnapshot > 0)
            {
                _renderDepthSnapshot = 0;
            }

            _graphicsContext.SingleChannelColourWrite = false;

            var depthPassEnd = Timer.GetTime();

            _depthPassLast = depthPassEnd - depthPassStart;
            _depthPassAvg += _depthPassLast;
            _depthPassAvg /= 2;
        }

        private Matrix GetGroundWorldMatrix()
        {
            var groundPosition = _bottomLeft.AsVector3() - _currentCamera.Vector;
            var world = Matrix.Translation(groundPosition.X, groundPosition.Y, 0);
            _groundWorldMatrix = world;
            return world;
        }

        private void DepthPass(Vector3 lightPos, ILight light, IEffect fx, CubeMapFaceEnum face)
        {
            var lightDistance = Math.Pow(light.Brightness, 4);

            var a = Timer.GetTime();

            _graphicsContext.RenderTarget = _cubeTexture.CubeMapSurface[(int) face];
            _graphicsContext.DepthStencilSurface = _cubeMapDepthStencilSurface;
            _graphicsContext.Clear(new Color4(0, 0, 0, 0), 1.0f);

            fx.Begin();

            var cameraViewMatrix = GetCameraViewMatrix(lightPos, face);

            fx.SetMatrix(_worldMatHandle, _groundWorldMatrix);
            fx.SetMatrix(_worldViewProjMatHandle, _groundWorldMatrix * cameraViewMatrix * _lightCameraProj);

            var b = Timer.GetTime();

            if (RenderTerrain)
            {
                foreach (var materialVertexBatch in _materialVertexBatches)
                {
                    fx.BeginPass(0);
                    materialVertexBatch.Renderer.RenderForShader(
                        PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount);
                    fx.EndPass();
                }
            }

            var c = Timer.GetTime();

            fx.BeginPass(0);

            _modelInstanceRenderer.BeginRender();

            foreach (var entity in _entityList)
            {
                var v = Timer.GetTime();

                var entityPosition = entity.GetPosition();

                var distance = entityPosition.DistanceSquared(light.Position);

                if (distance > lightDistance)
                    continue;

                if (face == CubeMapFaceEnum.PositiveX && entityPosition.X < light.Position.X)
                {
                    _entityFrustumCull++;
                    continue;
                }
                if (face == CubeMapFaceEnum.NegativeX && entityPosition.X > light.Position.X)
                {
                    _entityFrustumCull++;
                    continue;
                }
                if (face == CubeMapFaceEnum.PositiveY && entityPosition.Y < light.Position.Y)
                {
                    _entityFrustumCull++;
                    continue;
                }
                if (face == CubeMapFaceEnum.NegativeY && entityPosition.Y > light.Position.Y)
                {
                    _entityFrustumCull++;
                    continue;
                }
                if (face == CubeMapFaceEnum.PositiveZ && entityPosition.Z < light.Position.Z)
                {
                    _entityFrustumCull++;
                    continue;
                }
                if (face == CubeMapFaceEnum.NegativeZ && entityPosition.Z > light.Position.Z)
                {
                    _entityFrustumCull++;
                    continue;
                }

                var w = Timer.GetTime();

                var world = MatrixHelper.GetEntityWorldMatrix(_currentCamera, entity);

                var x = Timer.GetTime();

                var matrix = world * cameraViewMatrix * _lightCameraProj;

                var y = Timer.GetTime();

                fx.SetMatrix(_worldMatHandle, world);
                fx.SetMatrix(_worldViewProjMatHandle, matrix);

                //var y = Timer.GetTime();

                fx.CommitChanges();

                var z = Timer.GetTime();

                _modelInstanceRenderer.Render(entity.Model.ModelInstance, fx);

                _entityRenderCountBasic++;

                var o = Timer.GetTime();

                _depthPassVW += w - v;
                _depthPassWX += x - w;
                _depthPassXY += y - x;
                _depthPassYZ += z - y;
                _depthPassZO += o - z;
            }

            fx.EndPass();

            fx.End();

            var d = Timer.GetTime();

            _graphicsContext.CullMode = CullMode.None;
            _graphicsContext.RenderTarget = _defaultRenderTarget;
            _graphicsContext.DepthStencilSurface = _defaultDepthStencilSurface;

            var e = Timer.GetTime();

            _depthPassAB += b - a;
            _depthPassBC += c - b;
            _depthPassCD += d - c;
            _depthPassDE += e - d;
        }

        private void RenderEntities(Matrix viewTransform, Matrix projectionTransform,
            IEffect fx, bool initialPass = false, bool forLighting = false)
        {

            fx.BeginPass(0);

            _modelInstanceRenderer.BeginRender();

            foreach (var entity in _entityList)
            {
                if (entity.Model == null)
                    continue;

                var distanceFromCamera = entity.GetPosition().DistanceSquared(_currentCamera.Vector);
                if (distanceFromCamera > EntityRenderViewRadiusSquared)
                    continue;

                var world = MatrixHelper.GetEntityWorldMatrix(_currentCamera, entity);

                fx.SetMatrix(_worldMatHandle, world);
                fx.SetMatrix(_worldViewProjMatHandle, world * viewTransform * projectionTransform);
                fx.CommitChanges();

                if (forLighting)
                {
                    _modelInstanceRenderer.RenderNoTexture(entity.Model.ModelInstance);
                    _entityRenderCountBasic++;

                }
                else
                {
                    _modelInstanceRenderer.Render(entity.Model.ModelInstance, fx);

                    foreach (var subModel in entity.Model.ModelInstance.SubModels)
                    {
                        var subWorld = subModel.GetWorldMatrix() * world;
                        fx.SetMatrix(_worldMatHandle, subWorld);
                        fx.SetMatrix(_worldViewProjMatHandle, subWorld * viewTransform * projectionTransform);
                        fx.CommitChanges();

                        _modelInstanceRenderer.Render(subModel.ModelInstance, fx);
                    }

                    _entityRenderCountFull++;
                }
            }

            fx.EndPass();
        }

        private void RenderShadowedMeshes(IEffect fx, ILight light, List<Entity> entities,
            Matrix viewTransform, Matrix projectionTransform)
        {
            var shadowedStart = Timer.GetTime();

            _graphicsContext.SingleChannelColourWrite = false;
            _graphicsContext.RenderTarget = _defaultRenderTarget;
            _graphicsContext.CullMode = CullMode.CCW;

            fx.Technique = _cubicShadowMapTechniqueHandle;
            fx.SetMatrix(_worldMatHandle, _groundWorldMatrix);
            fx.SetMatrix(_worldViewProjMatHandle, _groundWorldMatrix * viewTransform * projectionTransform);
            fx.SetTexture(_cubeshadowmapHandle, _cubeTexture);

            fx.Begin();

            foreach (var materialVertexBatch in _materialVertexBatches)
            {
                fx.SetTexture("tex0", materialVertexBatch.TextureArea);

                fx.BeginPass(0);
                materialVertexBatch.Renderer
                    .RenderForShader(PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount);
                fx.EndPass();
            }

            fx.BeginPass(0);

            var lightDistance = Math.Pow(light.Brightness, 4);

            _modelInstanceRenderer.BeginRender();

            foreach (var entity in (IEnumerable<Entity>)entities)
            {
                var position = entity.GetPosition();

                var distanceFromCamera = position.DistanceSquared(_currentCamera.Vector);
                if (distanceFromCamera > EntityRenderViewRadiusSquared)
                {
                    _cameraDistanceCull++;
                    continue;
                }

                var distanceFromLight = position.DistanceSquared(light.Position);
                if (distanceFromLight > lightDistance)
                {
                    _lightDistanceCull++;
                    continue;
                }

                var entityWorldMatrix = MatrixHelper.GetEntityWorldMatrix(_currentCamera, entity);

                fx.SetMatrix(_worldMatHandle, entityWorldMatrix);
                fx.SetMatrix(_worldViewProjMatHandle, entityWorldMatrix * viewTransform * projectionTransform);
                fx.CommitChanges();

                _modelInstanceRenderer.Render(entity.Model.ModelInstance, fx);
                _entityRenderCountFull++;
            }

            fx.EndPass();

            fx.End();

            _graphicsContext.CullMode = CullMode.None;

            var shadowedEnd = Timer.GetTime();

            _lightPassLast = shadowedEnd - shadowedStart;
            _lightPassAvg += _lightPassLast;
            _lightPassAvg /= 2;
        }

        private static Matrix GetCameraViewMatrix(Vector3 lightPos, CubeMapFaceEnum face)
        {
            var x = new Vector3(1.0f, 0.0f, 0.0f);
            var y = new Vector3(0.0f, 1.0f, 0.0f);
            var z = new Vector3(0.0f, 0.0f, 1.0f);

            var v3 = lightPos;

            switch (face)
            {
                case CubeMapFaceEnum.NegativeZ:
                    return Matrix.LookAtLH(v3, v3 - z, y);
                case CubeMapFaceEnum.PositiveZ:
                    return Matrix.LookAtLH(v3, v3 + z, y);
                case CubeMapFaceEnum.NegativeY:
                    return Matrix.LookAtLH(v3, v3 - y, z);
                case CubeMapFaceEnum.PositiveY:
                    return Matrix.LookAtLH(v3, v3 + y, -z);
                case CubeMapFaceEnum.NegativeX:
                    return Matrix.LookAtLH(v3, v3 - x, y);
                case CubeMapFaceEnum.PositiveX:
                    return Matrix.LookAtLH(v3, v3 + x, y);
                default:
                    throw new ArgumentOutOfRangeException("face");
            }
        }

        /// <summary>
        /// Prepare a visible area of the map for rendering.
        /// </summary>
        /// <param name="camera"></param>
        public override void Prepare(BasicCamera camera)
        {
            if (ObservableArea.ChunkMeshes.Count == 0)
                return;

            if (!_shouldGenerateGeometry)
                return;
            _shouldGenerateGeometry = false;

            _bottomLeft = ObservableArea.BottomLeft;

            GenerateGeometry();
        }

        private void GenerateGeometry()
        {
            var materialBatches = CreateMaterialBatches();

            DisposeMaterialVertexBatches();

            foreach (var materialBatch in materialBatches)
            {
                var materialVertexBatch = CreateMaterialVertexBatch(materialBatch);
                _materialVertexBatches.Add(materialVertexBatch);
            }
        }

        private void DisposeMaterialVertexBatches()
        {
            foreach (var materialVertexBatch in _materialVertexBatches)
            {
                materialVertexBatch.Dispose();
            }
            _materialVertexBatches.Clear();
        }

        private Dictionary<int, List<TriangleKey>> CreateMaterialBatches()
        {
            var materialBatch = new Dictionary<int, List<TriangleKey>>(DefaultMaterialListLength);
            for (var meshIdx = 0; meshIdx < ObservableArea.ChunkMeshes.Count; meshIdx++)
            {
                var chunkMesh = ObservableArea.ChunkMeshes[meshIdx];

                for (var triIdx = 0; triIdx < chunkMesh.Triangles.Count; triIdx++)
                {
                    var triangle = chunkMesh.Triangles[triIdx];
                    var triangleMaterial = triangle.Material;

                    if (!materialBatch.ContainsKey(triangleMaterial))
                    {
                        materialBatch[triangleMaterial] = new List<TriangleKey>(DefaultTriangleListLength);
                    }

                    materialBatch[triangleMaterial].Add(new TriangleKey(meshIdx, triIdx));
                }
            }
            return materialBatch;
        }

        private MaterialVertexBatch CreateMaterialVertexBatch(
            KeyValuePair<int, List<TriangleKey>> materialBatch)
        {
            var materialIndex = materialBatch.Key;
            var material = _materialCache[materialIndex];
            var triangleCount = materialBatch.Value.Count;

            var renderer = _graphicsContext.CreateVertexRenderer<ColouredTexturedVertexNormal4>(triangleCount*3);

            var dataStream = renderer.LockVertexBuffer();

            var observableAreaBottomLeft = ObservableArea.BottomLeft.AsVector3();

            foreach (var triangleKey in materialBatch.Value)
            {
                var chunkMesh = ObservableArea.ChunkMeshes[triangleKey.ChunkMeshIndex];
                var triangle = chunkMesh.Triangles[triangleKey.TriangleIndex];
                var offset = chunkMesh.WorldVector - observableAreaBottomLeft;

                var color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                var position0 = (triangle.P0 + offset);
                var position1 = (triangle.P1 + offset);
                var position2 = (triangle.P2 + offset);
                var normal = new Vector3(0, 0, -1.0f);

                var uv0 = (triangle.P0 / TextureScale).Scale(1, -1, 1).AsVector2();
                var uv1 = (triangle.P1 / TextureScale).Scale(1, -1, 1).AsVector2();
                var uv2 = (triangle.P2 / TextureScale).Scale(1, -1, 1).AsVector2();

                dataStream.WriteRange(
                    new[]
                    {
                        new ColouredTexturedVertexNormal4(position0.AsVector4(), color, normal, uv0),
                        new ColouredTexturedVertexNormal4(position1.AsVector4(), color, normal, uv1),
                        new ColouredTexturedVertexNormal4(position2.AsVector4(), color, normal, uv2)
                    });
            }

            renderer.UnlockVertexBuffer();

            var textureName = _materialCache[materialIndex].TextureName;
            var texture = _graphicsContext.GetTexture(textureName);

            return new MaterialVertexBatch(renderer, triangleCount, texture, material);
        }
    }
}