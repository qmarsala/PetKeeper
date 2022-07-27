using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetActivitiesByPet : IRequest<Option<List<Activity>>>
{
    public string PetId { get; init; }
}

public class GetActivitiesByPetHandler : IRequestHandler<GetActivitiesByPet, Option<List<Activity>>>
{
    public GetActivitiesByPetHandler(IPetRepository petRepository, IActivityLogRepository activityLogRepository)
    {
        PetRepository = petRepository;
        ActivityLogRepository = activityLogRepository;
    }

    public IPetRepository PetRepository { get; }
    public IActivityLogRepository ActivityLogRepository { get; }

    public Task<Option<List<Activity>>> Handle(GetActivitiesByPet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
             PetRepository
                .GetPet(request.PetId)
                .Match(
                    Some: p => ActivityLogRepository.GetAllActivitiesForPet(p.Id), 
                    None: Option<List<Activity>>.None));
    }
}
