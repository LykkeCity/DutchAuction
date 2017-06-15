using System.ComponentModel.DataAnnotations;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Api.Models
{
    public class AuctionOperationResponse
    {
        [Required]
        [EnumDataType(typeof(AuctionOperationResult))]
        public AuctionOperationResult Result { get; set; }

        public static AuctionOperationResponse Create(AuctionOperationResult result)
        {
            return new AuctionOperationResponse
            {
                Result = result
            };
        }
    }
}
