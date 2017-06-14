using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DutchAuction.Core.Domain.Auction
{
    public class Bid : IBid
    {
        public string ClientId { get; }
        public double Price { get; private set; }
        public IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumes { get; } 
        public BidState State { get; private set; }
        public IReadOnlyCollection<KeyValuePair<string, double>> InMoneyAssetVolumes { get; }

        private readonly List<KeyValuePair<string, double>> _assetVolumes;
        private readonly List<KeyValuePair<string, double>> _inMoneyAsssetVolumes;

        public Bid(string clientId, double price, string assetId, double volume)
        {
            ClientId = clientId;
            Price = price;
            State = BidState.NotCalculatedYet;

            _assetVolumes = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>(assetId, volume)
            };

            _inMoneyAsssetVolumes = new List<KeyValuePair<string, double>>();

            AssetVolumes = new ReadOnlyCollection<KeyValuePair<string, double>>(_assetVolumes);
            InMoneyAssetVolumes = new ReadOnlyCollection<KeyValuePair<string, double>>(_inMoneyAsssetVolumes);
        }

        public void SetPrice(double price)
        {
            State = BidState.NotCalculatedYet;

            Price = price;
            
            _inMoneyAsssetVolumes.Clear();
        }

        public double TryGetVolume(string assetId)
        {
            var index = _assetVolumes.FindIndex(item => item.Key == assetId);

            if (index >= 0)
            {
                return _assetVolumes[index].Value;
            }

            return 0d;
        }

        public void SetVolume(string assetId, double volume)
        {
            State = BidState.NotCalculatedYet;

            var index = _assetVolumes.FindIndex(item => item.Key == assetId);

            if (index >= 0)
            {
                _assetVolumes[index] = new KeyValuePair<string, double>(assetId, volume);
            }
            else
            {
                _assetVolumes.Add(new KeyValuePair<string, double>(assetId, volume));
            }

            _inMoneyAsssetVolumes.Clear();
        }

        public void SetInMoneyState()
        {
            State = BidState.InMoney;

            _inMoneyAsssetVolumes.Clear();

            foreach (var item in AssetVolumes)
            {
                _inMoneyAsssetVolumes.Add(item);
            }
        }

        public void SetOutOfTheMoneyState()
        {
            State = BidState.OutOfTheMoney;

            _inMoneyAsssetVolumes.Clear();
        }

        public void SetPartiallyInMoneyState(IEnumerable<KeyValuePair<string, double>> inMoneyAssetVolumes)
        {
            State = BidState.PartiallyInMoney;

            _inMoneyAsssetVolumes.Clear();
            _inMoneyAsssetVolumes.AddRange(inMoneyAssetVolumes);
        }
    }
}