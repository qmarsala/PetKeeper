using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetAllActivities : IRequest<Option<List<Activity>>>
{
}

public class GetAllActivitiesHandler : IRequestHandler<GetAllActivities, Option<List<Activity>>>
{
    public GetAllActivitiesHandler(IActivityLogRepository activityLogRepository)
    {
        ActivityLogRepository = activityLogRepository;
    }

    public IActivityLogRepository ActivityLogRepository { get; }

    public Task<Option<List<Activity>>> Handle(GetAllActivities request, CancellationToken cancellationToken)
    {
        return Task.FromResult(ActivityLogRepository.GetAllActivities());
    }
}
