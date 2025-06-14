﻿using iLoveIbadah.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.Contracts.Identity
{
    public interface IUserService
    {
        Task<UserAccount> UpdateUserAccountPasswordHash(int id, string currentPasswordHash, string newPasswordHash);
        Task<UserAccount> UpdateUserAccountEmailConfirmed(int id);
    }
}
