using System.IO;
using System.Linq;
using System.Reflection;
using Vortex.Interface;

namespace Vortex.Mod
{
    public static class ModLoader
    {
        public static T LoadModule<T>(string dllName, StartArguments arguments)
        {
            var gameAssembly = Assembly.LoadFrom(Path.Combine("Mods", arguments.ModName, dllName));

            return (from t in gameAssembly.GetTypes()
                    where t.UnderlyingSystemType.Name == "Loader"
                    let methodInfo = t.GetMethod("Load")
                    select methodInfo.Invoke(t, new object[] {arguments})
                    into result select (T) result).First();
        }
    }
}
