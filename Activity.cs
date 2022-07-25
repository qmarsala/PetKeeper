
public record Activity(Need What, DateTime When, string Notes)
{
    public static Activity New(Need what, DateTime? when, string? notes)
        => (what, when, notes) switch
        {
            (Need w, DateTime t, string n) => new Activity(w, t, n),
            (Need w, DateTime t, null) => new Activity(w, t, string.Empty),
            (Need w, null, string n) => new Activity(w, DateTime.Now, n),
            (Need w, null, null) => new Activity(w, DateTime.Now, string.Empty)
        };
}
