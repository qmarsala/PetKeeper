
// this `What` would be a `Need` so we can know it is not a current need once completed.
public record Activity(string What, DateTime When, string Notes)
{
    public static Activity New(string what, DateTime? when, string? notes)
      => (what, when, notes) switch
      {
          (string w, DateTime t, string n) => new Activity(w, t, n),
          (string w, DateTime t, null) => new Activity(w, t, string.Empty),
          (string w, null, string n) => new Activity(w, DateTime.Now, n),
          (string w, null, null) => new Activity(w, DateTime.Now, string.Empty),
          (null, _, _) => new Activity("Unknown", DateTime.Now, string.Empty),
      };
}
