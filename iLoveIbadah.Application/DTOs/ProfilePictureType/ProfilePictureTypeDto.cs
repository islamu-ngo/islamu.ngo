﻿using iLoveIbadah.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.DTOs.ProfilePictureType
{
    public class ProfilePictureTypeDto
    {
        public int Id { get; set; }
        public int BlobFileId { get; set; }
        public int CreatedBy { get; set; }
    }
}
