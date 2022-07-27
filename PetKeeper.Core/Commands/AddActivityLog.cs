using LanguageExt.Common;
using MediatR;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Commands;

public record AddActivityLog : IRequest<Result<Activity>>
{
    public string PetId { get; init; } = string.Empty;
    public string? NeedId { get; init; }
    public DateTime When { get; init; } = DateTime.Now;
    public string Notes { get; init; } = string.Empty;
}

public class AddActivityLogHandler : IRequestHandler<AddActivityLog, Result<Activity>>
{
    public AddActivityLogHandler(IReadPets petReader, IActivityLogRepository activityLogRepository)
    {
        PetReader = petReader;
        ActivityLogRepository = activityLogRepository;
    }

    public IReadPets PetReader { get; }
    public IActivityLogRepository ActivityLogRepository { get; }

    public async Task<Result<Activity>> Handle(AddActivityLog request, CancellationToken cancellationToken) =>
        (await PetReader
            .GetPet(request.PetId))
            .Match(
                Some: _ => ActivityLogRepository.AddActivityLog(new Activity
                {
                    PetId = request.PetId,
                    NeedId = request.NeedId,
                    When = DateTime.Now,
                    Notes = request.Notes
                }),
                None: new Result<Activity>(new PetNotFoundException()));
}
