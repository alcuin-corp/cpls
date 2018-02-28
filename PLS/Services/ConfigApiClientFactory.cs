namespace PLS.Services
{
    public delegate IConfigApiClient ConfigApiClientFactory(string uri, string login, string password);
}