using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using DutchAuction.Api.Models;
using DutchAuction.Core;
using DutchAuction.Services;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DutchAuction.Api.Controllers
{
    [Route("api/[controller]")]
    public class LotsController : Controller
    {
        private readonly ApplicationSettings _settings;
        private readonly IAuctionLotCacheService _auctionLotCacheService;
        private readonly AuctionLotManager _auctionLotManager;

        public LotsController(
            ApplicationSettings settings,
            IAuctionLotCacheService auctionLotCacheService,
            AuctionLotManager auctionLotManager)
        {
            _settings = settings;
            _auctionLotCacheService = auctionLotCacheService;
            _auctionLotManager = auctionLotManager;
        }

        [HttpGet]
        [Route("getOrderbook/{assetId?}")]
        public Order[] Get(string assetId)
        {
            return _auctionLotCacheService.GetOrderbook(assetId);
        }

        [HttpPost("add")]
        public async Task<ResponseModel> AddLot([FromBody]AuctionLot model)
        {
            if (string.IsNullOrEmpty(model.ClientId))
            {
                return ResponseModel.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField,
                    $"{nameof(model.ClientId)} is required");
            }

            if (string.IsNullOrEmpty(model.AssetId))
            {
                return ResponseModel.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField,
                    $"{nameof(model.AssetId)} is required");
            }

            if (!_settings.Assets.Contains(model.AssetId))
            {
                return ResponseModel.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField,
                    $"wrong {nameof(model.AssetId)}");
            }

            if (model.Price <= 0)
            {
                return ResponseModel.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField,
                    $"wrong {nameof(model.Price)}");
            }

            if (model.Volume <= 0)
            {
                return ResponseModel.CreateFail(ResponseModel.ErrorCodeType.InvalidInputField,
                    $"wrong {nameof(model.Volume)}");
            }

            //TODO: validate model.ClientId
            await _auctionLotManager.AddAsync(model.ClientId, model.AssetId, model.Price, model.Volume);

            return ResponseModel.CreateOk();
        }
    }
}
