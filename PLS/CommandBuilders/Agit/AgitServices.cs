namespace PLS.CommandBuilders.Agit
{
    public class AgitServices : IAgitServices
    {
        public IAgitRepository LoadFromDirectory(string directoryName) => new AgitRepository(directoryName);
    }
}