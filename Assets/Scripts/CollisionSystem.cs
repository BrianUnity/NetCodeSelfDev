using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Collider = Unity.Physics.Collider;

struct CollisionSystemJob : ICollisionEventsJob
{
    public NativeArray<bool> collided;
    public NativeArray<Entity> collision; 
    
    public void Execute(CollisionEvent collisionEvent)
    {
        collided[0] = true;
        collision[0] = collisionEvent.Entities.EntityB;
    }
}

[UpdateBefore(typeof(EndFramePhysicsSystem))]
[UpdateAfter(typeof(StepPhysicsWorld))]
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class RemoveBlockSystem : JobComponentSystem
{
    BuildPhysicsWorld m_BuildPhysicsWorld;
    StepPhysicsWorld m_StepPhysicsWorld;
    EndFramePhysicsSystem m_endFrameSystem;
    
    protected override void OnCreate()
    {
        m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_endFrameSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();
    }
    
    protected unsafe override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CollisionSystemJob()
        {
            collided = new NativeArray<bool>(1, Allocator.TempJob),
            collision = new NativeArray<Entity>(1, Allocator.TempJob),
        };
        job.Schedule(m_StepPhysicsWorld.Simulation, ref m_BuildPhysicsWorld.PhysicsWorld, inputDeps).Complete();

        if (job.collided[0])
        {
            Entity entity = job.collision[0];
            
            if (EntityManager.HasComponent<ExtraCubeData>(entity))
                EntityManager.DestroyEntity(entity);
        }

        job.collided.Dispose();
        job.collision.Dispose();
        
        return inputDeps;
    }
}