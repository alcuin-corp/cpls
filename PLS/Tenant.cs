using System.ComponentModel.DataAnnotations;

namespace PLS
{
    public class Tenant
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string ConfigDb { get; set; }
        [Required]
        public string PublicDb { get; set; }
        public Server Server { get; set; }
        [Required]
        public string ServerId { get; set; }
    }
}