﻿using iLoveIbadah.Application.Contracts.Persistence;
using iLoveIbadah.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLoveIbadah.Persistence
{
    public static class PersistenceServicesRegistration
    {
        public static IServiceCollection CongfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<iLoveIbadahDbContext>(options =>
            {
                //options.UseSqlServer(configuration.GetConnectionString("localConnectionString")); // When Running Locally localdb
                //options.UseSqlServer(configuration.GetConnectionString("azuresqlserverconnectionstring")); // When Running Locally azuresqlserver db
                options.UseSqlServer(configuration.GetSection("ConnectionStrings:azuresqlconnectionstring").Value); // When Deployed to Azure
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<IBlogLikeRepository, BlogLikeRepository>();
            services.AddScoped<IBlogCategoryMappingRepository, BlogCategoryMappingRepository>();
            services.AddScoped<IBlogTagMappingRepository, BlogTagMappingRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IProfilePictureTypeRepository, ProfilePictureTypeRepository>();
            services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
            services.AddScoped<IRoleTypeRepository, RoleTypeRepository>();
            services.AddScoped<IBlobFileRepository, BlobFileRepository>();
            services.AddScoped<IUserAccountRoleTypeMappingRepository, UserAccountRoleTypeMappingRepository>();
            services.AddScoped<IRoleTypePermissionTypeMappingRepository, RoleTypePermissionTypeMappingRepository>();

            return services;
        }
    }
}
