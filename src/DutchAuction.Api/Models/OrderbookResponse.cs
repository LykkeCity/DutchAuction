using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    public class OrderbookResponse
    {
        [Required]
        public double Price { get; set; }

        [Required]
        public double InMoneyVolume { get; set; }

        [Required]
        public double OutOfTheMoneyVolume { get; set; }

        [Required]
        public IEnumerable<Order> InMoneyOrders { get; set; }

        [Required]
        public IEnumerable<Order> OutOfTheMoneyOrders { get; set; }

        public class Order
        {
            [Required]
            public double Price { get; set; }

            [Required]
            public double Volume { get; set; }

            [Required]
            public int Investors { get; set; }
        }
    }
}