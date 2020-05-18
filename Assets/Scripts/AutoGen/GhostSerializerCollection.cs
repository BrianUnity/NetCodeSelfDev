using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct SelfDev_NetcodeGhostSerializerCollection : IGhostSerializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PlayerGhostSerializer",
            "PuckGhostSerializer",
            "ExtraBlockGhostSerializer",
        };
        return arr;
    }

    public int Length => 3;
#endif
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        if (typeof(T) == typeof(PlayerSnapshotData))
            return 0;
        if (typeof(T) == typeof(PuckSnapshotData))
            return 1;
        if (typeof(T) == typeof(ExtraBlockSnapshotData))
            return 2;
        return -1;
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
        m_PlayerGhostSerializer.BeginSerialize(system);
        m_PuckGhostSerializer.BeginSerialize(system);
        m_ExtraBlockGhostSerializer.BeginSerialize(system);
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
            case 0:
                return m_PlayerGhostSerializer.CalculateImportance(chunk);
            case 1:
                return m_PuckGhostSerializer.CalculateImportance(chunk);
            case 2:
                return m_ExtraBlockGhostSerializer.CalculateImportance(chunk);
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_PlayerGhostSerializer.SnapshotSize;
            case 1:
                return m_PuckGhostSerializer.SnapshotSize;
            case 2:
                return m_ExtraBlockGhostSerializer.SnapshotSize;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(ref DataStreamWriter dataStream, SerializeData data)
    {
        switch (data.ghostType)
        {
            case 0:
            {
                return GhostSendSystem<SelfDev_NetcodeGhostSerializerCollection>.InvokeSerialize<PlayerGhostSerializer, PlayerSnapshotData>(m_PlayerGhostSerializer, ref dataStream, data);
            }
            case 1:
            {
                return GhostSendSystem<SelfDev_NetcodeGhostSerializerCollection>.InvokeSerialize<PuckGhostSerializer, PuckSnapshotData>(m_PuckGhostSerializer, ref dataStream, data);
            }
            case 2:
            {
                return GhostSendSystem<SelfDev_NetcodeGhostSerializerCollection>.InvokeSerialize<ExtraBlockGhostSerializer, ExtraBlockSnapshotData>(m_ExtraBlockGhostSerializer, ref dataStream, data);
            }
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    private PlayerGhostSerializer m_PlayerGhostSerializer;
    private PuckGhostSerializer m_PuckGhostSerializer;
    private ExtraBlockGhostSerializer m_ExtraBlockGhostSerializer;
}

public struct EnableSelfDev_NetcodeGhostSendSystemComponent : IComponentData
{}
public class SelfDev_NetcodeGhostSendSystem : GhostSendSystem<SelfDev_NetcodeGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableSelfDev_NetcodeGhostSendSystemComponent>();
    }
}
