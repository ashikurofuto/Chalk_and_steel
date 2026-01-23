using ChalkAndSteel.Services;

public class PlayerService : IPlayerService
{
    public PlayerStage GetCurrentStage() => PlayerStage.KID;
    public int GetDeathCount() => 0;
    public int GetKillCount() => 0;
}