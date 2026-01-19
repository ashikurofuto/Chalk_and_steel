/// <summary>
/// Расширенная конфигурация генерации с параметрами дверей.
/// </summary>
public record DungeonGenerationConfig
{
    public int RoomsCount { get; set; } = 5;
    public int RoomSize { get; set; } = 11;
    public float BossRoomChance { get; set; } = 0.1f;
    public float TreasureRoomChance { get; set; } = 0.15f;
    public float SpecialRoomChance { get; set; } = 0.05f;

    public int MinDoorsStart { get; set; } = 1;
    public int MaxDoorsStart { get; set; } = 2;
    public int MinDoorsNormal { get; set; } = 1;
    public int MaxDoorsNormal { get; set; } = 3;
    public int AdditionalDoorChance { get; set; } = 30;
    public int MaxAdditionalDoors { get; set; } = 2;
}