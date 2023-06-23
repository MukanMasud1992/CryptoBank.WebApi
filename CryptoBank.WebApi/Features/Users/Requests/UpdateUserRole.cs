﻿using CryptoBank.WebApi.Database;
using CryptoBank.WebApi.Features.Users.Domain;
using CryptoBank.WebApi.Features.Users.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CryptoBank.WebApi.Features.Users.Requests;

public static class UpdateUserRole
{
    public record Request(string Email, UserRole UpdateRole) : IRequest<Response>;

    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(ApplicationDbContext applicationDbContext)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email empty")
                .EmailAddress()
                .WithMessage("Email format not correct");

            RuleFor(x => x.Email)
              .Cascade(CascadeMode.Stop)
              .MustAsync(async (x, token) =>
              {
                  var userExists = await applicationDbContext.Users.AnyAsync(user => user.Email == x.ToLower(), token);

                  return userExists;
              }).WithMessage("User not exist");

            RuleFor(x => x.UpdateRole)
             .Cascade(CascadeMode.Stop)
             .IsInEnum()
             .WithMessage("Invalid role name, role not found");
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
            var user = await _applicationDbContext.Users
                .Include(u => u.Roles)
                .SingleOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

            var role = user.Roles.SingleOrDefault(r => r.Name == request.UpdateRole);

            if (role != null)
            {
                throw new Exception();
            }

            var newRole = new Role
            {
                UserId = user.Id,
                Name = request.UpdateRole,
                CreatedAt = DateTime.Now.ToUniversalTime()
            };

            user.Roles.Add(newRole);

            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return new Response(new UserModel()
            {
                Id = user.Id,
                Email = user.Email,
                DateOfBirth = user.BirthDate,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles.Select(role => new RoleModel
                {
                    RoleName = role.Name.ToString(),
                    CreatedAt = role.CreatedAt
                }).ToList()
            });
        }
    }
}
