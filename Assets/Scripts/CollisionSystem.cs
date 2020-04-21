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
    public NativeArray<ColliderKey> collisionKey; 
    
    public void Execute(CollisionEvent collisionEvent)
    {
        Debug.Log($"{collisionEvent.ColliderKeys.ColliderKeyA.Value} --- {collisionEvent.ColliderKeys.ColliderKeyB.Value}");

        collided[0] = true;
        collision[0] = collisionEvent.Entities.EntityB;
        collisionKey[0] = collisionEvent.ColliderKeys.ColliderKeyB;
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
            collisionKey = new NativeArray<ColliderKey>(1, Allocator.TempJob),
        };
        job.Schedule(m_StepPhysicsWorld.Simulation, ref m_BuildPhysicsWorld.PhysicsWorld, inputDeps).Complete();

        if (job.collided[0])
        {
            PhysicsCollider physCol = EntityManager.GetComponentData<PhysicsCollider>(job.collision[0]);
            CompoundCollider* col = (CompoundCollider*)(physCol.ColliderPtr);
            ColliderKey key = job.collisionKey[0];
            bool foundChild = col->GetChild(ref key, out ChildCollider child);
            
            if (foundChild)
                Debug.Log($"Collided: {job.collision[0]} | {job.collisionKey[0].Value} | {foundChild} | {child}");
        }
        
        return inputDeps;
    }
}