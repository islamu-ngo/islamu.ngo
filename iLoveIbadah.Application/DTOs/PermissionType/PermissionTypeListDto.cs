﻿using iLoveIbadah.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.DTOs.PermissionType
{
    public class PermissionTypeListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Details { get; set; }
    }
}
