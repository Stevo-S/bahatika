using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AverageBid.Models
{
    public class Bid
    {
        public int Id { get; set; }

        public int Value { get; set; }

        [StringLength(20)]
        public string Bidder { get; set; }

        public DateTime Timestamp { get; set; }

        public static int GetArithmeticAverage(ApplicationDbContext db)
        {
            return (int)Math.Round(db.Bids.Average(b => b.Value));
        }
    }
}