using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DutchAuction.Core.Domain.Auction
{
    public class Bid : IBid
    {
        public string ClientId { get; }
        public double LimitPriceChf { get; private set; }
        public double LkkPriceChf { get; private set; }
        public IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumes { get; } 
        public BidState State { get; private set; }
        public IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumesLkk { get; }
        public IReadOnlyCollection<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; }

        private readonly List<KeyValuePair<string, double>> _assetVolumesChf;
        private readonly List<KeyValuePair<string, double>> _assetVolumesLkk;
        private readonly List<KeyValuePair<string, double>> _inMoneyAsssetVolumesLkk;

        public Bid(string clientId, double price, string assetId, double volume)
        {
            ClientId = clientId;
            LimitPriceChf = price;
            State = BidState.NotCalculatedYet;

            _assetVolumesChf = new List<KeyValuePair<string, double>>
            {
                new KeyValuePair<string, double>(assetId, volume)
            };

            _assetVolumesLkk = new List<KeyValuePair<string, double>>();
            _inMoneyAsssetVolumesLkk = new List<KeyValuePair<string, double>>();

            AssetVolumes = new ReadOnlyCollection<KeyValuePair<string, double>>(_assetVolumesChf);
            AssetVolumesLkk = new ReadOnlyCollection<KeyValuePair<string, double>>(_assetVolumesLkk);
            InMoneyAssetVolumesLkk = new ReadOnlyCollection<KeyValuePair<string, double>>(_inMoneyAsssetVolumesLkk);
        }

        public void SetPrice(double price)
        {
            State = BidState.NotCalculatedYet;

            LimitPriceChf = price;

            LkkPriceChf = 0;
            _assetVolumesLkk.Clear();
            _inMoneyAsssetVolumesLkk.Clear();
        }

        public double TryGetVolume(string assetId)
        {
            var index = _assetVolumesChf.FindIndex(item => item.Key == assetId);

            if (index >= 0)
            {
                return _assetVolumesChf[index].Value;
            }

            return 0d;
        }

        public void SetVolume(string assetId, double volume)
        {
            State = BidState.NotCalculatedYet;

            var index = _assetVolumesChf.FindIndex(item => item.Key == assetId);

            if (index >= 0)
            {
                _assetVolumesChf[index] = new KeyValuePair<string, double>(assetId, volume);
            }
            else
            {
                _assetVolumesChf.Add(new KeyValuePair<string, double>(assetId, volume));
            }

            LkkPriceChf = 0;
            _assetVolumesLkk.Clear();
            _inMoneyAsssetVolumesLkk.Clear();
        }

        public void SetInMoneyState(double currentLkkPriceChf)
        {
            LkkPriceChf = currentLkkPriceChf;
            State = BidState.InMoney;

            _assetVolumesLkk.Clear();
            _inMoneyAsssetVolumesLkk.Clear();

            foreach (var item in AssetVolumes)
            {
                var lkkItem = new KeyValuePair<string, double>(item.Key, item.Value / currentLkkPriceChf);

                _assetVolumesLkk.Add(lkkItem);
                _inMoneyAsssetVolumesLkk.Add(lkkItem);
            }
        }

        public void SetOutOfTheMoneyState(double currentLkkPriceChf)
        {
            LkkPriceChf = currentLkkPriceChf;
            State = BidState.OutOfTheMoney;

            _assetVolumesLkk.Clear();
            _inMoneyAsssetVolumesLkk.Clear();

            foreach (var item in AssetVolumes)
            {
                var lkkItem = new KeyValuePair<string, double>(item.Key, item.Value / currentLkkPriceChf);

                _assetVolumesLkk.Add(lkkItem);
            }
        }

        public void SetPartiallyInMoneyState(double currentLkkPriceChf, IEnumerable<KeyValuePair<string, double>> inMoneyAssetVolumesLkk)
        {
            LkkPriceChf = currentLkkPriceChf;
            State = BidState.PartiallyInMoney;

            _assetVolumesLkk.Clear();
            _inMoneyAsssetVolumesLkk.Clear();

            foreach (var item in AssetVolumes)
            {
                var lkkItem = new KeyValuePair<string, double>(item.Key, item.Value / currentLkkPriceChf);

                _assetVolumesLkk.Add(lkkItem);
            }

            _inMoneyAsssetVolumesLkk.AddRange(inMoneyAssetVolumesLkk);
        }
    }
}