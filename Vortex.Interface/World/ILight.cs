using SlimMath;

namespace Vortex.Interface.World
{
    public interface ILight
    {
        /// <summary>
        /// Is the light in a state that needs to be broadcast to the clients
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Will this light move?
        /// </summary>
        bool IsDynamic { get; set; }

        /// <summary>
        /// Location of the light
        /// </summary>
        Vector3 Position { get; set;  }

        /// <summary>
        /// Colour of the light
        /// </summary>
        Color4 Colour { get; set; }

        /// <summary>
        /// Intensity of the light
        /// </summary>
        float Brightness { get; set; }

        /// <summary>
        /// Is the light actually something that should be processed
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Give the light intensity at a given point
        /// </summary>
        Color4 ColorAt(Vector3 samplePoint);

        /// <summary>
        /// Return a new light of the same time, but translated a bit
        /// </summary>
        ILight Translate(Vector3 amount);

        /// <summary>
        /// Remove dirty flag.
        /// </summary>
        void Checked();
    }
}
