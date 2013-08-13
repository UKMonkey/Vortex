namespace Vortex.Interface
{
    public class StartArguments
    {
        public string ModName;

        private StartArguments(string modName)
        {
            ModName = modName;
        }

        public static StartArguments Empty()
        {
            return new StartArguments("");
        }

        public static StartArguments ParseFrom(string[] args)
        {
            var modName = "";

            for (int i = 0; i < args.Length; i++)
            {
                var atl = args[i].ToLower();

                if (atl == "-mod")
                {
                    modName = args[i + 1];
                }
            }

            return new StartArguments(modName);
        }
    }
}
