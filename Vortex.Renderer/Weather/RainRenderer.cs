using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core;
using Psy.Core.Console;
using Psy.Graphics;
using Psy.Graphics.Effects;
using Psy.Graphics.VertexDeclarations;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.World.Observable;
using PrimitiveType = Psy.Graphics.PrimitiveType;
using Vector3 = SlimMath.Vector3;

namespace Vortex.Renderer.Weather
{
    public class RainRenderer : IDisposable
    {
        private readonly IObservableArea _observableArea;
        private readonly MaterialCache _materialCache;
        private const int DropletCount = 512;
        private const int SplashCount = DropletCount*3;
        private readonly IVertexRenderer<ColouredVertex4> _dropletRenderer;
        private readonly RainDroplet[] _droplets;

        private readonly IVertexRenderer<ColouredVertex4> _splashRenderer;
        private readonly Splash[] _splashes;
        private List<ChunkMeshTriangle> _outsideTriangles;
        private readonly GraphicsContext _graphicsContext;

        public RainRenderer(GraphicsContext graphicsContext, IObservableArea observableArea, MaterialCache materialCache)
        {
            _graphicsContext = graphicsContext;
            _observableArea = observableArea;
            _materialCache = materialCache;
            _observableArea.Updated += ObservableAreaUpdated;

            StaticConsole.Console.RegisterFloat("rain_life", () => Splash.LifeDuration, delegate(float f) { Splash.LifeDuration = (int)f; });
            StaticConsole.Console.RegisterFloat("rain_mod", () => Splash.MoveModifier, delegate(float f) { Splash.MoveModifier = (int)f; });

            _dropletRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(2*DropletCount);

            _droplets = new RainDroplet[DropletCount];
            for (var index = 0; index < _droplets.Length; index++)
            {
                _droplets[index] = new RainDroplet();
            }

            _splashRenderer = graphicsContext.CreateVertexRenderer<ColouredVertex4>(2*SplashCount);

            _splashes = new Splash[SplashCount];
            for (var index = 0; index < _splashes.Length; index++)
            {
                _splashes[index] = new Splash();
            }

            _outsideTriangles = new List<ChunkMeshTriangle>();
        }

        public void Dispose()
        {
            if (_dropletRenderer != null) _dropletRenderer.Dispose();
            if (_splashRenderer != null) _splashRenderer.Dispose();
        }

        public void Update(Vector3 viewPosition)
        {
            for (var index = 0; index < _droplets.Length; index++)
            {
                var rainDroplet = _droplets[index];
                if (rainDroplet.IsDead())
                {
                    ResetRainDroplet(index, viewPosition);
                }
                rainDroplet.Update();
            }

            foreach (var splash in _splashes)
            {
                splash.Update();
            }
        }

        private int FindNextSplash()
        {
            for (var i = 0; i < _splashes.Length; i++)
            {
                var splash = _splashes[i];
                if (splash.IsDead())
                {
                    return i;
                }
            }
            return -1;
        }

        void ObservableAreaUpdated(ObservableArea item)
        {
            PopulateOutsideChunkMeshList();
        }

        private void PopulateOutsideChunkMeshList()
        {
            _outsideTriangles = new List<ChunkMeshTriangle>(10);

            const float maxRange = Chunk.ChunkWorldSize * 1.5f;

            foreach (var chunkMesh in _observableArea.ChunkMeshes)
            {
                if ((chunkMesh.WorldVector.Translate(Chunk.ChunkWorldSize / 2.0f, Chunk.ChunkWorldSize / 2.0f, 0))
                    .DistanceSquared(_observableArea.Middle.AsVector3()) > (maxRange * maxRange))
                    continue;

                foreach (var chunkMeshTriangle in chunkMesh.Triangles)
                {
                    var materialId = chunkMeshTriangle.Material;
                    var material = _materialCache[materialId];
                    if (material.Outside)
                    {
                        _outsideTriangles.Add(chunkMeshTriangle);
                    }
                }
            }
        }

        private void ResetRainDroplet(int index, Vector3 viewPosition)
        {
            var dropletPosition = _droplets[index].Position;

            var randomTriangle = GetRandomOutsideTriangle();

            if (randomTriangle == null)
                return;

            var randomPoint = randomTriangle.GetRandomPointInWorld();

            _droplets[index].Position = new Vector3(
                randomPoint.X, 
                randomPoint.Y, 
                (float)(-10 * 1 - (StaticRng.Random.NextDouble() * 10)));

            _droplets[index].Velocity = 0.8f;

            // 3 splashes!
            SpawnSplash(dropletPosition);
            SpawnSplash(dropletPosition);
            SpawnSplash(dropletPosition);
            SpawnSplash(dropletPosition);
            SpawnSplash(dropletPosition);
        }

        private ChunkMeshTriangle GetRandomOutsideTriangle()
        {
            if (_outsideTriangles.Count == 0)
                return null;

            int i = StaticRng.Random.Next(_outsideTriangles.Count - 1);
            return _outsideTriangles[i];
        }

        private void SpawnSplash(Vector3 dropletPosition)
        {
            var isplash = FindNextSplash();
            if (isplash == -1)
            {
                // shit, no more splashes!
                return;
            }
            _splashes[isplash].Reset(dropletPosition);
        }

        public void Render(SlimMath.Matrix cameraTransform, SlimMath.Matrix perspectiveMatrix)
        {
            var effect = _graphicsContext.CreateEffect("basic.fx");

            var matrix = cameraTransform*perspectiveMatrix;
            effect.SetMatrix("worldViewProjMat", matrix);

            RenderDroplets(effect);
            RenderSplashes(effect);
        }

        private void RenderDroplets(IEffect effect)
        {
            var drawcount = 0;
            var stream = _dropletRenderer.LockVertexBuffer();

            foreach (var rainDroplet in _droplets.Where(d => !d.IsDead()))
            {
                stream.WriteRange(
                    new[]
                    {
                        new ColouredVertex4
                        {
                            Colour = new SlimMath.Color4(0.5f, 0.2f, 0.2f, 0.25f),
                            Position = rainDroplet.Position
                        },
                        new ColouredVertex4
                        {
                            Colour = new SlimMath.Color4(0.5f, 0.1f, 0.1f, 0.15f),
                            Position = rainDroplet.Position.Translate(0, 0, -2f)
                        }
                    });

                drawcount++;
            }
            _dropletRenderer.UnlockVertexBuffer();

            effect.Begin();
            effect.BeginPass(0);
            _dropletRenderer.Render(PrimitiveType.LineList, 0, drawcount);
            effect.EndPass();
            effect.End();
        }

        private void RenderSplashes(IEffect effect)
        {
            var drawcount = 0;
            var stream = _splashRenderer.LockVertexBuffer();

            foreach (var splash in _splashes.Where(s => !s.IsDead()))
            {
                stream.WriteRange(
                    new[]
                    {
                        new ColouredVertex4
                        {
                            Colour = new SlimMath.Color4(0.0f, 0.2f, 0.2f, 0.25f),
                            Position = splash.Position
                        },
                        new ColouredVertex4
                        {
                            Colour = new SlimMath.Color4(0.0f, 0.0f, 0.0f, 0.05f),
                            Position = (splash.Position + splash.Movement)
                        }
                    });

                drawcount++;
            }
            _splashRenderer.UnlockVertexBuffer();

            effect.Begin();
            effect.BeginPass(0);
            _splashRenderer.Render(PrimitiveType.LineList, 0, drawcount);
            effect.EndPass();
            effect.End();
        }
    }
}