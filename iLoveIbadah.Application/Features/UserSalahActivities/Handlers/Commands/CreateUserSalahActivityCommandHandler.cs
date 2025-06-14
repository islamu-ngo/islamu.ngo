﻿using AutoMapper;
using iLoveIbadah.Application.DTOs.UserDhikrActivity.Validators;
using iLoveIbadah.Application.DTOs.UserSalahActivity.Validators;
using iLoveIbadah.Application.Exceptions;
using iLoveIbadah.Application.Features.UserDhikrActivities.Requests.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLoveIbadah.Application.Contracts.Persistence;
using iLoveIbadah.Application.Features.UserSalahActivities.Requests.Commands;
using iLoveIbadah.Application.Responses;
using iLoveIbadah.Domain;

namespace iLoveIbadah.Application.Features.UserSalahActivities.Handlers.Commands
{
    public class CreateUserSalahActivityCommandHandler : IRequestHandler<CreateUserSalahActivityCommand, BaseCommandResponse>
    {
        private readonly IUserSalahActivityRepository _userSalahActivityRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ISalahTypeRepository _salahTypeRepository;
        private readonly IMapper _mapper;

        public CreateUserSalahActivityCommandHandler(IUserSalahActivityRepository userSalahActivityRepository, IUserAccountRepository userAccountRepository, ISalahTypeRepository salahTypeRepository, IMapper mapper)
        {
            _userSalahActivityRepository = userSalahActivityRepository;
            _userAccountRepository = userAccountRepository;
            _salahTypeRepository = salahTypeRepository;
            _mapper = mapper;
        }

        public async Task<BaseCommandResponse> Handle(CreateUserSalahActivityCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseCommandResponse();
            var validator = new CreateUserSalahActivityDtoValidator(_userSalahActivityRepository, _userAccountRepository, _salahTypeRepository); //need to give the 3 repositories as parameter as in dto
            var validationResult = await validator.ValidateAsync(request.UserSalahActivityDto);

            //var validationResult = await validator.ValidateAsync(new CreateUserAccountDto
            //{
            //    // Copy properties from UserAccountDto to CreateUserAccountDto
            //    Property1 = request.UserAccountDto.Property1,
            //    Property2 = request.UserAccountDto.Property2,
            //    // Add more properties as needed
            //});   

            if (!validationResult.IsValid)
            {
                response.Success = false;
                response.Message = "Creation Failed";
                response.Errors = validationResult.Errors.Select(q => q.ErrorMessage).ToList();
                //throw new ValidationException(validationResult);
            }

            var userSalahActivity = _mapper.Map<UserSalahActivity>(request.UserSalahActivityDto);
            userSalahActivity = await _userSalahActivityRepository.Create(userSalahActivity);

            response.Success = true;
            response.Message = "Creation Successful";
            response.Id = userSalahActivity.Id;
            return response;
        }
    }
}
