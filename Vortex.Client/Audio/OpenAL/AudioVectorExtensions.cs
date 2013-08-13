using SlimMath;

namespace Vortex.Client.Audio.OpenAL
{
    public static class AudioVectorExtensions
    {
        public static OpenTK.Vector3 ToOpenTkVector(this Vector3 vec)
        {
            return new OpenTK.Vector3(vec.X, vec.Y, vec.Z);
        }

        public static float[] ToFloatArray(this Vector3 vec)
        {
            return new float[] {vec.X, vec.Y, vec.Z};
        }
    }
}