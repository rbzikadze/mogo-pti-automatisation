﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MogoMyPti.Entities
{
    public class ScrapeResult
    {
        public string License { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastDate { get; set; }
    }
}
