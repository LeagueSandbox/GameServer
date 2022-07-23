namespace GameServerCore.Domain
{
    /// <summary>
    /// Class meant to tie all game updates to the main game tick loop.
    /// </summary>
    public interface IUpdate
    {
        /// <summary>
        /// Function tied to the main game tick loop.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        void Update(float diff)
        {
        }
    }
}