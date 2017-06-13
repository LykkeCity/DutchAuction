using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        public void Validate(ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                modelState.AddModelError(nameof(ClientId), "Client is required");
            }

            if (Price <= 0)
            {
                modelState.AddModelError(nameof(Price), "Price should be positive number");
            }
        }
    }
}