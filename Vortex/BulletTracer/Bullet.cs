using System;
using Psy.Core;
using SlimMath;
using Vortex.Interface;

namespace Vortex.BulletTracer
{
    public class Bullet
    {
        public Vector3 MovementVector { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Color4 Colour { get; set; }

        public DamageTypeEnum DamageType { get; private set; }
        public float DamageAmount { get; private set; }

        private const float BulletSpeed = 2.0f;
        private double _updateCount;

        public Bullet(Vector3 startPosition, Vector3 targetPosition)
        {
            DamageAmount = 20;
            DamageType = DamageTypeEnum.LowCaliberBullet;

            StartPosition = startPosition;

            CurrentPosition = startPosition;
            MovementVector = (targetPosition - startPosition);

            _updateCount = Math.Ceiling(MovementVector.Length / BulletSpeed);

            MovementVector = MovementVector.NormalizeRet();
            MovementVector *= BulletSpeed;
        }

        public bool Update()
        {
            if (_updateCount-- <= 0)
                return false;

            if (_updateCount <= 0)
            {
                CurrentPosition = TargetPosition;
                return false;
            }
            
            CurrentPosition += MovementVector;

            return true;
        }
    }
}
