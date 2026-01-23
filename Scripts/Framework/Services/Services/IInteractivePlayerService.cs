using UnityEngine;
using ChalkAndSteel.Services;
using Architecture.GlobalModules.Systems;

// Расширяем интерфейс IPlayerService для поддержки системы интерактивных объектов
namespace ChalkAndSteel.Services
{
    public interface IInteractivePlayerService : IPlayerService
    {
        int CurrentHardeningStage { get; set; }
        Vector2Int GetGridPosition();
        PlayerReceiver GetPlayerReceiver();
    }
}