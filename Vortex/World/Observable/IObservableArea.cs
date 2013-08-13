using System;
using System.Collections.Generic;
using Psy.Core.Collision;
using SlimMath;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.World.Observable
{
    public delegate void ObservableAreaUpdated(ObservableArea item);
    public delegate Vector3 GetVectorCallback();

    public interface IObservableArea: IDisposable
    {
        /************************************
         * Double Buffered Items            *
         ************************************/
        /**
         *  Threading note:  
         *  It is important to note that there will be 2 threads using this class
         *  Main thread - to read the primary buffer
         *  Worker thread - to update the secondary buffer
         *  
         *  Do not let the main thread access the secondary buffer or bad things will happen
         */
        /**
         * Performance note:
         *   Don't keep getting a buffer if you don't have to - try to get the attribute once and
         *   then edit the local pointer rather than try to continually get the attribute to edit it
         */
        /// <summary>
        /// The world
        /// </summary>
        IOutsideLightingColour OutsideLightingColour { get; }

        /// <summary>
        /// get the chunks observed by the Primary Read only buffer
        /// maintained by the observable area
        /// </summary>
        List<List<ChunkKey>> ChunksObserved { get; }

        /// <summary>
        /// get the chunks observed by the Secondary read-write buffer
        /// maintained by the observable area
        /// </summary>
        List<List<ChunkKey>> ChunksObservedBuffer { get; }

        /// <summary>
        /// get the chunks observed by the primary read only buffer, with a border
        /// maintained by the observable area
        /// </summary>
        HashSet<ChunkKey> ChunksObservedExtended { get; }

        /// <summary>
        /// get the chunks observed by the primary read only buffer, with a border
        /// maintained by the observable area
        /// </summary>
        HashSet<ChunkKey> ChunksObservedExtendedBuffer { get; }
            
        /// <summary>
        /// Primary Tile buffer
        /// not maintained by the observable area
        /// </summary>
        List<ChunkMesh> ChunkMeshes { get; }

        /// <summary>
        /// Tiles for use by lighting etc - will be swapped to be 'Tiles' at a later date
        /// not maintained by the observable area
        /// </summary>
        List<ChunkMesh> ChunkMeshesBuffer { get; }

        /// <summary>
        /// Primary Collision mesh tester
        /// not maintained by the observable area - OBS COORDS
        /// </summary>
        IMeshCollisionTester CollisionTester { get; }

        /// <summary>
        /// Secondary Collision mesh tester
        /// not maintained by the observable area - OBS COORDS
        /// </summary>
        IMeshCollisionTester CollisionTesterBuffer { get; set; }

        /// <summary>
        /// Primary static lights in the observable area
        /// not maintained by the observable area - OBS COORDS
        /// </summary>
        List<ILight> Lights { get; }

        /// <summary>
        /// Secondary static lights in the observable area
        /// not maintained by the observable area - OBS COORDS
        /// </summary>
        List<ILight> LightsBuffer { get; }

        /// <summary>
        /// Primary world vector of the bottom left of the observable area
        /// not maintained by the observable area
        /// </summary>
        Vector2 BottomLeft { get; }

        /// <summary>
        /// Secondary world vector of the bottom left of the observable area
        /// not maintained by the observable area
        /// </summary>
        Vector2 BottomLeftBuffer { get; set; }

        /// <summary>
        /// Update the double buffer so that the renderer can use the latest Tile information
        /// </summary>
        void SwapBuffers();

        /****************************************
         * Non - double buffer stuffs           *
         ****************************************/
        /// <summary>
        /// Tests to see if the tile at the given position is above ground level
        /// returns false if it's not or if it's not in the observable area
        /// (ie it is not blocked if it's not in the observable area)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool IsBlocked(Vector3 position);

        /// <summary>
        /// Size of the observable area
        /// </summary>
        float ObservedSize     { get; }

        /// <summary>
        /// Half of the above (might save some divisions)
        /// </summary>
        float HalfObservedSize { get; }

        Vector2 Middle { get; }

        void ForceUpdate();
        void Update();
        event ObservableAreaUpdated Updated;
    }
}
