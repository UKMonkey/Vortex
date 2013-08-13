using System;
using Psy.Core;
using Psy.Graphics;
using Psy.Graphics.VertexDeclarations;

namespace Vortex.Renderer.WorldRenderers.ShadowedRenderer
{
    class MaterialVertexBatch : IDisposable
    {
        public readonly Material Material;
        public readonly IVertexRenderer<ColouredTexturedVertexNormal4> Renderer;
        public readonly int TriangleCount;
        public readonly TextureAreaHolder TextureArea;

        public void Dispose()
        {
            if (Renderer != null)
                Renderer.Dispose();
        }

        public MaterialVertexBatch(IVertexRenderer<ColouredTexturedVertexNormal4> renderer, 
            int triangleCount, TextureAreaHolder textureArea, Material material)
        {
            Material = material;
            TriangleCount = triangleCount;
            TextureArea = textureArea;
            Renderer = renderer;
        }
    }
}