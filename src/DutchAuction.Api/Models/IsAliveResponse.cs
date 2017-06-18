using System;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Checks service is alive response
    /// </summary>
    public class IsAliveResponse
    {
        /// <summary>
        /// API version
        /// </summary>
        [Required]
        public string Version { get; set; }
        
        /// <summary>
        /// Environment variables
        /// </summary>
        [Required]
        public string Env { get; set; }

        /// <summary>
        /// Length of auction events to persist in-memory queue
        /// </summary>
        [Required]
        public int AuctionEventsPersistQueueLength { get; set; }

        /// <summary>
        /// Duration of the last orderbook rendering
        /// </summary>
        [Required]
        public TimeSpan LastOrderbookRenderDuration { get; set; }

        /// <summary>
        /// The last orderbook`s bids count
        /// </summary>
        [Required]
        public int LastOrderbookBidsCount { get; set; }
    }
}