namespace Cinema.Domain.Entities;

// Film-oyuncu iliskisi (many-to-many join tablo).
public class MovieActor
{
    // Composite key parcasi 1: hangi filme baglandigi
    public int MovieId { get; set; }
    // Navigation property: bu join kaydinin bagli oldugu film
    public Movie? Movie { get; set; }

    // Composite key parcasi 2: hangi oyuncuya baglandigi
    public int ActorId { get; set; }
    // Navigation property: bu join kaydinin bagli oldugu oyuncu
    public Actor? Actor { get; set; }

    // Iliskiye ait ek alan ornegi:
    // Bu bilgi ne filme ne de oyuncuya ait; "oyuncunun filmdeki rolune" aittir.
    public string? CharacterName { get; set; }
}
