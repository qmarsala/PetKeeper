using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

//todo:
// look into language-ext more
// there are io monads and ways of dealing with async
public interface IWriteActivityLogs
{
    Task<Result<Activity>> WriteActivityLog(Activity newActivity);
}

public interface IReadActivityLogs
{
    Task<List<Activity>> GetAllActivities();
    Task<Option<List<Activity>>> GetAllActivitiesForPet(string petId);
}

public interface IActivityLogRepository
{
    Option<List<Activity>> GetAllActivities();
    Option<List<Activity>> GetAllActivitiesForPet(string petId);
    Result<Activity> AddActivityLog(Activity newActivity);
}
