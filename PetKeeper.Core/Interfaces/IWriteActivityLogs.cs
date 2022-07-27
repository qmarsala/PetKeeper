using LanguageExt.Common;

namespace PetKeeper.Core.Interfaces;

//todo:
// look into language-ext more
// there are io monads and ways of dealing with async
public interface IWriteActivityLogs
{
    Task<Result<Activity>> WriteActivityLog(Activity newActivity);
}
