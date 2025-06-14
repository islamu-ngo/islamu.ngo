﻿using iLoveIbadah.Application.DTOs.BlobFile;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Application.Features.BlobFiles.Requests.Queries
{
    public class GetBlobFileListRequest : IRequest<List<BlobFileListDto>>
    {
        public string Uri { get; set; }
        public string FullName { get; set; }
        public string Extension { get; set; }
        public int? Size { get; set; }
        public int CreatedBy { get; set; }
    }
}
