using System.ComponentModel.DataAnnotations;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Api.Models
{
    public class AuctionOperationResponse
    {
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
