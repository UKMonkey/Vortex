namespace Vortex.Interface.EntityBase.Damage
{
    public interface IEntityDamageHandler
    {
        int EstablishDamage(Entity target, float amount, DamageTypeEnum type, Entity dealer);
    }
}
