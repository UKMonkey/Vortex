namespace Vortex.Interface.EntityBase.Properties
{
    public enum EntityPropertyEnum : short
    {
        /// <summary>
        /// ID of the player if this entity is player-controlled
        /// </summary>
        RemotePlayer = 0,

        /// <summary>
        /// Parent entity ID
        /// </summary>
        Parent = 1,

        /// <summary>
        /// Is collision detection performed for this entity
        /// </summary>
        Solid = 2,

        /// <summary>
        ///  Will the position of this entity be updated
        /// </summary>
        Static = 3,

        /// <summary>
        /// Maximum hitpoints
        /// </summary>
        MaxHealth = 4,

        /// <summary>
        /// Current hitpoints
        /// </summary>
        Health = 5,

        /// <summary>
        /// Movement vector
        /// </summary>
        MovementVector = 6,

        /// <summary>
        /// Position vector
        /// </summary>
        Position = 7,

        /// <summary>
        /// Rate of change of roation
        /// </summary>
        RotationRate = 8,

        /// <summary>
        /// Target rotation value
        /// </summary>
        RotationTarget = 9,

        /// <summary>
        /// Rotation float
        /// </summary>
        Rotation = 10,

        /// <summary>
        /// Text of the nameplate
        /// </summary>
        Nameplate = 11,

        /// <summary>
        /// Color4 of the nameplate.
        /// </summary>
        NameplateColour = 12,

        /// <summary>
        /// TypeName used for debugging and performance stats
        /// </summary>
        TypeName = 13,

        /// <summary>
        /// Name of 3D model
        /// </summary>
        Model = 14,

        /// <summary>
        /// Range that this entity is able to see
        /// </summary>
        ViewRange = 16,

        /// <summary>
        /// Range of angles that this entity is able to see
        /// </summary>
        ViewAngleRange = 17,

        /// <summary>
        /// Range where everything is visible
        /// </summary>
        MeleeViewRange = 18,

        /// <summary>
        /// Mod entity properties enum values should start from here
        /// </summary>
        MaxEngineEnumProperty = 100,
    }
}
