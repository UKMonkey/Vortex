using Psy.Core;
using Psy.Graphics;
using SlimMath;

namespace Vortex.Renderer.Camera
{
    public class DemoCamera : BasicCamera
    {
        private readonly Vector3[] _track;
        private int _destinationIndex;
        private Vector3 _currentVector;

        /// <summary>
        /// The position of the currently tracked destination.
        /// </summary>
        private Vector3 Destination { get { return _track[_destinationIndex]; } }
        
        /// <summary>
        /// Minimum distance from track point to flag up as arriving at it.
        /// Default is 2.0f.
        /// </summary>
        private float Epsilon { get; set; }

        /// <summary>
        /// Higher is slower. Default is 40.0f.
        /// </summary>
        private float AccelerationInv { get; set; }

        /// <summary>
        /// Minimum speed. Should not be above Epsilon/2 or camera may overshoot.
        /// Default is 0.9f;
        /// </summary>
        private float MinimumSpeed { get; set; }

        /// <summary>
        /// The maximum distance the camera can travel in 1 frame. Default is 10.0f.
        /// </summary>
        private float MaximumSpeed { get; set; }

        /// <summary>
        /// Determine if the camera should loop after reaching the end of the
        /// track. Default is false.
        /// </summary>
        private bool Loop { get; set; }

        private bool _finished;

        internal DemoCamera(GraphicsContext graphicsContext, Vector3[] track)
            : base(graphicsContext)
        {
            Epsilon = 5.0f;
            AccelerationInv = 40.0f;
            MinimumSpeed = 2.2f;
            MaximumSpeed = 10.0f;
            Loop = false;

            _track = track;
            _destinationIndex = 1;
            _currentVector = _track[0];
            _finished = false;
        }

        private void SetNextTrackIndex()
        {
            // check if near destination
            if (_currentVector.Distance(Destination) < Epsilon)
            {
                _destinationIndex++;
                System.Console.WriteLine("Destination is now " + _destinationIndex);
            }

            // check if going over end of track.
            if (_destinationIndex + 1 <= _track.Length)
                return;

            _destinationIndex = 0;
            System.Console.WriteLine("Destination is now " + _destinationIndex);
            if (!Loop)
                _finished = true;
        }

        public override void Update()
        {
            base.Update();

            if (_finished)
                return;

            // move towards next point in the track.
            var direction = Destination - _currentVector;
            var distance = direction.Length;

            var mult = distance/AccelerationInv;
            if (mult <= MinimumSpeed)
            {
                mult = MinimumSpeed;
            }
            if (mult >= MaximumSpeed)
            {
                mult = MaximumSpeed;
            }

            direction = direction.NormalizeRet();
            direction *= mult;

            _currentVector += direction;

            SetNextTrackIndex();
        }

        protected override Vector3 GetPosition()
        {
            return _currentVector;
        }
    }
}
