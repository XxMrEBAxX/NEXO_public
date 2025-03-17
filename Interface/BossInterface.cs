namespace BirdCase
{
    public interface IBossGetDamage
    {
        void GetDamage(int damage, PlayerName playerName);
        void GetNeutralize(int neutralize, PlayerName playerName);
        int CurrentGetDamage { get; set; }
    }
}
