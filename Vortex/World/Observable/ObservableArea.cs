using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Psy.Core.Collision;
using Psy.Core.Logging;
using SlimMath;
using Vortex.Interface.Debugging;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;
using Vortex.World.Chunks;
using Vortex.World.Observable.Workers;

/**
 * Threading on the observable area...
 * 1.  Main thread swaps buffers if possible (set PerformBufferSwap to false)
 * 2.  Main thread adds n attempts to start the worker thread
 *    each of these requests increments a counter so that the worker knows if it missed any while it was working
 *    and has to start again
 * 
 * 1.  Worker thread waits to be told to start work
 * 2.  Worker thread waits for the PerformBufferSwap to be false
 * 3.  Worker thread starts performing update
 * 4.  Worker thread calls 'ObservableAreaListeners' to perform additional work to the secondary buffer
 * 5.  Worker thread sets PerformBufferSwap to be true
 * 6.  Worker thread checks counter to see if it missed any requests - and returns to #2 if it did
 */

namespace Vortex.World.Observable
{
    public class ObservableArea : IObservableArea
    {
        // central location of the observable area
        private readonly GetVectorCallback _getCenterPosition;
        private Vector3 CenterPosition
        {
            get { return _getCenterPosition(); }
        }

        // mechanism to get chunks
        private readonly IChunkCache _chunkCache;

        // size of chunk square to maintain  (must be odd)
        private const short ChunkSquareSize = 5;

        /** Double buffered items **/
        private volatile List<List<ChunkKey>> _keysGridA;
        private volatile List<List<ChunkKey>> _keysGridB;

        private volatile List<ChunkMesh> _meshesA;
        private volatile List<ChunkMesh> _meshesB;

        public IOutsideLightingColour OutsideLightingColour { get; private set; }

        private volatile List<ILight> _lightsA;
        private volatile List<ILight> _lightsB;

        private volatile IMeshCollisionTester _meshTesterA;
        private volatile IMeshCollisionTester _meshTesterB;

        private Vector2 _bottomLeftA;
        private Vector2 _bottomLeftB;

        private volatile HashSet<ChunkKey> _observedChunksExtendedA;
        private volatile HashSet<ChunkKey> _observedChunksExtendedB;

        private const float _observedSize = ChunkSquareSize*Chunk.ChunkWorldSize;
        public float ObservedSize { get { return _observedSize; } }

        private const float _halfObservedSize = _observedSize / 2;
        public float HalfObservedSize { get { return _halfObservedSize; } }

        /// <summary>
        /// Middle in Observable Area coordinates
        /// </summary>
        public Vector2 Middle
        {
            get { return BottomLeft + new Vector2(_halfObservedSize, _halfObservedSize); }
        }

        /** Double buffered attributes **/
        public List<List<ChunkKey>> ChunksObserved        { get { return _bufferState ? _keysGridA : _keysGridB; } }
        public List<List<ChunkKey>> ChunksObservedBuffer  { get { return _bufferState ? _keysGridB : _keysGridA; } }

        // what should these be now?
        public List<ChunkMesh> ChunkMeshes             { get { return _bufferState ? _meshesA : _meshesB; } }
        public List<ChunkMesh> ChunkMeshesBuffer       { get { return _bufferState ? _meshesB : _meshesA; } }

        public HashSet<ChunkKey> ChunksObservedExtended       { get { return _bufferState ? _observedChunksExtendedB : _observedChunksExtendedA; } }
        public HashSet<ChunkKey> ChunksObservedExtendedBuffer { get { return _bufferState ? _observedChunksExtendedA : _observedChunksExtendedB; } }

        public List<ILight> Lights                        { get { return _bufferState ? _lightsA : _lightsB; } }
        public List<ILight> LightsBuffer                  { get { return _bufferState ? _lightsB : _lightsA; } }

        public IMeshCollisionTester CollisionTester { get { return _bufferState ? _meshTesterA : _meshTesterB; } }
        public IMeshCollisionTester CollisionTesterBuffer
        {
            get { return _bufferState ? _meshTesterB : _meshTesterA; }
            set
            {
                if (_bufferState) _meshTesterB = value;
                else _meshTesterA = value;
            }
        }

        public Vector2 BottomLeft                         { get { return _bufferState ? _bottomLeftA : _bottomLeftB; } }
        public Vector2 BottomLeftBuffer                   { get { return _bufferState ? _bottomLeftB : _bottomLeftA; }
            set
            {
                if (_bufferState) _bottomLeftB = value;
                else _bottomLeftA = value;
            }
        }

        public event ObservableAreaUpdated Updated;

        private void OnUpdated()
        {
            var handler = Updated;
            if (handler != null) handler(this);
        }

        // Methods that will be responsible for updating the states of the 2ndary buffer
        private readonly List<IObservableAreaWorker> _updaters;

        // state of the dual buffers ...
        // true   = A = active set;  B = secondary set;
        // false  = B = active set;  A = secondary set;
        private volatile bool _bufferState;

        // if true, the buffer state is changable...
        private volatile bool _canPerformBufferSwap;

        // worker thread - see above
        private readonly Thread _workerThread;
        private volatile bool _runWorker;

        // worker request count - see above
        private int _workCount;

        public ObservableArea(IOutsideLightingColour outsideLightingColour, GetVectorCallback getCenterPosition, 
            IChunkCache cache, List<IObservableAreaWorker> updaters)
        {
            Debug.Assert(outsideLightingColour != null);
            Debug.Assert(cache != null);
            Debug.Assert(updaters != null);

            OutsideLightingColour = outsideLightingColour;
            _getCenterPosition = getCenterPosition;
            _chunkCache = cache;
            _chunkCache.OnChunksLoaded += ChunksLoaded;
            _chunkCache.OnChunksUpdated += ChunksUpdated;

            _updaters = updaters;

            _lightsA = new List<ILight>();
            _lightsB = new List<ILight>();

            _meshesA = new List<ChunkMesh>();
            _meshesB = new List<ChunkMesh>();

            _keysGridA = new List<List<ChunkKey>>();
            _keysGridB = new List<List<ChunkKey>>();

            _observedChunksExtendedA = new HashSet<ChunkKey>();
            _observedChunksExtendedB = new HashSet<ChunkKey>();

            _meshTesterA = new MeshCollisionTester(new Mesh());
            _meshTesterB = new MeshCollisionTester(new Mesh());

            _workerThread = new Thread(WorkerThreadMain) {Name = "ObservableArea Worker"};
            _workerThread.Priority = ThreadPriority.Lowest;
            _workCount = 0;

            for (var i = 0; i < ChunkSquareSize; ++i )
            {
                _keysGridA.Add(new List<ChunkKey>());
                _keysGridB.Add(new List<ChunkKey>());
            }

            _bufferState = false;
            _runWorker = true;

            PerformUpdate();
            SwapBuffers();

            _workerThread.Start();
        }

        //stop the worker thread - we want to lose any we don't need
        public void Dispose()
        {
            _workerThread.Abort();
        }

        // worker thread - see above...
        private void WorkerThreadMain()
        {
            Logger.Write(
                string.Format("Thread '{0}' started.", Thread.CurrentThread.Name));
            try
            {
                while (_runWorker)
                {
                    lock (this)
                    {
                        if (_workCount == 0)
                            Monitor.Wait(this);
                    }

                    lock (_workerThread)
                    {
                        if (_canPerformBufferSwap)
                            Monitor.Wait(_workerThread);
                    }

                    if (_workCount > 0)
                    {
                        _workCount = 0;
                        PerformUpdate();
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Write(
                    string.Format("Thread '{0}' aborted.", Thread.CurrentThread.Name));
            }
            Logger.Write(
                string.Format("Thread '{0}' exited.", Thread.CurrentThread.Name));
        }

        // only need to update if the new centre tile has changed
        private bool ShouldUpdate()
        {
            var key = GetCentreChunkKey();
            const int position = ChunkSquareSize / 2;
            return _keysGridA == null 
                || _keysGridA.Count < 2 
                || _keysGridA[0].Count < 2
                || key != _keysGridA[position][position];
        }

        /// <summary>
        /// Returns a ChunkKey of the Chunk in the middle of the grid of Chunks.
        /// </summary>
        /// <returns></returns>
        private ChunkKey GetCentreChunkKey()
        {
            var position = CenterPosition;
            var key = Utils.GetChunkKeyForPosition(position);
            return key;
        }

        // update all the keys that we want to be watching
        private void UpdateObserveredKeys(Vector3 position)
        {
            var key = Utils.GetChunkKeyForPosition(position);
            const int middle = (ChunkSquareSize/2);

            var monitoredChunks = ChunksObservedBuffer;
            foreach (var items in monitoredChunks)
                items.Clear();

            var bottomLeft = key;

            // (sorry GC - this is a little hungry, but should only be a few extra objects!)
            for (var i = 0; i < middle; ++i)
            {
                bottomLeft = bottomLeft.Left();
                bottomLeft = bottomLeft.Bottom();
            }

            for (var i = 0; i < ChunkSquareSize; ++i)
            {
                var toAdd = bottomLeft;
                for (var j = 0; j < ChunkSquareSize; ++j)
                {
                    monitoredChunks[i].Add(toAdd);
                    toAdd = toAdd.Right();
                }
                bottomLeft = bottomLeft.Top();
            }

            lock (this)
            {
                var requiredKeys = ChunksObservedExtendedBuffer;
                requiredKeys.Clear();

                // make sure that for each monitored chunk we've got the 8 keys surrounding it monitored
                foreach (var item in monitoredChunks.SelectMany(item => item))
                {
                    requiredKeys.Add(item.Left());
                    requiredKeys.Add(item.Left().Top());
                    requiredKeys.Add(item.Left().Bottom());

                    requiredKeys.Add(item.Top());
                    requiredKeys.Add(item.Bottom());

                    requiredKeys.Add(item.Right());
                    requiredKeys.Add(item.Right().Top());
                    requiredKeys.Add(item.Right().Bottom());
                }
            }
        }

        // re-establish what chunks should be observed, and update all the tiles
        private void PerformUpdate()
        {
            var stats = new TimingStats("Observable Area Update");

            stats.StartingTask("UpdateObservedKeys");
            UpdateObserveredKeys(CenterPosition);
            stats.CompletedTask("UpdateObservedKeys");

            foreach (var updater in _updaters)
            {
                var name = string.Format("Worker: {0}", updater.GetType().Name);

                stats.StartingTask(name);
                stats.MergeStats(updater.WorkOnArea(this));
                stats.CompletedTask(name);
            }

            stats.LogStats(100, 200, 500);
            _canPerformBufferSwap = true;
        }


        /**************
         * Main thread functions
         **************/

        // Called when a chunk has been updated
        private void ChunksUpdated(List<Chunk> chunks)
        {
            bool doUpdate;
            lock (this)
            {
                doUpdate = chunks.Any(chunk => ChunksObservedExtended.Contains(chunk.Key) || 
                                        ChunksObservedExtendedBuffer.Contains(chunk.Key));
            }

            if (doUpdate)
            {
                ForceUpdate();
            }
        }

        // Called when a chunk has been loaded from fresh
        private void ChunksLoaded(List<Chunk> chunks)
        {
            ChunksUpdated(chunks);
        }

        // Enque an update
        public void ForceUpdate()
        {
            lock (this)
            {
                _workCount++;
                Monitor.PulseAll(this);
                Logger.Write("Forcing update of observable area");
            }
        }

        // Enque an update if required
        public void Update()
        {
            if (ShouldUpdate())
            {
                ForceUpdate();
            }
        }

        // The vector is the world vector
        public bool IsBlocked(Vector3 position)
        {
            // TODO - look at the gradient, and ensure it's not too steep
            return false;
        }

        // to be called on the main thread to swap primary & secondary buffers if possible.
        // note that it is important for the worker thread to NOT be active if the _canPerformBufferSwap is true
        // since the buffer state may change at any moment
        public void SwapBuffers()
        {
            if (!_canPerformBufferSwap)
                return;

            _bufferState = !_bufferState;
            OnUpdated();

            // IMPORTANT! - do this last
            _canPerformBufferSwap = false;

            lock(_workerThread)
            {
                Monitor.PulseAll(_workerThread);
            }

            OnUpdated();
        }
    }
}
