namespace Vortex.Interface.Audio
{
    /**
     * Server and client should call Register Sound
     * with the same data in the same order.  This will allow both 
     * to produce the same mapping of string to id.
     */
    public interface IAudioLookup
    {
        void RegisterSound(string soundFile);
        int GetSoundId(string soundFile);
        string GetSoundFile(int id);
    }
}
