using SlimMath;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Interface.EntityBase
{
    public static class EntityFactoryExtensions
    {
        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, int value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short)type, value));
        }

        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, float value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short)type, value));
        }

        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, string value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short)type, value));
        }

        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, bool value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short)type, value));
        }

        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, Color4 value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short)type, value));
        }

        public static IEntityFactory RegisterDefaultProperty(this IEntityFactory factory, EntityPropertyEnum type, Vector3 value)
        {
            return factory.RegisterDefaultProperty(new EntityProperty((short) type, value));
        }



        public static IEntityFactory RegisterDefaultServerProperty(this IEntityFactory factory, EntityPropertyEnum type, float value)
        {
            var prop = new EntityProperty((short)type, value) {IsDirtyable = false};
            return factory.RegisterDefaultProperty(prop);
        }

        public static IEntityFactory RegisterDefaultServerProperty(this IEntityFactory factory, EntityPropertyEnum type, int value)
        {
            var prop = new EntityProperty((short)type, value) { IsDirtyable = false };
            return factory.RegisterDefaultProperty(prop);
        }

        public static IEntityFactory RegisterDefaultServerProperty(this IEntityFactory factory, EntityPropertyEnum type, string value)
        {
            var prop = new EntityProperty((short)type, value) { IsDirtyable = false };
            return factory.RegisterDefaultProperty(prop);
        }

        public static IEntityFactory RegisterDefaultServerProperty(this IEntityFactory factory, EntityPropertyEnum type, bool value)
        {
            var prop = new EntityProperty((short)type, value) { IsDirtyable = false };
            return factory.RegisterDefaultProperty(prop);
        }

        public static IEntityFactory RegisterDefaultServerProperty(this IEntityFactory factory, EntityPropertyEnum type, Color4 value)
        {
            var prop = new EntityProperty((short)type, value) { IsDirtyable = false };
            return factory.RegisterDefaultProperty(prop);
        }
    }
}
