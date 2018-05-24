namespace PLS.Agit
{
    public interface IAgitServices
    {
        IAgitRepository LoadFromDirectory(string directoryName);
    }
}