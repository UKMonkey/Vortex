using System.Collections.Generic;
using Psy.Core.Input;
using Psy.Gui;
using Psy.Gui.Loader;
using SlimMath;
using Vortex.Interface.Audio;
using Vortex.Interface.EntityBase;

namespace Vortex.Interface
{
    public interface IClient : IEngine
    {
        int LastKnownLatency { get; }
        uint LastKnownServerFrameNumber { get; }
        int TargetFrameLagAmount { get; }
        Entity Me { get; set; }
        IAudioEngine AudioEngine { get; }
        GuiManager Gui { get; }
        XmlLoader GuiLoader { get; }
        IInputBinder InputBinder { get; }
        IClientConfiguration Configuration { get; }

        /// <summary>
        /// Camera zoom level
        /// </summary>
        float ZoomLevel { get; set; }

        Vector3 WorldMousePosition { get; }
        Vector3 CameraPosition { get; }
        Ray CameraMouseRay { get; }

        void ConnectToServer(string hostname, int port);
        void Disconnect();

        void HideSplashImage();

        void BloodSpray(Vector3 position, float direction);

        /// <summary>
        /// Client - tells the engine to focus the camera on a specific entity
        /// </summary>
        /// <param name="entity"></param>
        void SetCameraTarget(Entity entity);

        void ShowSplashImage(string filename);

        /// <summary>
        /// these bullets aren't so bad!
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="colour"></param>
        void FireBullet(Vector3 from, Vector3 to, Color4 colour);

        /// <summary>
        /// Ask the engine to keep track of entities that are viewable by a specific entity
        /// this will put a strain on the system, so try to limit how many entities are called on this
        /// Entities that are registered on this system are automatically disabled when the entity is removed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="viewTester"></param>
        /// <param name="onVisible">mechanism to handle known visible entities</param>
        /// <param name="onHidden">mechanism to handle entities that are not visible</param>
        void RegisterViewSystemForEntity(Entity target, EntityTester viewTester, EntityHandler onVisible, EntityHandler onHidden);

        /// <summary>
        /// Set a callback which will decide if an entity should be visible or not
        /// </summary>
        /// <param name="entityViewRequirement"></param>
        void SetEntityViewCollector(EntityCollection entityViewRequirement);
    }
}