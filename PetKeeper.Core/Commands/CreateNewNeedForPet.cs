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
    public CreateNewNeedForPetHandler(IPetRepository petRepository)
    {
        PetRepository = petRepository;
    }

    public IPetRepository PetRepository { get; }

    public Task<Result<Need>> Handle(CreateNewNeedForPet request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            PetRepository
             .GetPet(request.PetId)
             .Match(
                 Some: p =>
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

                     return PetRepository
                         .UpdatePet(p)
                         .Match(
                             Succ: _ => need,
                             Fail: e => new Result<Need>(e));
                 },
                 None: new Result<Need>(new PetNotFoundException())));
    }            
}
