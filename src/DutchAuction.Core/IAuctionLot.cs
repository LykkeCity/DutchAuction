using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core
{
    public interface IAuctionLot
    {
        string ClientId { get; }
        string AssetId { get; }
        double Volume { get; }
        double Price { get; }
        DateTime Date { get; }
    }

    public class AuctionLot : IAuctionLot
    {
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }

        public static AuctionLot Create(IAuctionLot src)
        {
            return new AuctionLot
            {
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }
    }

    public interface IAuctionLotRepository
    {
        Task AddAsync(IAuctionLot lot);
        Task<IEnumerable<IAuctionLot>> GetAllAsync();
    }
}
