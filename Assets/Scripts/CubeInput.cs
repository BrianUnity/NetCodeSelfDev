﻿using Unity.NetCode;
using Unity.Networking.Transport;

public struct CubeInput : ICommandData<CubeInput>
{
    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(horizontal);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public uint Tick => tick;
    public uint tick;
    public int horizontal;
}

public class NetCubeSendCommandSystem : CommandSendSystem<CubeInput>
{
}
public class NetCubeReceiveCommandSystem : CommandReceiveSystem<CubeInput>
{
}