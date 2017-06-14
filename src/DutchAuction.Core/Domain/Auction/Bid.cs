using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace DutchAuction.Core.Domain.Auction
{
    /// <summary>
    /// Immutable bid
    /// </summary>
    public class Bid : IBid
    {
        public string ClientId { get; }
        public double Price { get; }
        public IReadOnlyDictionary<string, double> AssetVolumes { get; }
        public BidState State { get; }
        public IReadOnlyDictionary<string, double> InMoneyAssetVolumes { get; }
        
        public Bid(string clientId, double price, string assetId, double volume)
        {
            ClientId = clientId;
            Price = price;
            AssetVolumes = new ReadOnlyDictionary<string, double>(new Dictionary<string, double>
            {
                {assetId, volume}
            });
            State = BidState.NotCalculatedYet;
            InMoneyAssetVolumes = new ReadOnlyDictionary<string, double>(new Dictionary<string, double>());
        }

        private Bid(string clientId, double price, IReadOnlyDictionary<string, double> assetVolumes, BidState state, IReadOnlyDictionary<string, double> inMoneyAssetVolumes)
        {
            ClientId = clientId;
            Price = price;
            AssetVolumes = assetVolumes;
            State = state;
            InMoneyAssetVolumes = inMoneyAssetVolumes;
  
        }

        public Bid SetPrice(double price)
        {
            return new Bid(ClientId, price, AssetVolumes, State, InMoneyAssetVolumes);
        }

        public Bid SetVolume(string assetId, double volume)
        {
            var assetVolumes = AssetVolumes.ToDictionary(i => i.Key, i => i.Value);

            assetVolumes[assetId] = volume;

            return new Bid(
                ClientId, 
                Price, 
                new ReadOnlyDictionary<string, double>(assetVolumes), 
                BidState.NotCalculatedYet, 
                new ReadOnlyDictionary<string, double>(new Dictionary<string, double>()));
        }

        public Bid SetInMoneyState()
        {
            return new Bid(ClientId, Price, AssetVolumes, BidState.InMoney, AssetVolumes);
        }

        public Bid SetOutOfTheMoneyState()
        {
            return new Bid(ClientId, Price, AssetVolumes, BidState.OutOfTheMoney, new ReadOnlyDictionary<string, double>(new Dictionary<string, double>()));
        }

        public Bid SetPartiallyInMoneyState(IDictionary<string, double> inMoneyAssetVolumes)
        {
            return new Bid(ClientId, Price, AssetVolumes, BidState.PartiallyInMoney, inMoneyAssetVolumes.ToImmutableDictionary());
        }
    }
}