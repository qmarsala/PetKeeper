using LanguageExt.Common;
using MediatR;
using PetKeeper.Core.Errors;
using PetKeeper.Core.Interfaces;

namespace PetKeeper.Core.Commands;

public record CreateNewNeedForPet : IRequest<Result<Need>>
{
    public string PetId { get; init; } = string.Empty;
    public string Name { get; init; } = "An unknown need";
    public string Notes { get; init; } = string.Empty;
    public int Times { get; init; } = 1;
    public IEnumerable<DayOfWeek> Days { get; init; } = Enumerable.Empty<DayOfWeek>();
}

public class CreateNewNeedForPetHandler : IRequestHandler<CreateNewNeedForPet, Result<Need>>
{
    public CreateNewNeedForPetHandler(IReadPets petReader, IWritePets petWriter)
    {
        PetReader = petReader;
        PetWriter = petWriter;
    }

    public IReadPets PetReader { get; }
    public IWritePets PetWriter { get; }

    public async Task<Result<Need>> Handle(CreateNewNeedForPet request, CancellationToken cancellationToken)
    {
        //todo: Aff
        var maybePet = await PetReader.GetPet(request.PetId);
        return await maybePet.MatchAsync(
            Some: async p => await AddNeed(p, request),
            None: () => new Result<Need>(new PetNotFoundException()));
    }

    private async Task<Result<Need>> AddNeed(Pet p, CreateNewNeedForPet request)
    {
        var need = new Need
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Notes = request.Notes,
            Times = request.Times,
            Days = request.Days
        };
        p.AddNeed(need);

        var result = await PetWriter.WritePet(p);
        return result.Match(
                    Succ: _ => need,
                    Fail: e => new Result<Need>(e));
    }
}
