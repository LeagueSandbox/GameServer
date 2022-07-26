namespace LeagueSandbox.GameServer.Content
{
    public class ServerCulling
    {
        /// <summary>
        /// Distances greater then this for turrets perform the LOS check, otherwise they don't.
        /// This was put in because of all the collision around Turrets. 
        /// </summary>
        public float ClosenessLineOfSightThresholdTurret { get; set; } = 200.0f;
    }
}
