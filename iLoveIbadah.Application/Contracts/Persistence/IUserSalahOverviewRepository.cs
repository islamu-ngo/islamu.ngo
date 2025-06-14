﻿using iLoveIbadah.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.Contracts.Persistence
{
    public interface IUserSalahOverviewRepository : IGenericRepository<UserSalahOverview>
    {
        Task<UserSalahOverview> GetUserSalahOverviewWithDetails(int id);
        Task<UserSalahOverview> GetUserSalahOverviewByUserAccountWithDetails(int userAccountId);
        Task<List<UserSalahOverview>> GetUserSalahOverviewsWithDetails();
    }
}
