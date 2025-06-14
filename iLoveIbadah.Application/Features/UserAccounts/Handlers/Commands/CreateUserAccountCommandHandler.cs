﻿using AutoMapper;
using iLoveIbadah.Application.DTOs.UserAccount;
using iLoveIbadah.Application.DTOs.UserAccount.Validators;
using iLoveIbadah.Application.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iLoveIbadah.Application.Models;
using iLoveIbadah.Application.Features.UserAccounts.Requests.Commands;
using iLoveIbadah.Application.Contracts.Persistence;
using iLoveIbadah.Application.Contracts.Infrastructure;
using iLoveIbadah.Application.Responses;
using iLoveIbadah.Domain;

namespace iLoveIbadah.Application.Features.UserAccounts.Handlers.Commands
{
    public class CreateUserAccountCommandHandler : IRequestHandler<CreateUserAccountCommand, BaseCommandResponse>
    {
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public CreateUserAccountCommandHandler(IUserAccountRepository userAccountRepository, IEmailSender emailSender, IMapper mapper)
        {
            _userAccountRepository = userAccountRepository;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        public async Task<BaseCommandResponse> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseCommandResponse();
            var validator = new CreateUserAccountDtoValidator();
            var validationResult = await validator.ValidateAsync(request.UserAccountDto);

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

            var userAccount = _mapper.Map<UserAccount>(request.UserAccountDto);
            userAccount = await _userAccountRepository.Create(userAccount);

            response.Success = true;
            response.Message = "Creation Successful";
            response.Id = userAccount.Id;

            string emailVerificationToken = "";
            var email = new Email
            {
                To = userAccount.Email,
                Subject = "Activate Your Account [iLoveIbadah]",
                Body = $"Activate Your Account to verify it is you who created it, if it is someone else using your email just ignore this email. Click on following link:{emailVerificationToken}"
            };
            try
            {
                await _emailSender.SendEmail(email);
            }
            catch (Exception ex)
            {
                // Log or handle exception
                response.Success = false;
                response.Message = "Creation Failed";
                response.Errors = new List<string> { "Email sending failed" };
            };

            return response;
        }
    }
}
