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

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume)
        {
            lock (_accounts)
            {
                if(_accounts.ContainsKey(clientId))
                {
                    return AuctionOperationResult.AccountAlreadyExist;
                }

                var account = new ClientAccountModel
                {
                    Price = price
                };

                account.AssetVolumes.Add(assetId, volume);

                _accounts.Add(clientId, account);
            }

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult SetPrice(string clientId, double price)
        {
            lock (_accounts)
            {
                if (!_accounts.TryGetValue(clientId, out ClientAccountModel account))
                {
                    return AuctionOperationResult.AccountNotFound;
                }

                if (account.Price > price)
                {
                    return AuctionOperationResult.PriceIsLessThanCurrent;
                }

                account.Price = price;
            }

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult SetAssetVolume(string clientId, string assetId, double volume)
        {
            lock (_accounts)
            {
                if (!_accounts.TryGetValue(clientId, out ClientAccountModel account))
                {
                    return AuctionOperationResult.AccountNotFound;
                }

                account.AssetVolumes.TryGetValue(assetId, out double oldVolume);

                if (oldVolume > volume)
                {
                    return AuctionOperationResult.VolumeIsLessThanCurrent;
                }

                account.AssetVolumes[assetId] = volume;
            }

            return AuctionOperationResult.Ok;
        }
    }
}