using System;
using System.Collections.Generic;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Services.Auction.Models;

namespace DutchAuction.Services.Auction
{
    public class ClientAccountsService : IClientAccountsService
    {
        private readonly Dictionary<string, ClientAccountModel> _accounts;
        
        public ClientAccountsService()
        {
            _accounts = new Dictionary<string, ClientAccountModel>();
        }

        public void Add(string clientId, string assetId, double price, double volume)
        {
            lock (_accounts)
            {
                if(_accounts.ContainsKey(clientId))
                {
                    throw new InvalidOperationException($"Client account {clientId} already exist");
                }

                var account = new ClientAccountModel
                {
                    Price = price
                };

                account.AssetVolumes.Add(assetId, volume);

                _accounts.Add(clientId, account);
            }
        }

        public void SetPrice(string clientId, double price)
        {
            lock (_accounts)
            {
                if (!_accounts.TryGetValue(clientId, out ClientAccountModel account))
                {
                    throw new InvalidOperationException($"Client account {clientId} not found");
                }

                if (account.Price > price)
                {
                    throw new InvalidOperationException(
                        $"New price {price} for client account {clientId} is less than current price {account.Price}");
                }

                account.Price = price;
            }            
        }

        public void SetAssetVolume(string clientId, string assetId, double volume)
        {
            lock (_accounts)
            {
                if (!_accounts.TryGetValue(clientId, out ClientAccountModel account))
                {
                    throw new InvalidOperationException($"Client account {clientId} not found");
                }

                account.AssetVolumes.TryGetValue(assetId, out double oldVolume);

                if (oldVolume > volume)
                {
                    throw new InvalidOperationException(
                        $"New volume {volume} of assetId {assetId} for client account {clientId} is less than current volume {oldVolume}");
                }

                account.AssetVolumes[assetId] = volume;
            }
        }
    }
}