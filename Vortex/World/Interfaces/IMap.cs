using System;
using System.Collections.Generic;
using Psy.Core.Collision;
using Vortex.Interface;
using Vortex.Interface.Debugging;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.World.Observable;

namespace Vortex.World.Interfaces
{
    public interface IMap : IMapGeometry, IDisposable
    {
        /// <summary>
        /// the size of the largest observable area
        /// </summary>
        float MaximumObservableAreaSize { get; }

        /// <summary>
        /// Register a camera with the map.
        /// The map will work on keeping an observable area available to the camera
        /// </summary>
        /// <param name="camera"></param>
        IObservableArea AddCamera(ICamera camera);

        /// <summary>
        /// Removes the requirement that the map will need to keep the area around 
        /// the given camera up to date
        /// </summary>
        /// <param name="camera"></param>
        void RemoveCamera(ICamera camera);

        /// <summary>
        /// Texture set used for the map
        /// </summary>
        string TileSetName { get; }

        /// <summary>
        /// Update the map if required
        /// </summary>
        TimingStats Update();

        /// <summary>
        /// set some additional non-chunk based mesh testers that should be considered solid &
        /// part of the observable area in the same way that a tile would be.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        void SetStaticItemsInChunk(ChunkKey key, IEnumerable<IMeshCollisionTester> items);

        /// <summary>
        /// gets all the chunk keys that are being observed
        /// </summary>
        /// <returns></returns>
        IEnumerable<ChunkKey> GetObservedChunkKeys();
    }
}
