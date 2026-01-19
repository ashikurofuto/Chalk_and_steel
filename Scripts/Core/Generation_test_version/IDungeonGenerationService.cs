using ChalkAndSteel.Services;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Интерфейс сервиса генерации подземелья.
/// Определяет контракт для создания и очистки процедурных комнат.
/// Все взаимодействие через методы интерфейса, без подписки на события внутри сервиса.
/// </summary>
public interface IDungeonGenerationService
{
    /// <summary>
    /// Генерирует подземелье с указанной конфигурацией.
    /// </summary>
    /// <param name="config">Конфигурация генерации</param>
    /// <param name="generationOrigin">Точка отсчета для генерации</param>
    /// <returns>Список данных созданных комнат</returns>
    IReadOnlyList<RoomData> GenerateDungeon(DungeonGenerationConfig config, Transform generationOrigin);
    void Initialize(GameObject prefab);
    /// <summary>
    /// Генерирует подземелье с параметрами по умолчанию.
    /// </summary>
    /// <param name="roomsCount">Количество комнат для генерации</param>
    /// <param name="generationOrigin">Точка отсчета для генерации</param>
    /// <returns>Список данных созданных комнат</returns>
    IReadOnlyList<RoomData> GenerateDungeon(int roomsCount, Transform generationOrigin);

    /// <summary>
    /// Очищает все сгенерированные комнаты.
    /// </summary>
    void ClearDungeon();

    /// <summary>
    /// Возвращает текущее сгенерированное подземелье.
    /// </summary>
    IReadOnlyList<RoomData> GetCurrentDungeon();

    /// <summary>
    /// Пересоздает подземелье с теми же параметрами.
    /// </summary>
    IReadOnlyList<RoomData> RegenerateDungeon();

    /// <summary>
    /// Получает комнату по ее координатам в сетке.
    /// </summary>
    /// <param name="gridPosition">Координаты в сетке</param>
    /// <returns>Данные комнаты или null если комната не найдена</returns>
    RoomData GetRoomAtPosition(Vector3Int gridPosition);
}