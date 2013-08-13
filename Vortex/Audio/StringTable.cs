using System.Collections.Generic;

namespace Vortex.Audio
{
    public class StringTable
    {
        private readonly Dictionary<int, string> _idToString;
        private readonly Dictionary<string, int> _stringToId;
 
        public StringTable()
        {
            _idToString = new Dictionary<int, string>();
            _stringToId = new Dictionary<string, int>();
        }

        public void AddString(string str)
        {
            if (_stringToId.ContainsKey(str))
                return;

            var id = str.GetHashCode();
            while (_idToString.ContainsKey(id))
                ++id;

            _idToString.Add(id, str);
            _stringToId.Add(str, id);
        }

        public int GetStringId(string str)
        {
            return _stringToId[str];
        }

        public string GetString(int id)
        {
            return _idToString[id];
        }
    }
}
