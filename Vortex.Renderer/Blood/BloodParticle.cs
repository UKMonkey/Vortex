using Psy.Core;
using SlimMath;

namespace Vortex.Renderer.Blood
{
    public class BloodParticle
    {
        public Vector3 Position;
        public Vector3 Direction;
        public int Life;
        public int MaxLife;
        public Vector3 StartPosition;
        public readonly int ParticleId;
        public bool Stationary;

        public BloodParticle(int particleId)
        {
            MaxLife = 800;
            Life = 0;
            Position = new Vector3();
            Direction = new Vector3();
            ParticleId = particleId;
            Stationary = false;
        }

        public bool IsDead()
        {
            return Life == 0;
        }

        public void Reset(Vector3 startPosition, float shotAngle)
        {
            var shotSlew = (float) (0.2f - StaticRng.Random.NextDouble()*0.4f);
            var speedAlter = (float)StaticRng.Random.NextDouble()*0.2f;

            StartPosition = startPosition;
            Position = startPosition;
            Life = MaxLife;
            Direction = VectorExtensions.From2DAngle(shotAngle + shotSlew) * speedAlter;
            Direction.Z = 0.04f +(float)(StaticRng.Random.NextDouble() * 0.09f);
            Stationary = false;
        }

        public void Update()
        {
            Life--;

            if (Stationary)
                return;

            StartPosition += Direction.Scale(0.1f, 0.1f, 1.02f);
            Position += Direction;
            Direction = Direction.Scale(0.9f, 0.9f, 1.0f);

            Direction.Z += 0.005f;

            if (StartPosition.Z >= 0 && Position.Z >= 0)
            {
                Stationary = true;
            }

            if (StartPosition.Z >= 0)
            {
                StartPosition.Z = -0.01f;
            }
            if (Position.Z >= 0)
            {
                Position.Z = -0.01f;
            }
        }

    }
}