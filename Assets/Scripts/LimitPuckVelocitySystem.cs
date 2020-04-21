using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class LimitPuckVelocitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach( (ref ServerPuckData puck, ref PhysicsVelocity vel) => { SetVel(ref vel); });
    }

    public static void SetVel(ref PhysicsVelocity vel)
    {
        vel.Angular = float3.zero;
        
        float speed = Vector3.Magnitude(vel.Linear);
        vel.Linear *= SPEED / speed;
    }
    
    public const float SPEED = 8f;
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PredictionLimitPuckVelocitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        
        Entities.ForEach( (ref ServerPuckData puck, ref PhysicsVelocity vel, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;

            LimitPuckVelocitySystem.SetVel(ref vel);
        });
    }
}