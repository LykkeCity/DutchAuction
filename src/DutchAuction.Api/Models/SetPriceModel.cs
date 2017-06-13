using System;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Set client's price model
    /// </summary>
    public class SetPriceModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Bid price in CHF
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// Bid date
        /// </summary>
        [Required]
        public DateTime Date { get; set; }
    }
}