using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class OrderbookService : IOrderbookService
    {
        private class ClientAccountModel
        {
            public double Price { get; set; }
            public Dictionary<string, double> AssetVolumes { get; } = new Dictionary<string, double>();
        }

        private readonly IAssetExchangeService _assetExchangeService;
        private readonly Dictionary<string, ClientAccountModel> _clientAccounts;
        private readonly ReaderWriterLockSlim _lock;

        public OrderbookService(IAssetExchangeService assetExchangeService)
        {
            _assetExchangeService = assetExchangeService;

            _clientAccounts = new Dictionary<string, ClientAccountModel>();
            _lock = new ReaderWriterLockSlim();
        }

        public Order[] Render()
        {
            _lock.EnterReadLock();

            try
            {
                return _clientAccounts
                    .Select(i => new
                    {
                        ClientId = i.Key,
                        Price = i.Value.Price,
                        Volume = i.Value.AssetVolumes
                            .Select(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF"))
                            .Sum(chf => chf)
                    })
                    .GroupBy(i => i.Price)
                    .Select(g => new Order
                    {
                        Investors = g.Select(i => i.ClientId).Distinct().Count(),
                        Price = g.Key,
                        Volume = g.Sum(i => i.Volume)
                    })
                    .OrderByDescending(o => o.Price)
                    .ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void OnClientAccountAdded(string clientId, string assetId, double price, double volume)
        {
            _lock.EnterWriteLock();

            try
            {
                var account = new ClientAccountModel
                {
                    Price = price
                };

                account.AssetVolumes[assetId] = volume;

                _clientAccounts.Add(clientId, account);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void OnPriceSet(string clientId, double price)
        {
            _lock.EnterWriteLock();

            try
            {
                _clientAccounts[clientId].Price = price;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void OnAssetVolumeSet(string clientId, string assetId, double volume)
        {
            _lock.EnterWriteLock();

            try
            {
                _clientAccounts[clientId].AssetVolumes[assetId] = volume;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}