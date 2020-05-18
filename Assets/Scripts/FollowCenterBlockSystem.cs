using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class FollowCenterBlockSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityQuery query = EntityManager.CreateEntityQuery(typeof(MainPlayerData), typeof (Translation));
        NativeArray<Entity> targets = query.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetPositions = query.ToComponentDataArray<Translation>(Allocator.TempJob);

        int count = query.CalculateEntityCount();
        NativeHashMap<Entity, Translation> map = new NativeHashMap<Entity, Translation>(count, Allocator.TempJob);
        for (int i=0; i<count; i++)
        {
            map.Add(targets[i], targetPositions[i]);
        }
        
        Entities.ForEach( (ref FixToTargetData target, ref Translation pos) => { SetPos(in map, in target, ref pos); });

        targets.Dispose();
        targetPositions.Dispose();
        map.Dispose();
    }
    
    public static void SetPos(in NativeHashMap<Entity, Translation> targetPositions, in FixToTargetData target, ref Translation pos)
    {
        Translation targetTranslation = targetPositions[target.targetEntity];
        pos.Value = targetTranslation.Value + (target.offset * OFFSET);
    }
    
    public static readonly float3 OFFSET = new float3(1.1f, 0f, 0f);
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PredictionFollowCenterBlockSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityQuery query = EntityManager.CreateEntityQuery(typeof(MainPlayerData), typeof (Translation));
        NativeArray<Entity> targets = query.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetPositions = query.ToComponentDataArray<Translation>(Allocator.TempJob);

        int count = query.CalculateEntityCount();
        NativeHashMap<Entity, Translation> map = new NativeHashMap<Entity, Translation>(count, Allocator.TempJob);
        for (int i=0; i<count; i++)
        {
            map.Add(targets[i], targetPositions[i]);
        }
        
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        
        Entities.ForEach((ref FixToTargetData target, ref Translation pos, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;

            FollowCenterBlockSystem.SetPos(in map, in target, ref pos);
        });

        targets.Dispose();
        targetPositions.Dispose();
        map.Dispose();
    }
}