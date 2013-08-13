using SlimMath;

namespace Vortex.Renderer.Weather
{
    class RainDroplet
    {
        public Vector3 Position { get; set; }
        public float Velocity { get; set; }

        public RainDroplet()
        {
            Velocity = -1; // Dead.
        }

        public bool IsDead()
        {
            return (Velocity <= 0 || Position.Z >= 0);
        }

        public void Update()
        {
            if (IsDead())
                return;
            Position = Position + new Vector3(0, 0, Velocity);
        }
    }
}