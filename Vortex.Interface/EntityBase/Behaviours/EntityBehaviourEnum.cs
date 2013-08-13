namespace Vortex.Interface.EntityBase.Behaviours
{
    public enum EntityBehaviourEnum : short
    {
        OnSpawn,
        OnKilled,
        OnCollisionWithEntity,
        OnCollisionWithTerrain,
        OnInteract,
        Think,
        PlayerChanged,
        MaxEngineEnumBehaviour = 100
    }
}
