﻿using System;
using System.Collections.Generic;
using System.IO;
using Psy.Core.Serialization;
using SlimMath;


namespace Vortex.Interface.World.Chunks
{
    public class BlockChunk : IChunk
    {
        public event SingleChunkCallback ChunkChanged;

        public ChunkKey Key { get; set; }
        public List<ILight> Lights { get; private set; }

        private readonly Dictionary<int, ChunkMesh> _interestToMesh;
        private ushort[][,] _blocks;

        private short XSize { get; set; }
        private short YSize { get; set; }
        private short ZSize { get; set; }

        private int XMultiplier { get { return 1; } }
        private int YMultiplier { get { return XSize; } }
        private int ZMultiplier { get { return XSize * YSize; } }

        private readonly List<ushort> _updatedBlocks;

        public BlockChunk()
        {
            Lights = new List<ILight>();
            _updatedBlocks = new List<ushort>(5);
            _interestToMesh = new Dictionary<int, ChunkMesh>();
        }

        public BlockChunk(IEngine engine, short depth)
            : this()
        {
            XSize = engine.ChunkWorldSize;
            YSize = engine.ChunkWorldSize;
            ZSize = depth;

            _blocks = new ushort[ZSize][,];
            for (var i = 0; i < ZSize; ++i)
                _blocks[i] = new ushort[XSize, YSize];
        }

        public ushort[,] GetBlocksForLevel(ushort level)
        {
            return _blocks[level];
        }

        private int GetBlockIndex(ushort x, ushort y, ushort z)
        {
            return x * XMultiplier + y * YMultiplier + z * ZMultiplier;
        }

        private void DecodeBlockIndex(int index, out ushort x, out ushort y, out ushort z)
        {
            z = (ushort)Math.Floor((double)index / ZMultiplier);
            var xyRemainer = index % ZMultiplier;
            y = (ushort)Math.Floor((double)xyRemainer / YMultiplier);
            x = (ushort)(xyRemainer % YMultiplier);
        }

        public void SetBlockType(ushort x, ushort y, ushort z, ushort type)
        {
            if (_blocks[z][y, x] == type)
                return;

            var index = GetBlockIndex(x, y, z);
            _blocks[z][y,x] = type;
            _interestToMesh.Remove(z);
            
            if (ChunkChanged != null)
            {
                _updatedBlocks.Add((ushort)index);
                ChunkChanged(this);
            }
        }

        public byte[] GetFullData()
        {
            var stream = new MemoryStream();

            stream.Write(XSize);
            stream.Write(YSize);
            stream.Write(ZSize);
            stream.Write(_blocks);

            return stream.ToArray();
        }

        public void ApplyFullData(byte[] data)
        {
            var stream = new MemoryStream(data);

            XSize = stream.ReadShort();
            YSize = stream.ReadShort();
            ZSize = stream.ReadShort();

            _blocks = stream.ReadUShortMap();
            _interestToMesh.Clear();
            if (ChunkChanged != null)
                ChunkChanged(this);
        }

        public byte[] GetDirtyData()
        {
            var stream = new MemoryStream();

            stream.Write((ushort)_updatedBlocks.Count);
            foreach (var index in _updatedBlocks)
            {
                ushort x, y, z;
                DecodeBlockIndex(index, out x, out y, out z);
                stream.Write(index);
                stream.Write(_blocks[z][y,x]);
            }

            return stream.ToArray();
        }

        public void ApplyDirtyData(byte[] data)
        {
            var stream = new MemoryStream(data);

            var count = stream.ReadUShort();
            for (var i = 0; i < count; ++i)
            {
                var index = stream.ReadUShort();
                var value = stream.ReadUShort();

                ushort x, y, z;
                DecodeBlockIndex(index, out x, out y, out z);

                _blocks[z][y,x] = value;
                _updatedBlocks.Add(index);
                _interestToMesh.Remove(z);
            }
            if (ChunkChanged != null)
                ChunkChanged(this);
        }
        
        public ChunkMesh GetChunkMesh(int levelOfInterest)
        {
            return _interestToMesh[levelOfInterest];
        }

        public IEnumerable<KeyValuePair<int, ChunkMesh>> GetChunkMeshes()
        {
            return _interestToMesh;
        }

        private ChunkMesh GenerateMesh(ushort[,] area, IEngine engine)
        {
            var mesh = new ChunkMesh();

            for (var y = 0; y < YSize; ++y)
            {
                for (var x=0; x<XSize; ++x)
                {
                    var blockType = area[y, x];
                    var blockProperties = engine.BlockTypeCache.GetBlockProperties(blockType);

                    var material = blockProperties.GetMaterial();
                    var bottomLeft = new Vector3(x, y, 0);
                    var topRight = new Vector3(x+1, y+1, 0);

                    mesh.AddRectangle(material, bottomLeft, topRight);
                }
            }

            return mesh;
        }

        // something quick and dirty for now
        // will in the future want to just invalidate the cache
        // for the level(s) and use the level below to try and fill
        // in holes rather than leave them meshless
        public void RecalculateMesh(IEngine engine)
        {
            for (var z = 0; z < ZSize; ++z)
            {
                if (_interestToMesh.ContainsKey(z))
                    continue;

                var area = _blocks[z];
                _interestToMesh[z] = GenerateMesh(area, engine);
            }
        }
    }
}
