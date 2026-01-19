/// <summary>
/// Конфигурация генерации подземелья с уменьшенными шансами особых комнат
/// </summary>
public record DungeonGenerationConfig
{
    public int RoomsCount { get; set; } = 5;
    public int RoomSize { get; set; } = 11;

    // Уменьшенные шансы по умолчанию
    public float BossRoomChance { get; set; } = 0.02f;      // 2% шанс (было 10%)
    public float TreasureRoomChance { get; set; } = 0.03f;  // 3% шанс (было 15%)
    public float SpecialRoomChance { get; set; } = 0.01f;   // 1% шанс (было 5%)

    // Параметры дверей
    public int MinDoorsStart { get; set; } = 1;
    public int MaxDoorsStart { get; set; } = 2;
    public int MinDoorsNormal { get; set; } = 1;
    public int MaxDoorsNormal { get; set; } = 3;
    public int AdditionalDoorChance { get; set; } = 30;
    public int MaxAdditionalDoors { get; set; } = 2;
}