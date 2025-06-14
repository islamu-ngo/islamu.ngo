﻿using iLoveIbadah.Application.DTOs.UserSalahActivity;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.Features.UserSalahActivities.Requests.Queries
{
    public class GetUserSalahActivityDetailsRequest : IRequest<UserSalahActivityDto>
    {
        public int Id { get; set; }
        public int UserAccountId { get; set; }
        public int SalahTypeId { get; set; }
        [DataType(DataType.Date)]
        public DateTime TrackedOn { get; set; }
        public decimal PunctualityPercentage { get; set; }
    }
}
