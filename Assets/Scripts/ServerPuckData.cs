using Unity.Entities;

struct ServerPuckData : IComponentData
{
    
}

struct MainPlayerData : IComponentData
{
    public int teamID;
}

public struct FixToTargetData : IComponentData
{
    public Entity targetEntity;
    public float offset;
}

struct ExtraCubeData : IComponentData
{
}

struct TeamData : ISharedComponentData
{
    public int teamID;
}