using LanguageExt;
using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces
{
    public interface IActivityLogReposity
    {
        Option<List<Activity>> GetAllActivities();
        Option<List<Activity>> GetAllActivitiesForPet(string petId);
        Result<Activity> AddActivityLog(Activity activity);
    }
}
