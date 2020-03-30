using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

// System for the server to setup gameplay objects
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ServerSetupGameSystem : ComponentSystem
{
    protected override void OnStartRunning()
    {
        GameData data = GameObject.FindObjectOfType<GameData>();
        
        // TODO: Figure out how to do this completely in code. As in add a new ghost by code. Is this even possible?
        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
        var ghostId = SelfDev_NetcodeGhostSerializerCollection.FindGhostType<SphereSnapshotData>();
        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
        var sphere = EntityManager.Instantiate(prefab);
        
#if UNITY_EDITOR
        EntityManager.SetName(sphere, "Puck");
#endif

        Debug.Log($"Server creating game entities {sphere}...");
    }

    protected override void OnUpdate()
    {
    }
}