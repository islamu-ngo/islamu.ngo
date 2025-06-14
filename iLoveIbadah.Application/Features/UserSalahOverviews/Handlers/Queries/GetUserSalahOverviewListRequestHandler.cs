﻿using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLoveIbadah.Application.DTOs.UserSalahOverview;
using iLoveIbadah.Application.Contracts.Persistence;
using iLoveIbadah.Application.Features.UserSalahOverviews.Requests.Queries;

namespace iLoveIbadah.Application.Features.UserSalahOverviews.Handlers.Queries
{
    public class GetUserSalahOverviewListRequestHandler : IRequestHandler<GetUserSalahOverviewListRequest, List<UserSalahOverviewListDto>>
    {
        private readonly IUserSalahOverviewRepository _userSalahOverviewRepository;
        private readonly IMapper _mapper;
        public GetUserSalahOverviewListRequestHandler(IUserSalahOverviewRepository userSalahOverviewRepository, IMapper mapper)
        {
            _userSalahOverviewRepository = userSalahOverviewRepository;
            _mapper = mapper;
        }
        public async Task<List<UserSalahOverviewListDto>> Handle(GetUserSalahOverviewListRequest request, CancellationToken cancellationToken)
        {
            var userSalahOverviews = await _userSalahOverviewRepository.GetAll();
            return _mapper.Map<List<UserSalahOverviewListDto>>(userSalahOverviews);
        }
    }
}
