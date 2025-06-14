﻿using iLoveIbadah.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Domain
{
    public class BanType
    {
        public int Id { get; set; }
        public int TotalWarnings { get; set; }
        public int BanDuration { get; set; }
        public bool IsPermanent { get; set; }
    }
}
