using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AverageBid.Models
{
    public class InboundMessage
    {
        public int Id { get; set; }

        public string Message { get; set; }

        [StringLength(20)]
        public string Sender { get; set; }

        [StringLength(50)]
        public string ServiceId { get; set; }

        public string LinkId { get; set; }

        [StringLength(100)]
        public string TraceUniqueId { get; set; }

        [StringLength(50)]
        public string Correlator { get; set; }

        [StringLength(6)]
        public string ShortCode { get; set; }

        public DateTime Timestamp { get; set; }

        public string GetResponse(ApplicationDbContext db)
        {
            int bid = 0;
            int minimum = 1;
            int maximum = 1000000;
            // Take the first word of message as bid value and try to parse it
            if (Int32.TryParse(this.Message.Split(' ').First(), out bid))
            {
                if (bid < minimum || bid > maximum)
                {
                    return "Sorry. You can only bid a whole number between " + minimum.ToString()
                        + " and " + maximum.ToString() + ".";
                }
                else
                {
                    var newBid = new Bid
                    {
                        Bidder = this.Sender,
                        Value = bid,
                        Timestamp = DateTime.Now
                    };

                    db.Bids.Add(newBid);
                    db.SaveChanges();

                    int deviation = Math.Abs(Bid.GetArithmeticAverage(db) - bid);
                    return "GREAT JOB! You bid " + bid.ToString() + ". You are " + deviation.ToString()
                        + " points away from the average.";
                }
            }
            // Else could not parse bid, return error message
            return "Please start your message with the bid";
        }
    }
}