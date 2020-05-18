using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

public struct PlayerGhostSerializer : IGhostSerializer<PlayerSnapshotData>
{
    private ComponentType componentTypeMovableCubeComponent;
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypePhysicsGravityFactor;
    private ComponentType componentTypePhysicsMass;
    private ComponentType componentTypePhysicsVelocity;
    private ComponentType componentTypePerInstanceCullingTag;
    private ComponentType componentTypeRenderBounds;
    private ComponentType componentTypeRenderMesh;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<MovableCubeComponent> ghostMovableCubeComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 8;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<PlayerSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeMovableCubeComponent = ComponentType.ReadWrite<MovableCubeComponent>();
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypePhysicsGravityFactor = ComponentType.ReadWrite<PhysicsGravityFactor>();
        componentTypePhysicsMass = ComponentType.ReadWrite<PhysicsMass>();
        componentTypePhysicsVelocity = ComponentType.ReadWrite<PhysicsVelocity>();
        componentTypePerInstanceCullingTag = ComponentType.ReadWrite<PerInstanceCullingTag>();
        componentTypeRenderBounds = ComponentType.ReadWrite<RenderBounds>();
        componentTypeRenderMesh = ComponentType.ReadWrite<RenderMesh>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostMovableCubeComponentType = system.GetArchetypeChunkComponentType<MovableCubeComponent>(true);
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref PlayerSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataMovableCubeComponent = chunk.GetNativeArray(ghostMovableCubeComponentType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetMovableCubeComponentPlayerId(chunkDataMovableCubeComponent[ent].PlayerId, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
