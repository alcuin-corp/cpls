namespace PLS.CommandBuilders.Agit
{
    public interface IAgitServices
    {
        IAgitRepository LoadFromDirectory(string directoryName);
    }
}