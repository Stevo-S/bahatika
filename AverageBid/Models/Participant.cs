using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AverageBid.Models
{
    public class Participant
    {
        public int Id { get; set; }

        [Index]
        [StringLength(20)]
        public string Phone { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}