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
    public GetActivitiesByPetHandler(IReadPets petReader, IActivityLogRepository activityLogRepository)
    {
        PetReader = petReader;
        ActivityLogRepository = activityLogRepository;
    }

    public IReadPets PetReader { get; }
    public IActivityLogRepository ActivityLogRepository { get; }

    public async Task<Option<List<Activity>>> Handle(GetActivitiesByPet request, CancellationToken cancellationToken) =>
        (await PetReader
            .GetPet(request.PetId))
            .Match(
                Some: p => ActivityLogRepository.GetAllActivitiesForPet(p.Id),
                None: Option<List<Activity>>.None);
}
