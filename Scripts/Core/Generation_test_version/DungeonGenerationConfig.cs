/// <summary>
/// Конфигурация генерации подземелья с магазином
/// </summary>
public record DungeonGenerationConfig
{
    public int RoomsCount { get; set; } = 10;
    public int RoomSize { get; set; } = 11;

    // Балансированные шансы по умолчанию
    public float BossRoomChance { get; set; } = 0.00f;      // 0% шанс в середине (только в конце)
    public float TreasureRoomChance { get; set; } = 0.05f;  // 5% шанс сокровищницы
    public float SpecialRoomChance { get; set; } = 0.03f;   // 3% шанс особой комнаты
    public float ShopRoomChance { get; set; } = 0.08f;      // 8% шанс магазина (НОВЫЙ!)

    // Параметры дверей
    public int MinDoorsStart { get; set; } = 1;
    public int MaxDoorsStart { get; set; } = 2;
    public int MinDoorsNormal { get; set; } = 1;
    public int MaxDoorsNormal { get; set; } = 3;
    public int AdditionalDoorChance { get; set; } = 30;
    public int MaxAdditionalDoors { get; set; } = 2;
}