using Vortex.Interface.Audio;

namespace Vortex.Audio
{
    public class AudioLookup : IAudioLookup
    {
        private readonly StringTable _stringTable;


        public AudioLookup()
        {
            _stringTable = new StringTable();
        }

        public void RegisterSound(string soundFile)
        {
            _stringTable.AddString(soundFile);
        }

        public int GetSoundId(string soundFile)
        {
            return _stringTable.GetStringId(soundFile);
        }

        public string GetSoundFile(int id)
        {
            return _stringTable.GetString(id);
        }
    }
}
