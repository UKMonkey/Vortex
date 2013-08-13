using System;
using SlimMath;

namespace Vortex.Renderer
{
    class RenderedRay
    {
        public const int MaxLife = 2000;

        public Vector3 From { get; set; }
        public Vector3 To { get; set; }
        public int Life { get; set; }
        private Color4 MainColour { get; set; }

        public bool IsDead()
        {
            return Life == 0;
        }

        public void Update()
        {
            if (IsDead())
                return;
            Life--;
        }

        public Color4 Colour
        {
            get
            {
                var f = (float)Life / MaxLife;

                if (Math.Abs(To.X - 0) < 0.001f && Math.Abs(To.Y - 0) < 0.001f)
                    return new Color4(1.0f, 0.0f, f * MainColour.Green, f * MainColour.Blue);

                return new Color4(f, f * MainColour.Red, f * MainColour.Green, f * MainColour.Blue);
            }
        }

        public void Reset(Vector3 @from, Vector3 to, Color4 mainColour)
        {
            Life = MaxLife;
            From = from;
            To = to;
            MainColour = mainColour;
        }
    }
}