using Psy.Core;
using SlimMath;
using Vortex.Interface;

namespace Vortex
{
    class ProximityTester
    {
        private readonly float _distance;
        private readonly Vector3 _position;

        public ProximityTester(Vector3 position, float distance)
        {
            _position = position;
            _distance = distance;
        }

        public bool Test(IEngine engine, float outdoorLightIntensity, Color4 bakedLight, float fieldOfFiew, Vector3 location)
        {
            return location.FastDistance(_position) < _distance;
        }
    }
}