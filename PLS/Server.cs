using System.ComponentModel.DataAnnotations;

namespace PLS
{
    public class Server
    {
        [Key]
        public string Id { get; set; }
        public string Hostname { get; set; } = "localhost";
        public string Login { get; set; } = "sa";
        public string Password { get; set; } = "P@ssw0rd";
    }
}