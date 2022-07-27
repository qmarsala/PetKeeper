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
    public AddActivityLogHandler(IReadPets petReader, IWriteActivityLogs activityLogWriter)
    {
        PetReader = petReader;
        ActivityLogWriter = activityLogWriter;
    }

    public IReadPets PetReader { get; }
    public IWriteActivityLogs ActivityLogWriter { get; }

    public async Task<Result<Activity>> Handle(AddActivityLog request, CancellationToken cancellationToken)
    {
        var maybePet = await PetReader
            .GetPet(request.PetId);
        return await maybePet
            .MatchAsync(
                Some: async _ => await ActivityLogWriter.WriteActivityLog(new Activity
                {
                    PetId = request.PetId,
                    NeedId = request.NeedId,
                    When = DateTime.Now,
                    Notes = request.Notes
                }),
                None: () => new Result<Activity>(new PetNotFoundException()));
    }

}
