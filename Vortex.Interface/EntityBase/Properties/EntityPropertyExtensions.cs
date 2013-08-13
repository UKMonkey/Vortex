using System;
using SlimMath;
using Vortex.Interface.Net;
using EntityBehaviourEnum = Vortex.Interface.EntityBase.Behaviours.EntityBehaviourEnum;

namespace Vortex.Interface.EntityBase.Properties
{
    public static class EntityPropertyExtensions
    {
        public static EntityProperty GetProperty(this Entity entity, EntityPropertyEnum target)
        {
            var realVal = (short) target;
            return entity.GetProperty(realVal);
        }

        public static short GetParentId(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.Parent).ShortValue;
        }

        public static void SetParentId(this Entity entity, short value)
        {
            entity.GetProperty(EntityPropertyEnum.Parent).ShortValue = value;
        }

        public static bool GetSolid(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.Solid).BoolValue;
        }
  
        public static void SetSolid(this Entity entity, bool value)
        {
            entity.GetProperty(EntityPropertyEnum.Solid).BoolValue = value;
        }

        public static bool GetStatic(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.Static).BoolValue;
        }
  
        public static void SetStatic(this Entity entity, bool value)
        {
            entity.GetProperty(EntityPropertyEnum.Static).BoolValue = value;
        }

        public static int GetMaxHealth(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.MaxHealth).IntValue;
        }
  
        public static void SetMaxHealth(this Entity entity, int value)
        {
            entity.GetProperty(EntityPropertyEnum.MaxHealth).IntValue = value;
        }

        public static int GetHealth(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.Health).IntValue;
        }
  
        public static void SetHealth(this Entity entity, int value)
        {
            var maxHealth = entity.GetMaxHealth();
            var newHealth = Math.Max(Math.Min(value, maxHealth), 0);

            entity.GetProperty(EntityPropertyEnum.Health).IntValue = newHealth;
            if (newHealth == 0)
            {
                entity.PerformBehaviour((int)EntityBehaviourEnum.OnKilled, null);
                entity.Destroy();
            }
        }

        /****/

        public static Vector3 GetMovementVector(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.MovementVector).VectorValue;
        }

        public static void SetMovementVector(this Entity entity, Vector3 value)
        {
            entity.GetProperty(EntityPropertyEnum.MovementVector).VectorValue = value;
        }

        public static ushort? GetPlayerId(this Entity entity)
        {
            if (!entity.HasProperty((int)EntityPropertyEnum.RemotePlayer))
                return null;
            return (ushort)entity.GetProperty(EntityPropertyEnum.RemotePlayer).ShortValue;
        }


        /****/

        public static float GetRotationSpeed(this Entity entity)
        {
            if (!entity.HasProperty((int)EntityPropertyEnum.RotationRate))
                return 0;
            return entity.GetProperty(EntityPropertyEnum.RotationRate).FloatValue;
        }

        public static void SetRotationSpeed(this Entity entity, float value)
        {
            entity.SetProperty(new EntityProperty((short) EntityPropertyEnum.RotationRate, value));
        }


        /****/

        public static float GetRotationTarget(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.RotationTarget).FloatValue;
        }

        public static void SetRotationTarget(this Entity entity, float value)
        {
            entity.SetProperty(new EntityProperty((short)EntityPropertyEnum.RotationTarget, value));
        }


        /// <summary>
        /// Get the player associated with this entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="engine"></param>
        /// <returns>An instance of RemotePlayer, or null if this entity is not controlled by a player.</returns>
        public static RemotePlayer GetPlayer(this Entity entity, IEngine engine)
        {
            var id = entity.GetPlayerId();
            if (id == null)
                return null;
            return engine.RemotePlayers.GetRemotePlayer(id.Value);
        }

        public static void SetPlayer(this Entity entity, RemotePlayer player)
        {
            entity.SetProperty(new EntityProperty((short)EntityPropertyEnum.RemotePlayer, (short)player.ClientId));
        }

        /****/
        public static float GetViewRange(this Entity entity)
        {
            return entity.GetProperty((short) EntityPropertyEnum.ViewRange).FloatValue;
        }

        public static void SetViewRange(this Entity entity, float value)
        {
            entity.SetProperty(new EntityProperty((short)EntityPropertyEnum.ViewRange, value));
        }

        /****/
        public static float GetViewAngleRange(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.ViewAngleRange).FloatValue;
        }

        public static void SetViewAngleRange(this Entity entity, float value)
        {
            entity.SetProperty(new EntityProperty((short)EntityPropertyEnum.ViewAngleRange, value));
        }


        /****/
        public static float GetMeleeViewRange(this Entity entity)
        {
            return entity.GetProperty(EntityPropertyEnum.MeleeViewRange).FloatValue;
        }

        public static void SetMeleeViewRange(this Entity entity, float value)
        {
            entity.SetProperty(new EntityProperty((short)EntityPropertyEnum.MeleeViewRange, value));
        }

        /// <summary>
        /// ensure that the angle is between -2Pi & 2Pi
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float LimitAngle(float angle)
        {
            var ret = angle;

            while (ret > (2 * Math.PI))
            {
                ret -= (float)(2 * Math.PI);
            }
            while (ret <= (-2 * Math.PI))
            {
                ret += (float)(2 * Math.PI);
            }

            return ret;
        }
    }
}
