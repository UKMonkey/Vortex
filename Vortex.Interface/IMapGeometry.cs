using System.Collections.Generic;
using Psy.Core.Collision;
using SlimMath;

namespace Vortex.Interface
{
    public interface IMapGeometry
    {
        /// <summary>
        /// returns true if there's no map part in the way between the from & to
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        bool IsLineOfSight(Vector3 from, Vector3 to);

        /// <summary>
        /// walk from 'from' in the given direction, and report where it hits the map.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        CollisionResult TestMapCollision(Vector3 from, Vector3 direction);

        /// <summary>
        /// walk from 'from' in the given direction, and report where it hits the map or any of the additional targets
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <param name="additionalTargets"></param>
        /// <returns></returns>
        CollisionResult TestMapCollision(Vector3 from, Vector3 direction, IEnumerable<Mesh> additionalTargets);
    }
}
