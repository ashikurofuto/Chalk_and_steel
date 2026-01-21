using UnityEngine;

namespace ChalkAndSteel.Services
{
    [CreateAssetMenu(fileName = "DungeonGeneratorSettings", menuName = "ScriptableObjects/DungeonGeneratorSettings", order = 1)]
    public class DungeonGeneratorSettings : ScriptableObject
    {
        [Header("Конфигурация подземелья")]
        [Tooltip("Количество комнат в подземелье (минимум 3, максимум 25)")]
        [Range(3, 25)]
        public int NumberOfRooms = 9;

        [Header("Структура сетки")]
        [Tooltip("Размер сетки подземелья (количество комнат по одной стороне)")]
        [Range(2, 5)]
        public int GridSize = 3;

        [Header("Вероятности типов комнат")]
        [Tooltip("Вероятность пустых комнат")]
        [Range(0f, 1f)]
        public float EmptyRoomProbability = 0.2f;

        [Tooltip("Вероятность тактических комнат")]
        [Range(0f, 1f)]
        public float TacticalRoomProbability = 0.3f;

        [Tooltip("Вероятность охотничьих комнат")]
        [Range(0f, 1f)]
        public float HuntRoomProbability = 0.3f;

        [Tooltip("Вероятность головоломных комнат")]
        [Range(0f, 1f)]
        public float PuzzleRoomProbability = 0.2f;

        [Header("Настройки соединений")]
        [Tooltip("Минимальное количество соединений у комнаты")]
        [Range(1, 4)]
        public int MinConnections = 1;

        [Tooltip("Максимальное количество соединений у комнаты")]
        [Range(1, 4)]
        public int MaxConnections = 3;

        [Header("Настройки стадии")]
        [Tooltip("Влияние стадии игрока на генерацию")]
        public bool AdjustForPlayerStage = true;
    }
}