using LanguageExt;
using MediatR;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Queries;

public record GetActivitiesByPet : IRequest<Option<ActivityLog>>
{
    public string PetId { get; init; }
}

public class GetActivitiesByPetHandler : IRequestHandler<GetActivitiesByPet, Option<ActivityLog>>
{
    public GetActivitiesByPetHandler(IReadPets petReader, IReadActivityLogs activityLogReader)
    {
        PetReader = petReader;
        ActivityLogReader = activityLogReader;
    }

    public IReadPets PetReader { get; }
    public IReadActivityLogs ActivityLogReader { get; }

    public async Task<Option<ActivityLog>> Handle(GetActivitiesByPet request, CancellationToken cancellationToken)
    {
        var maybePet = await PetReader.GetPet(request.PetId);
        return await maybePet
            .MatchAsync(
                Some: async p => await ActivityLogReader.GetAllActivitiesForPet(p.Id),
                None: () => Option<ActivityLog>.None);
    }
}
