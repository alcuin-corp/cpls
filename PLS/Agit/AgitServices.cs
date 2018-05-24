using System;
using PLS.CommandBuilders.Agit;

namespace PLS.Agit
{
    public class AgitServices : IAgitServices
    {
        public IAgitRepository LoadFromDirectory(string directoryName) => new AgitRepository(directoryName);

        public IAgitRepository LoadFromDatabase(string database) => throw new NotImplementedException();
    }
}