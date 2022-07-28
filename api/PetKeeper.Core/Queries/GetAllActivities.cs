using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetAllActivities : IRequest<Option<ActivityLog>>
{
}

public class GetAllActivitiesHandler : IRequestHandler<GetAllActivities, Option<ActivityLog>>
{
    public GetAllActivitiesHandler(IReadActivityLogs activityLogReader)
    {
        ActivityLogReader = activityLogReader;
    }
    public IReadActivityLogs ActivityLogReader { get; }

    public async Task<Option<ActivityLog>> Handle(GetAllActivities request, CancellationToken cancellationToken)
         => await ActivityLogReader.GetAllActivities();
}
