﻿using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Accounts.Domain;
using CryptoBank.WebApi.Features.Accounts.Models;
using CryptoBank.WebApi.Features.Accounts.Options;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;

namespace CryptoBank.WebApi.Features.Accounts.Requests;

public static class CreateAccount
{
    public record Request(string Number, string Currency, long UserId) : IRequest<Response>;

    public record Response(AccountModel AccountModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.Currency).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Currency required")
                .Matches("BTC")
                .WithMessage("Currency not exist");

            RuleFor(x => x.UserId).Cascade(CascadeMode.Stop)
                .MustAsync(async (x, token) =>
                {
                    var userExists = await applicationDbContext.Users.AnyAsync(user => user.Id == x, token);
                    return userExists;
                }).WithMessage("User not exist");

            RuleFor(x => x.Number).Cascade(CascadeMode.Stop)
                .MustAsync(async (x, token) =>
                {
                    var isAccountExist = await applicationDbContext.Accounts.AnyAsync(account => account.Number == x, token);
                    return !isAccountExist;
                }).WithMessage("Account already exist");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly AccountOptions _accountOptions;

        public RequestHandler(ApplicationDbContext applicationDbContext, IOptions<AccountOptions> accountOptions)
        {
            _applicationDbContext = applicationDbContext;
            _accountOptions = accountOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var accountCount = await _applicationDbContext.Accounts.Where(a => a.UserId == request.UserId).CountAsync(cancellationToken);
            if (accountCount == _accountOptions.MaxAccountsPerUser)
            {
                throw new Exception();
            }

            var account = new Account()
            {
                Number = request.Number,
                Amount = 0,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow.ToUniversalTime(),
                UserId = request.UserId
            };
            await _applicationDbContext.Accounts.AddAsync(account, cancellationToken);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return new Response(new AccountModel()
            {
                Id = account.Id,
                Number = account.Number,
                Amount = account.Amount,
                Currency = account.Currency,
                CreatedAt = account.CreatedAt,
                UserId = account.UserId
            });
        }
    }
}
