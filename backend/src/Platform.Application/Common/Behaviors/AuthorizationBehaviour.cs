using System.Reflection;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Security;
using Platform.Domain.Exceptions;
using Platform.Domain.Results;

namespace Platform.Application.Common.Behaviors;

public sealed class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;

    public AuthorizationBehaviour(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = typeof(TRequest).GetCustomAttributes<AuthorizeAttribute>().ToList();

        if (authorizeAttributes.Count > 0)
        {
            if (_currentUser.UserId is null)
            {
                throw new DomainException(Error.Unauthorized("auth.unauthorized", "User is not authenticated."));
            }

            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)).ToList();

            if (authorizeAttributesWithRoles.Count > 0)
            {
                var authorized = false;
                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    throw new DomainException(Error.Forbidden("auth.forbidden", "User is not authorized to execute this request."));
                }
            }
        }

        return await next();
    }
}
