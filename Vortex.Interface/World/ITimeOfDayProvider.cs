namespace Vortex.Interface.World
{
    public interface ITimeOfDayProvider
    {
        /// <summary>
        /// The time of day in-game
        /// </summary>
        uint TimeOfDay { get; }

        /// <summary>
        /// The number of ticks in a day
        /// </summary>
        uint TicksPerDay { get; }
    }
}
