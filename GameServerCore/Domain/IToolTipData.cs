namespace GameServerCore.Domain
{
    public interface IToolTipData
    {
        uint NetID { get; }
        byte Slot { get; }
        /// <summary>
        /// Writing to this array will cause an exception
        /// </summary>
        IToolTipValue[] Values { get; }
        bool Changed { get; }
        void MarkAsUnchanged();
        void Update<T>(int tipIndex, T value) where T : struct;
    }

    public interface IToolTipValue
    {
        bool Hide { get; }
        float Value { get; }
        bool IsFloat { get; }
        bool Changed { get; }
    }
}
