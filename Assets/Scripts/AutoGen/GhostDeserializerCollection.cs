using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct SelfDev_NetcodeGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PlayerGhostSerializer",
            "PuckGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curPlayerGhostSpawnSystem = world.GetOrCreateSystem<PlayerGhostSpawnSystem>();
        m_PlayerSnapshotDataNewGhostIds = curPlayerGhostSpawnSystem.NewGhostIds;
        m_PlayerSnapshotDataNewGhosts = curPlayerGhostSpawnSystem.NewGhosts;
        curPlayerGhostSpawnSystem.GhostType = 0;
        var curPuckGhostSpawnSystem = world.GetOrCreateSystem<PuckGhostSpawnSystem>();
        m_PuckSnapshotDataNewGhostIds = curPuckGhostSpawnSystem.NewGhostIds;
        m_PuckSnapshotDataNewGhosts = curPuckGhostSpawnSystem.NewGhosts;
        curPuckGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_PlayerSnapshotDataFromEntity = system.GetBufferFromEntity<PlayerSnapshotData>();
        m_PuckSnapshotDataFromEntity = system.GetBufferFromEntity<PuckSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<SelfDev_NetcodeGhostDeserializerCollection>.InvokeDeserialize(m_PlayerSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 1:
                return GhostReceiveSystem<SelfDev_NetcodeGhostDeserializerCollection>.InvokeDeserialize(m_PuckSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_PlayerSnapshotDataNewGhostIds.Add(ghostId);
                m_PlayerSnapshotDataNewGhosts.Add(GhostReceiveSystem<SelfDev_NetcodeGhostDeserializerCollection>.InvokeSpawn<PlayerSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 1:
                m_PuckSnapshotDataNewGhostIds.Add(ghostId);
                m_PuckSnapshotDataNewGhosts.Add(GhostReceiveSystem<SelfDev_NetcodeGhostDeserializerCollection>.InvokeSpawn<PuckSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<PlayerSnapshotData> m_PlayerSnapshotDataFromEntity;
    private NativeList<int> m_PlayerSnapshotDataNewGhostIds;
    private NativeList<PlayerSnapshotData> m_PlayerSnapshotDataNewGhosts;
    private BufferFromEntity<PuckSnapshotData> m_PuckSnapshotDataFromEntity;
    private NativeList<int> m_PuckSnapshotDataNewGhostIds;
    private NativeList<PuckSnapshotData> m_PuckSnapshotDataNewGhosts;
}
public struct EnableSelfDev_NetcodeGhostReceiveSystemComponent : IComponentData
{}
public class SelfDev_NetcodeGhostReceiveSystem : GhostReceiveSystem<SelfDev_NetcodeGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableSelfDev_NetcodeGhostReceiveSystemComponent>();
    }
}
