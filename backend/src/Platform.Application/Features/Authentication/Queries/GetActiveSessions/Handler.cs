using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Authentication.Queries.GetActiveSessions;

public sealed class GetActiveSessionsHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<SessionResponse>>>
{
    private readonly IRepository<UserSession> _sessions;
    private readonly ICurrentUser _current;

    public GetActiveSessionsHandler(IRepository<UserSession> sessions, ICurrentUser current)
    {
        _sessions = sessions;
        _current  = current;
    }

    public async Task<Result<List<SessionResponse>>> Handle(GetActiveSessionsQuery query, CancellationToken ct)
    {
        var sessions  = await _sessions.ListAsync(s => s.UserId == query.UserId, ct);
        var currentIp = _current.IpAddress;

        return sessions.ToSessionResponseList(currentIp);
    }
}
