﻿using iLoveIbadah.Application.DTOs.PermissionType;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.Features.PermissionTypes.Requests.Queries
{
    public class GetPermissionTypeDetailsRequest : IRequest<PermissionTypeDto>
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Details { get; set; }
        //public DateTime CreatedOn { get; set; }
        //public int CreatedBy { get; set; }
        //public DateTime LastModifiedOn { get; set; }
        //public int LastModifiedBy { get; set; }
    }
}
