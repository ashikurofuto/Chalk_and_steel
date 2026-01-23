using System.Collections.Generic;

/// <summary>
/// Интерфейс сервиса генерации комнат
/// </summary>
public interface IRoomGeneratorService
{
    /// <summary>
    /// Генерирует комнаты
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    void GenerateRooms(RoomView prefab, int maxRooms);
    
    /// <summary>
    /// Инициализирует генерацию комнат
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    void InitializeRooms(RoomView prefab, int maxRooms);
    
    /// <summary>
    /// Генерирует новую комнату в указанном направлении от текущей комнаты
    /// </summary>
    /// <param name="direction">Направление для генерации новой комнаты</param>
    /// <returns>Новая комната или null, если генерация не удалась</returns>
    RoomNode GenerateRoomInDirection(DoorDirection direction);
    
    /// <summary>
    /// Генерирует комнаты по требованию - создает недостающие комнаты для текущей
    /// </summary>
    void GenerateRoomsIfNeeded();
    
    /// <summary>
    /// Устанавливает фабрику для создания экземпляров комнат
    /// </summary>
    /// <param name="factory">Фабрика для создания экземпляров комнат</param>
    void SetRoomFactory(System.Func<RoomView, RoomView> factory);
    
    /// <summary>
    /// Генерирует все комнаты сразу
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    void GenerateAllRoomsAtOnce(RoomView prefab, int maxRooms);
    
    /// <summary>
    /// Получает все сгенерированные комнаты
    /// </summary>
    /// <returns>Список всех сгенерированных комнат</returns>
    List<RoomNode> GetAllRooms();
    
    /// <summary>
    /// Получает текущую комнату из двусвязного списка
    /// </summary>
    /// <returns>Текущая комната</returns>
    RoomNode GetCurrentRoom();
    
    /// <summary>
    /// Получает всех соседей текущей комнаты
    /// </summary>
    /// <returns>Список соседей текущей комнаты</returns>
    List<RoomNode> GetCurrentRoomNeighbors();
    
    /// <summary>
    /// Получает общее количество сгенерированных комнат
    /// </summary>
    /// <returns>Количество комнат</returns>
    int GetTotalRoomCount();
    
    /// <summary>
    /// Устанавливает комнату как текущую
    /// </summary>
    /// <param name="room">Комната, которую нужно установить как текущую</param>
    void SetCurrentRoom(RoomNode room);
    
    /// <summary>
    /// Переходит к следующей комнате в списке
    /// </summary>
    /// <returns>Следующая комната или null, если следующей комнаты нет</returns>
    RoomNode MoveToNextRoom();
    
    /// <summary>
    /// Переходит к предыдущей комнате в списке
    /// </summary>
    /// <returns>Предыдущая комната или null, если предыдущей комнаты нет</returns>
    RoomNode MoveToPreviousRoom();
}