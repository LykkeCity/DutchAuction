using System.Linq;
using Autofac;
using System;
using System.Threading.Tasks;
using DutchAuction.Core.Domain;
using DutchAuction.Core.Services;

namespace DutchAuction.Services
{
    public class AuctionLotManager : IStartable
    {
        private readonly IAuctionLotRepository _repository;
        private readonly IAuctionLotCacheService _cacheService;

        public AuctionLotManager(
            IAuctionLotRepository auctionLotRepository,
            IAuctionLotCacheService cacheService)
        {
            _repository = auctionLotRepository;
            _cacheService = cacheService;
        }

        public void Start()
        {
            UpdateCache().Wait();
        }

        public async Task AddAsync(string clientId, string assetId, double price, double volume)
        {
            var lot = new AuctionLot
            {
                ClientId = clientId,
                AssetId = assetId,
                Date = DateTime.UtcNow,
                Price = price,
                Volume = volume
            };

            await _repository.AddAsync(lot);
            _cacheService.Add(lot);
        }

        private async Task UpdateCache()
        {
            var lots = await _repository.GetAllAsync();
            _cacheService.InitCache(lots.ToList());
        }
    }
}
