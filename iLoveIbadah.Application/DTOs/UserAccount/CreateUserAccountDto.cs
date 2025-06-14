﻿using iLoveIbadah.Application.DTOs.Common;
using iLoveIbadah.Application.DTOs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.DTOs.UserAccount
{
    public class CreateUserAccountDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } //pour l'instant! pas d'autre methode de connection
        //public OAuthProviderType? OAuthProvider { get; set; }
        //public string? OAuthId { get; set; }
    }
}
