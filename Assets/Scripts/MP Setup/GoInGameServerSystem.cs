using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

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
                var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<CubeSnapshotData>();
                var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
                var player = EntityManager.Instantiate(prefab);

                int playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;
                EntityManager.SetComponentData(player, new MovableCubeComponent {PlayerId = playerId});
                PostUpdateCommands.AddBuffer<CubeInput>(player);

                bool up = (playerId % 2) == 0;
                EntityManager.SetComponentData(player, new Translation { Value = up ?
                    new float3(0f, 0f, 6f) : new float3(0f, 0f, -6f)});

                Debug.Log($"Creating input buffer {player}");

                PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent {targetEntity = player});

                PostUpdateCommands.DestroyEntity(reqEnt);
            });
    }
}