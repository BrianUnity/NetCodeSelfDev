using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Joint = Unity.Physics.Joint;

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach(
            (Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
                UnityEngine.Debug.Log(String.Format("Server setting connection {0} to in game",
                    EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));
                
                var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
                var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<PlayerSnapshotData>();
                var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
                var playerEntity = EntityManager.Instantiate(prefab);

                EntityManager.AddComponent<MainPlayerData>(playerEntity);

                int playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;
                EntityManager.SetComponentData(playerEntity, new MovableCubeComponent {PlayerId = playerId});
                PostUpdateCommands.AddBuffer<CubeInput>(playerEntity);

                bool up = (playerId % 2) == 0;
                EntityManager.SetComponentData(playerEntity, new Translation { Value = up ?
                    new float3(0f, 0f, POS) : new float3(0f, 0f, -POS)});

                AddExtraBlock(playerEntity, 1f);
                AddExtraBlock(playerEntity, 2f);
                AddExtraBlock(playerEntity, -1f);
                AddExtraBlock(playerEntity, -2f);

                Debug.Log($"Creating input buffer {playerEntity}");
                
                PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent {targetEntity = playerEntity});

                PostUpdateCommands.DestroyEntity(reqEnt);
            });
    }

    void AddExtraBlock(Entity centerEntity, float offset)
    {
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<ExtraBlockSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var blockEntity = EntityManager.Instantiate(prefab);
        
        EntityManager.AddComponentData(blockEntity, new FixToTargetData() {targetEntity = centerEntity, offset = offset});
        EntityManager.AddComponent<ExtraCubeData>(blockEntity);

        /*var jointComponentData = new PhysicsJoint
        {
            JointData = JointData.CreateFixed(
                new RigidTransform(Quaternion.identity, float3.zero),
                new RigidTransform(Quaternion.identity, offset)
            ),
            EntityA = blockEntity,
            EntityB = centerEntity,
            EnableCollision = 0,
        };
        EntityManager.AddComponentData(blockEntity, jointComponentData);*/
    }

    private const float POS = 6.5f;
}