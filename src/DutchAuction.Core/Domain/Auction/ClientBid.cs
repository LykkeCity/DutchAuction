using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DutchAuction.Core.Domain.Auction
{
    public class ClientBid : IClientBid
    {
        public string ClientId { get; }
        public double LimitPriceChf { get; }
        public IImmutableList<KeyValuePair<string, double>> AssetVolumes { get; } 
        
        public ClientBid(string clientId, double price, string assetId, double volume)
        {
            ClientId = clientId;
            LimitPriceChf = price;
            AssetVolumes = ImmutableArray.Create(new KeyValuePair<string, double>(assetId, volume));
        }

        private ClientBid(string clientId, double price, IImmutableList<KeyValuePair<string, double>> assetVolumes)
        {
            ClientId = clientId;
            LimitPriceChf = price;
            AssetVolumes = assetVolumes;
        }

        public ClientBid SetPrice(double price)
        {
            return new ClientBid(ClientId, price, AssetVolumes);
        }

        public double TryGetVolume(string assetId)
        {
            return AssetVolumes
                .Where(item => item.Key == assetId)
                .Select(item => item.Value)
                .SingleOrDefault();
        }

        public ClientBid SetVolume(string assetId, double volume)
        {
            var indices = AssetVolumes
                .Where(item => item.Key == assetId)
                .Select((item, itemIndex) => itemIndex)
                .ToArray();
            
            switch (indices.Length)
            {
                case 1:
                    return new ClientBid(ClientId, LimitPriceChf, AssetVolumes.SetItem(indices[0], new KeyValuePair<string, double>(assetId, volume)));

                case 0:
                    return new ClientBid(ClientId, LimitPriceChf, AssetVolumes.Add(new KeyValuePair<string, double>(assetId, volume)));

                default:
                    throw new InvalidOperationException($"More than one asset with key {assetId} found");
            }
        }
    }
}