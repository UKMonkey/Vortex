using Psy.Core;
using SlimMath;

namespace Vortex.Renderer.Weather
{
    class Splash
    {
        public Vector3 Position { get; set; }
        public Vector3 Movement { get; set; }
        public int Life { get; set; }

        public static int LifeDuration = 10;
        public static float MoveModifier = 2.0f;

        public Splash()
        {
            Life = 0;
        }

        public bool IsDead()
        {
            return Life == 0;
        }

        public void Update()
        {
            if (IsDead())
                return;

            Position = Movement + Position;
            Movement = Movement * 0.5f;
            Life -= 1;
        }

        public void Reset(Vector3 position)
        {
            Position = position.Scale(1.0f, 1.0f, 0.0f);
            Movement = new Vector3(
                (float)((0.5f - StaticRng.Random.NextDouble()) / MoveModifier),
                (float)((0.5f -StaticRng.Random.NextDouble()) / MoveModifier),
                -(float)(StaticRng.Random.NextDouble()) / MoveModifier);
            Life = LifeDuration;
        }
    }
}