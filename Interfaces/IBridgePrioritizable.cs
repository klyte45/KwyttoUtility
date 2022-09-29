namespace Kwytto.Interfaces
{
    public interface IBridgePrioritizable
    {
        int Priority { get; }
        bool IsBridgeEnabled { get; }
    }
}