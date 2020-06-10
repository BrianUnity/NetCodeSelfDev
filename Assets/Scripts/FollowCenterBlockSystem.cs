using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class FollowCenterBlockSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MainPlayerData mainPlayer, ref Translation targetTranslation) =>
        {
            //Debug.Log($"Setting Pos: {mainPlayer.teamID}: {targetTranslation}");
            
            int playerId = mainPlayer.teamID;
            Translation target = targetTranslation;
            Entities.ForEach((ref MovableCubeComponent player, ref FixToTargetData targetData, ref Translation cubeTranslation) =>
            {
                //Debug.Log($"Setting Pos: {player.PlayerId} -> {playerId}: {cubeTranslation.Value} -> {target.Value}");
                
                if (player.PlayerId != playerId)
                    return;
                
                SetPos(target, targetData, ref cubeTranslation);
            });
        });
    }
    
    public static void SetPos(in Translation targetPosition, in FixToTargetData target, ref Translation pos)
    {
        pos.Value = targetPosition.Value + (target.offset * OFFSET);
    }
    
    public static readonly float3 OFFSET = new float3(1.1f, 0f, 0f);
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class PredictionFollowCenterBlockSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        
        Entities.ForEach((ref MainPlayerData mainPlayer, ref Translation targetTranslation) =>
        {
            int playerId = mainPlayer.teamID;
            Translation target = targetTranslation;
            Entities.ForEach((ref MovableCubeComponent player, ref FixToTargetData targetData, ref Translation cubeTranslation, ref PredictedGhostComponent prediction) =>
            {
                if (player.PlayerId != playerId)
                    return;
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                    return;
                
                FollowCenterBlockSystem.SetPos(target, targetData, ref cubeTranslation);
            });
        });
    }
}