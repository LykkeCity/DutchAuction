using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Checks service is alive response
    /// </summary>
    public class IsAliveResponse
    {
        /// <summary>
        /// API version
        /// </summary>
        [Required]
        public string Version { get; set; }
        
        /// <summary>
        /// Environment variables
        /// </summary>
        [Required]
        public string Env { get; set; }
    }
}