using LanguageExt;

namespace PetKeeper.Core.Interfaces;

public interface IReadActivityLogs
{
    Task<ActivityLog> GetAllActivities();
    Task<Option<ActivityLog>> GetAllActivitiesForPet(string petId);
}
