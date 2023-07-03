﻿using CryptoBank.WebApi.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using static CryptoBank.WebApi.Features.Accounts.Errors.AccountValidationErrors;

namespace CryptoBank.WebApi.Features.Accounts.Requests;

public static class GetAccountsByPeriod
{
    public record Request(DateOnly Start, DateOnly End) : IRequest<Response>;

    public record Response(Dictionary<DateOnly, int> Result);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Start)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(StartPeriod);

            RuleFor(x => x.End)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(EndPeriod);

            RuleFor(x => new { x.Start, x.End })
                .Cascade(CascadeMode.Stop)
                .Must((pair, cancellationToken) => IsValidRange(pair.Start, pair.End))
                .WithErrorCode(InvalidRange);
        }

        private static bool IsValidRange(DateOnly start, DateOnly end)
        {
            return start < end;
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RequestHandler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var beginDateTime = request.Start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endDateTime = request.End.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var resullt = await _applicationDbContext.Accounts
                .Select(account => new { account.Number, account.CreatedAt })
                .Where(account => account.CreatedAt >= beginDateTime && account.CreatedAt <= endDateTime)
                .GroupBy(account => account.CreatedAt.Date)
                .Select(group => new { Date = DateOnly.FromDateTime(group.Key), Count = group.Count() })
                .OrderBy(record => record.Date)
                .ToDictionaryAsync(record => record.Date, record => record.Count, cancellationToken);

            return new Response(resullt);
        }
    }
}
