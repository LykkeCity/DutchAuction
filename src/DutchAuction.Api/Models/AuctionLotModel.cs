using System;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    public class AuctionLotModel
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string AssetId { get; set; }
        [Required]
        public double Volume { get; set; }
        [Required]
        public double Price { get; set; }
        public DateTime Date { get; set; }
    }
}
