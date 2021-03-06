﻿using System.Collections.Generic;
using Psy.Core;
using SlimMath;
using Vortex.Interface.EntityBase;
using Vortex.Interface.Traits;
using Vortex.Interface.World;
using Vortex.Interface.World.Chunks;

namespace Vortex.Interface.Net
{
    public interface IIncomingMessageStream
    {
        byte ReadByte();
        byte[] ReadBytes();

        short ReadInt16();
        ushort ReadUint16();

        int ReadInt32();
        uint ReadUInt32();

        long ReadInt64();
        string ReadString();
        float ReadFloat();
        double ReadDouble();
        Vector3 ReadVector();
        float ReadRotation();
        int ReadEntityId();
        bool ReadBoolean();

        Entity ReadEntity();
        List<Entity> ReadEntities();
            
        Color4 ReadColour();
        List<ILight> ReadLights();
        ILight ReadLight();
        List<int> ReadEntityIds();
        List<RemotePlayer> ReadRemotePlayers();
        RemotePlayer ReadRemotePlayer();
        List<string> ReadStringList();
        List<IChunk> ReadChunks();
        IChunk ReadChunk();
        ChunkKey ReadChunkKey();
        List<ChunkKey> ReadChunkKeys();
        T ReadTrait<T>() where T : Trait, new();
        List<T> ReadEntityProperties<T>() where T : Trait, new();

        T2 Read<T1, T2>(IIncomingMessageStream messageStream) 
            where T1 : Trait, new()
            where T2: TraitCollection<T1>, new();
    }
}