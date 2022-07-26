using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces
{
    public interface IActivityLogRepository
    {
        Option<List<Activity>> GetAllActivities();
        Option<List<Activity>> GetAllActivitiesForPet(string petId);
        Result<Activity> AddActivityLog(Activity newActivity);
    }
}
