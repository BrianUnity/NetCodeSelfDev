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

                int playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;
                EntityManager.AddComponentData(playerEntity, new MovableCubeComponent {PlayerId = playerId});
                PostUpdateCommands.AddBuffer<CubeInput>(playerEntity);
                
                EntityManager.AddComponentData(playerEntity, new MainPlayerData {teamID = playerId});
                TeamData teamData = new TeamData() { teamID = playerId};

                bool up = (playerId % 2) == 0;
                EntityManager.SetComponentData(playerEntity, new Translation { Value = up ?
                    new float3(0f, 0f, POS) : new float3(0f, 0f, -POS)});

                AddExtraBlock(playerEntity, 1f, teamData, playerId);
                AddExtraBlock(playerEntity, 2f, teamData, playerId);
                AddExtraBlock(playerEntity, -1f, teamData, playerId);
                AddExtraBlock(playerEntity, -2f, teamData, playerId);

                Debug.Log($"Creating input buffer {playerEntity}");
                
                PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent {targetEntity = playerEntity});

                PostUpdateCommands.DestroyEntity(reqEnt);
            });
    }

    void AddExtraBlock(Entity centerEntity, float offset, in TeamData teamData, int playerId)
    {
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<ExtraBlockSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var blockEntity = EntityManager.Instantiate(prefab);
        
        EntityManager.AddComponentData(blockEntity, new FixToTargetData() {targetEntity = centerEntity, offset = offset});
        EntityManager.AddComponent<ExtraCubeData>(blockEntity);
        EntityManager.AddComponentData(blockEntity, new MovableCubeComponent {PlayerId = playerId});
        EntityManager.AddSharedComponentData(blockEntity, teamData);
    }

    private const float POS = 6.5f;
}