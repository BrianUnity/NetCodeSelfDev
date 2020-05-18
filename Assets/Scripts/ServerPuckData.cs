using Unity.Entities;

struct ServerPuckData : IComponentData
{
    
}

struct MainPlayerData : IComponentData
{
    
}

public struct FixToTargetData : IComponentData
{
    public Entity targetEntity;
    public float offset;
}

struct ExtraCubeData : IComponentData
{
}