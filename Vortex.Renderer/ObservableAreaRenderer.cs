using System;
using System.Collections.Generic;
using System.Linq;
using Beer.Interface;
using Beer.Interface.EntityBase;
using Beer.Interface.World.Chunks;
using Beer.Renderer.Camera;
using Beer.World.Observable;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.Effects;
using Psy.Graphics.Textures;
using Psy.Graphics.VertexDeclarations;
using SlimDX;
using SlimDX.Direct3D9;

namespace Beer.Renderer
{
    class ObservableAreaRenderer : IDisposable
    {
        private const int DefaultTriangleListLength = 30;
        private const int DefaultMaterialListLength = 10;
        private const float TextureScaleFactor = 2f;
        private const float TextureScale = (Chunk.ChunkWorldSize / TextureScaleFactor);

        struct TriangleKey
        {
            public readonly int ChunkMeshIndex;
            public readonly int TriangleIndex;

            public TriangleKey(int chunkMeshIndex, int triangleIndex)
            {
                ChunkMeshIndex = chunkMeshIndex;
                TriangleIndex = triangleIndex;
            }
        }

        class MaterialVertexBatch : IDisposable
        {
            public readonly VertexRenderer<ColouredTexturedVertexNormal4> Renderer;
            public readonly int TriangleCount;
            public readonly int TextureId;

            public void Dispose()
            {
                if (Renderer != null)
                    Renderer.Dispose();
            }

            public MaterialVertexBatch(
                VertexRenderer<ColouredTexturedVertexNormal4> renderer, 
                int triangleCount, int textureId)
            {
                TriangleCount = triangleCount;
                TextureId = textureId;
                Renderer = renderer;
            }
        }

        private bool _shouldGenerateGeometry;
        private readonly IMaterialCache _materialCache;
        private readonly Device _device;
        private readonly List<MaterialVertexBatch> _materialVertexBatches;
        public readonly IObservableArea ObservableArea;
        private CubeTexture _cubeTexture;

        public ObservableAreaRenderer(Device device, 
            IObservableArea observableArea, IMaterialCache materialCache)
        {
            _shouldGenerateGeometry = false;
            _device = device;
            _materialCache = materialCache;
            _materialVertexBatches = new List<MaterialVertexBatch>();

            ObservableArea = observableArea;
            ObservableArea.Updated += ObservableAreaUpdated;
        }

        public void Dispose()
        {
            DeleteMaterialVertexBatches();
        }

        private void DeleteMaterialVertexBatches()
        {
            foreach (var materialVertexBatch in _materialVertexBatches)
            {
                materialVertexBatch.Dispose();
            }
            _materialVertexBatches.Clear();
        }

        void ObservableAreaUpdated(ObservableArea item)
        {
            _shouldGenerateGeometry = true;
        }

        public void Render(EntityRenderer entityRenderer, List<Entity> entities, 
            Matrix worldTransform, Matrix viewTransform, Matrix projectionTransform, AnyCamera camera)
        {
            var fx = StaticEffectCache.EffectCache.GetEffect("default.fx");

            //_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.0f, 0.0f, 0.2f, 0.0f), 0.0f, 0);

            foreach (var light in ObservableArea.Lights)
            {
                fx.SetValue("worldMat", worldTransform);
                fx.SetValue("worldViewProjMat", worldTransform * viewTransform * projectionTransform);
                fx.SetValue("ambient", new Color4(0, 0, 0, 0));

                var lightPos = light.Position + ObservableArea.BottomLeft;

                fx.SetValue("lightPos", lightPos.ToSlimDxFormat4());
                fx.SetValue("lightIntensity", light.Brightness);
                fx.SetValue("lightColour", light.Colour);

                fx.Begin(FX.None);

                bool blend = false;
                foreach (var materialVertexBatch in _materialVertexBatches)
                {
                    fx.SetValue("alphaBlend", blend);
                    var textureId = materialVertexBatch.TextureId;
                    StaticTextureCache.TextureCache.SetShaderSampler(fx, "tex0", textureId);
                    fx.BeginPass(0);
                    materialVertexBatch.Renderer.Render(
                        PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount,
                        materialVertexBatch.TextureId);
                    fx.EndPass();
                    blend = true;
                }

                fx.End();
                
                fx.Begin(FX.None);
                
                entityRenderer.Render(viewTransform, 
                    projectionTransform, fx, entities, lightPos, true, false);
                
                fx.End();
            }
            
        }

        // NEW RENDERER - HERE BE DRA'NOGS

        public void RenderNew(EntityRenderer entityRenderer, IEnumerable<Entity> entities, Matrix worldTransform, Matrix viewTransform, Matrix projectionTransform, AnyCamera camera)
        {
            var fx = StaticEffectCache.EffectCache.GetEffect("omniShadow.fx");
            fx.SetValue("worldMat", viewTransform);
            fx.SetValue("worldViewProjMat", worldTransform * viewTransform * projectionTransform);

            var eyePosition = new Vector4(camera.Vector.ToSlimDxFormat2(), -70, 1);
            fx.SetValue("eyePosition", eyePosition);

            if (_cubeTexture == null)
            {
                _cubeTexture = new CubeTexture(_device, 512, 1, Usage.RenderTarget, Format.R32F, Pool.Default);
            }

            var defaultRenderTarget = _device.GetRenderTarget(0);
            _device.SetRenderTarget(0, defaultRenderTarget);
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0);

            var entityList = entities.ToList();

            _device.SetRenderState(RenderState.CullMode, Cull.None);
            _device.SetRenderState(RenderState.Lighting, false);
            _device.SetRenderState(RenderState.ZEnable, false);
            _device.SetRenderState(RenderState.Clipping, false);
            _device.SetRenderState(RenderState.AlphaTestEnable, false);

            foreach (var materialVertexBatch in _materialVertexBatches)
            {
                RenderNewMaterialVertexBatch(entityRenderer, entityList, defaultRenderTarget, worldTransform, 
                    viewTransform, projectionTransform, fx, materialVertexBatch);
            }
        }

        private void RenderNewMaterialVertexBatch(EntityRenderer entityRenderer, List<Entity> entities, 
            Surface defaultRenderTarget, Matrix worldTransform, Matrix viewTransform, Matrix projectionTransform, Effect fx, 
            MaterialVertexBatch materialVertexBatch)
        {
            var textureId = materialVertexBatch.TextureId;

            //var eyePosition = new Vector4(camera.Vector.ToSlimDxFormat2(), -70, 1);
            //fx.SetValue("eyePosition", eyePosition);

            int lightIdx = 0;
            foreach (var light in ObservableArea.Lights)
            {
                fx.Technique = "depthMap";

                var lightPos = light.Position + ObservableArea.BottomLeft;

                fx.SetValue("lightPosition", lightPos);
                fx.SetValue("lightDiffuse", light.Colour);
                // todo: ADD LIGHT INTENSITY BACK IN.
                //fx.SetValue("lightIntensity", light.Brightness);

                _device.SetRenderState(RenderState.ColorWriteEnable, ColorWriteEnable.Red);

                fx.Begin(FX.None);

                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.PositiveX);
                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.NegativeX);

                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.NegativeY);
                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.PositiveY);

                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.PositiveZ);
                Pass(lightIdx, entityRenderer, entities, lightPos, worldTransform, fx, materialVertexBatch, CubeMapFace.NegativeZ);
                
                fx.End();

                _device.SetRenderState(RenderState.ColorWriteEnable, ColorWriteEnable.All);
                _device.SetRenderTarget(0, defaultRenderTarget);

                fx.Technique = "cubicShadowMapping";
                fx.SetValue("worldViewProjMat", worldTransform * viewTransform * projectionTransform);
                fx.SetValue("worldMat", worldTransform);
                fx.SetTexture("cubeShadowMap", _cubeTexture);
                StaticTextureCache.TextureCache.SetShaderSampler(fx, "tex0", textureId);

                fx.Begin(FX.None);

                fx.BeginPass(0);
                materialVertexBatch.Renderer.RenderForShader(PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount);
                fx.EndPass();

                //fx.BeginPass(0);
                entityRenderer.Render(viewTransform, projectionTransform, fx, entities, lightPos, false);
                //fx.EndPass();
                
                fx.End();
                lightIdx++;
            }

            //if (ObservableArea.BottomLeft.X < -35)
            //{
            //    var i = 1;
            //}
            
        }

        private void Pass(int lightIdx, EntityRenderer entityRenderer, IEnumerable<Entity> entities, Vector lightPos, 
            Matrix worldTransform, Effect fx, MaterialVertexBatch materialVertexBatch, CubeMapFace face)
        {
            var surface = _cubeTexture.GetCubeMapSurface(face, 0);

            _device.SetRenderTarget(0, surface);
            //_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(), 0.0f, 0);

            var cameraViewMatrix = GetCameraViewMatrix(lightPos, face);
            var cameraProjMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 2.0f), 1.0f, 0.5f, 1000.0f);
            fx.SetValue("worldViewProjMat", worldTransform * cameraViewMatrix * cameraProjMatrix);
            fx.SetValue("worldMat", worldTransform);

            fx.BeginPass(0);
            materialVertexBatch.Renderer.RenderForShader(PrimitiveType.TriangleList, 0, materialVertexBatch.TriangleCount);
            fx.EndPass();

            //fx.BeginPass(0);
            entityRenderer.Render(cameraViewMatrix, cameraProjMatrix, fx, entities, lightPos);
            //fx.EndPass();

            //if (ObservableArea.BottomLeft.X < -35)
            //{
            //    Surface.ToFile(surface, string.Format(@"e:\LightPass\cubeDump-{0}-{1}.png", lightIdx, face), ImageFileFormat.Png);
            //}
        }

        private static Matrix GetCameraViewMatrix(Vector lightPos, CubeMapFace face)
        {
            var x = new Vector3(1.0f, 0.0f, 0.0f);
            var y = new Vector3(0.0f, 1.0f, 0.0f);
            var z = new Vector3(0.0f, 0.0f, 1.0f);

            var lightPos3 = new Vector3(lightPos.X, lightPos.Y, lightPos.Z);
            
            switch (face)
            {
                case CubeMapFace.NegativeZ:
                    return Matrix.LookAtLH(lightPos3, lightPos3 - z, y);
                case CubeMapFace.PositiveZ:
                    return Matrix.LookAtLH(lightPos3, lightPos3 + z, y);
                case CubeMapFace.NegativeY:
                    return Matrix.LookAtLH(lightPos3, lightPos3 - y, z);
                case CubeMapFace.PositiveY:
                    return Matrix.LookAtLH(lightPos3, lightPos3 + y, -z);
                case CubeMapFace.NegativeX:
                    return Matrix.LookAtLH(lightPos3, lightPos3 - x, y);
                case CubeMapFace.PositiveX:
                    return Matrix.LookAtLH(lightPos3, lightPos3 + x, y);
                default:
                    throw new ArgumentOutOfRangeException("face");
            }

        }

        /// <summary>
        /// Prepare a visible area of the map for rendering.
        /// </summary>
        /// <param name="camera"></param>
        public void Prepare(BasicCamera camera)
        {
            if (ObservableArea.ChunkMeshes.Count == 0)
                return;

            if (!_shouldGenerateGeometry)
                return;
            _shouldGenerateGeometry = false;

            GenerateGeometry();
        }

        private Dictionary<int, List<TriangleKey>> CreateMaterialBatches()
        {
            var materialBatch = new Dictionary<int, List<TriangleKey>>(DefaultMaterialListLength);
            for (int meshIdx = 0; meshIdx < ObservableArea.ChunkMeshes.Count; meshIdx++)
            {
                var chunkMesh = ObservableArea.ChunkMeshes[meshIdx];

                for (int triIdx = 0; triIdx < chunkMesh.Triangles.Count; triIdx++)
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

        private void GenerateGeometry()
        {
            var materialBatches = CreateMaterialBatches();

            DeleteMaterialVertexBatches();

            foreach (var materialBatch in materialBatches)
            {
                var materialVertexBatch = CreateMaterialVertexBatch(materialBatch);
                _materialVertexBatches.Add(materialVertexBatch);
            }
        }

        private MaterialVertexBatch CreateMaterialVertexBatch(KeyValuePair<int, List<TriangleKey>> materialBatch)
        {
            var material = materialBatch.Key;
            var triangleCount = materialBatch.Value.Count;

            var renderer = new VertexRenderer<ColouredTexturedVertexNormal4>(triangleCount * 3, _device);

            var dataStream = renderer.LockBuffer();

            var observableAreaOffset = ObservableArea.BottomLeft;

            foreach (var triangleKey in materialBatch.Value)
            {
                var chunkMesh = ObservableArea.ChunkMeshes[triangleKey.ChunkMeshIndex];
                var triangle = chunkMesh.Triangles[triangleKey.TriangleIndex];
                var offset = chunkMesh.ObservableAreaOffset - observableAreaOffset;

                var color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                var position0 = (triangle.P0 + offset).ToSlimDxFormat4();
                var position1 = (triangle.P1 + offset).ToSlimDxFormat4();
                var position2 = (triangle.P2 + offset).ToSlimDxFormat4();
                var normal = new Vector3(0, 0, -1.0f);
                
                var uv0 = (triangle.P0 / TextureScale).Scale(1, -1).ToSlimDxFormat2();
                var uv1 = (triangle.P1 / TextureScale).Scale(1, -1).ToSlimDxFormat2();
                var uv2 = (triangle.P2 / TextureScale).Scale(1, -1).ToSlimDxFormat2();

                dataStream.WriteRange(
                    new[]
                    {
                        new ColouredTexturedVertexNormal4(position0, color, normal, uv0),
                        new ColouredTexturedVertexNormal4(position1, color, normal, uv1),
                        new ColouredTexturedVertexNormal4(position2, color, normal, uv2)
                    });
            }

            renderer.UnlockBuffer();

            var textureName = _materialCache[material].TextureName;
            var texture = StaticTextureCache.TextureCache.GetTexture(textureName);

            return new MaterialVertexBatch(renderer, triangleCount, texture.TextureId);
        }
    }
}