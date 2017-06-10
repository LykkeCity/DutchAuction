﻿using System.Collections.Generic;
using DutchAuction.Core.Domain.Lots;

namespace DutchAuction.Core.Services.Lots
{
    public interface IAuctionLotCacheService
    {
        IAuctionLot[] GetAllAsync();
        void InitCache(List<IAuctionLot> lots);
        void Add(IAuctionLot lot);
    }
}
