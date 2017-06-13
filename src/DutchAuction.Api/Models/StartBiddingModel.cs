using System;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Start client's bidding
    /// </summary>
    public class StartBiddingModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Asset ID (CHF, USD...)
        /// </summary>
        [Required]
        public string AssetId { get; set; }

        /// <summary>
        /// Volume of given asset
        /// </summary>
        [Required]
        public double Volume { get; set; }

        /// <summary>
        /// Bid price in CHF
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// Bid date
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        public void Validate()
        {
            //if (string.IsNullOrEmpty(model.ClientId))
            //{
            //    return ResponseModel.CreateFail(ResponseModel.ErrorCode.InvalidInputField,
            //        $"{nameof(model.ClientId)} is required");
            //}

            //if (string.IsNullOrEmpty(model.AssetId))
            //{
            //    return ResponseModel.CreateFail(ResponseModel.ErrorCode.InvalidInputField,
            //        $"{nameof(model.AssetId)} is required");
            //}

            //if (!_settings.Assets.Contains(model.AssetId))
            //{
            //    return ResponseModel.CreateFail(ResponseModel.ErrorCode.InvalidInputField,
            //        $"wrong {nameof(model.AssetId)}");
            //}

            //if (model.Price <= 0)
            //{
            //    return ResponseModel.CreateFail(ResponseModel.ErrorCode.InvalidInputField,
            //        $"wrong {nameof(model.Price)}");
            //}

            //if (model.Volume <= 0)
            //{
            //    return ResponseModel.CreateFail(ResponseModel.ErrorCode.InvalidInputField,
            //        $"wrong {nameof(model.Volume)}");
            //}
        }
    }
}
