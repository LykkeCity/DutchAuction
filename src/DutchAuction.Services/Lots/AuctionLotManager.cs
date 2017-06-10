using System;
using System.Linq;
using System.Threading.Tasks;
using DutchAuction.Core.Domain.Lots;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Lots;

namespace DutchAuction.Services.Lots
{
    public class AuctionLotManager :
        IAuctionLotManager
    {
        private readonly IAuctionLotRepository _repository;
        private readonly IAuctionLotCacheService _cacheService;
        private readonly IAssetExchangeService _assetrExchangeService;

        public AuctionLotManager(
            IAuctionLotRepository auctionLotRepository,
            IAuctionLotCacheService cacheService,
            IAssetExchangeService assetrExchangeService)
        {
            _repository = auctionLotRepository;
            _cacheService = cacheService;
            _assetrExchangeService = assetrExchangeService;
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

        public Order[] GetOrderbook()
        {
            return _cacheService
                .GetAllAsync()
                .Select(item => new
                {
                    Price = _assetrExchangeService.Exchange(item.Price, item.AssetId, "CHF"),
                    ClientId = item.ClientId,
                    Volume = item.Volume
                })
                .GroupBy(item => item.Price)
                .Select(group => new Order
                {
                    Investors = group.Select(item => item.ClientId).Distinct().Count(),
                    Price = group.Key,
                    Volume = group.Sum(item => item.Volume)
                })
                .OrderByDescending(order => order.Price)
                .ToArray();
        }

        private async Task UpdateCache()
        {
            var lots = await _repository.GetAllAsync();
            _cacheService.InitCache(lots.ToList());
        }
    }
}
