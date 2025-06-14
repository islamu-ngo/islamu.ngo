﻿using iLoveIbadah.Application.Contracts.Persistence;
using iLoveIbadah.Domain;
using iLoveIbadah.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Persistence.Repositories
{
    public class UserDhikrActivityRepository : GenericRepository<UserDhikrActivity>, IUserDhikrActivityRepository
    {
        private readonly iLoveIbadahDbContext _dbContext;
        public UserDhikrActivityRepository(iLoveIbadahDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<UserDhikrActivity>> GetUserDhikrActivitiesWithDetails()
        {
            var userDhikrActivities = await _dbContext.UserDhikrActivities
                .Include(q => q.UserAccount)
                .Include(q => q.DhikrType)
                .ToListAsync();
            return userDhikrActivities;
        }

        public async Task<UserDhikrActivity> GetUserDhikrActivityWithDetails(int id)
        {
            var userDhikrActivity = await _dbContext.UserDhikrActivities
                .Include(q => q.UserAccount)
                .Include(q => q.DhikrType)
                .FirstOrDefaultAsync(q => q.Id == id);
            return userDhikrActivity;
        }

        public async Task<UserDhikrActivity> GetUserDhikrActivityByPerformedOn(int userAccountId, DateTime performedOn, int dhikrTypeId)
        {
            var userDhikrActivity = await _dbContext.UserDhikrActivities
                .Include(q => q.UserAccount)
                .Include(q => q.DhikrType)
                .FirstOrDefaultAsync(q => q.UserAccountId == userAccountId && q.DhikrTypeId == dhikrTypeId && q.PerformedOn == performedOn);
            return userDhikrActivity;
        }

        public async Task<bool> PerformedOnExists(int userAccountId, DateTime performedOn, int dhikrTypeId)
        {
            return await _dbContext.UserDhikrActivities
                .AnyAsync(q => q.UserAccountId == userAccountId
                && q.PerformedOn == performedOn
                && q.DhikrTypeId == dhikrTypeId);
            //var userDhikrActivity = await _dbContext.UserDhikrActivities
            //    .Include(q => q.UserAccount)
            //    .Include(q => q.DhikrType)
            //    .FirstOrDefaultAsync(q => q.UserAccountId == userAccountId && q.PerformedOn == performedOn && q.DhikrTypeId == dhikrTypeId);

            //return userDhikrActivity != null;
        }

        public async Task IncrementTotalPerformed(int userAccountId, DateTime performedOn, int dhikrTypeId)
        {
            var userDhikrActivity = await _dbContext.UserDhikrActivities
                .FirstOrDefaultAsync(q => q.UserAccountId == userAccountId && q.PerformedOn == performedOn && q.DhikrTypeId == dhikrTypeId);
            
            userDhikrActivity.TotalPerformed += 1;
            await _dbContext.SaveChangesAsync();
        }
    }
}
