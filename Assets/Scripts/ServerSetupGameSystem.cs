using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;
using Random = UnityEngine.Random;

// System for the server to setup gameplay objects
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerSetupGameSystem : ComponentSystem
{
    protected override void OnStartRunning()
    {
        GameData data = GameObject.FindObjectOfType<GameData>();
        
        // TODO: Figure out how to do this completely in code. As in add a new ghost by code. Is this even possible?
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<PuckSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var puck = EntityManager.Instantiate(prefab);
        
#if UNITY_EDITOR
        EntityManager.SetName(puck, "Puck");
#endif

        Vector2 vel = Random.insideUnitCircle * LimitPuckVelocitySystem.SPEED;
        EntityManager.SetComponentData(puck, new PhysicsVelocity()
        {
            Linear = new float3(vel.x, 0f, vel.y),
            Angular = float3.zero
        });

        EntityManager.AddComponent<ServerPuckData>(puck);

        Debug.Log($"Server creating game entities {puck}...");
    }

    protected override void OnUpdate()
    {
    }
}