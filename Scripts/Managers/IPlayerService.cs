

// Services/IPlayerService.cs
namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Интерфейс сервиса игрока.
    /// </summary>
    public interface IPlayerService
    {
        PlayerStage GetCurrentStage();
        int GetDeathCount();
        int GetKillCount();
    }
}
