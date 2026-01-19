using Architecture.GlobalModules;
using ChalkAndSteel.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ChalkAndSteel.Handlers
{
    /// <summary>
    /// UI обработчик для тестирования генерации подземелья.
    /// Содержит кнопки для запуска генерации и очистки.
    /// </summary>
    public class DungeonTestUIHandler : MonoBehaviour
    {
        // === ЗАВИСИМОСТИ (инжектятся) ===
        private IEventBus _eventBus;
        private DungeonGenerationService _dungeonGenerationService;

        // === ССЫЛКИ НА СЦЕНУ (настраиваются в инспекторе) ===
        [SerializeField] private Transform _generationOrigin;
        [SerializeField] private int _roomsCount = 5;

        [Header("UI References")]
        [SerializeField] private Button _generateButton;
        [SerializeField] private Button _clearButton;
       

        // === ИНЖЕКЦИЯ ===
        [Inject]
        private void Construct(IEventBus eventBus, DungeonGenerationService dungeonGenerationService)
        {
            _eventBus = eventBus;
            _dungeonGenerationService = dungeonGenerationService;

            // Подписываемся на события
        }

        // === ИНИЦИАЛИЗАЦИЯ ===
        private void Start()
        {

            // Настраиваем обработчики кнопок
            _generateButton.onClick.AddListener(OnGenerateButtonClicked);
            _clearButton.onClick.AddListener(OnClearButtonClicked);
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
   

            // Очищаем подписки на кнопки
            _generateButton?.onClick.RemoveListener(OnGenerateButtonClicked);
            _clearButton?.onClick.RemoveListener(OnClearButtonClicked);


        }

        // === ОБРАБОТЧИКИ КНОПОК ===
        private void OnGenerateButtonClicked()
        {
            Debug.Log($"Generating dungeon with {_roomsCount} rooms");

            _dungeonGenerationService.GenerateDungeon(_roomsCount, _generationOrigin);
            _generateButton.interactable = false;
        }

        private void OnClearButtonClicked()
        {
            Debug.Log("Clearing dungeon");

            // Вызываем очистку напрямую (или можно через событие)
            _dungeonGenerationService.ClearDungeon();
            _generateButton.interactable = true;
        }

     

     
        private void HighlightRooms(IReadOnlyList<RoomData> rooms)
        {
            // Простая подсветка комнат для визуализации
            foreach (var room in rooms)
            {
                if (room.RoomObject != null)
                {
                    var spriteRenderer = room.RoomObject.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        // Подсвечиваем комнаты разными цветами в зависимости от типа
                        switch (room.RoomType)
                        {
                            case RoomType.StartRoom:
                                spriteRenderer.color = Color.green;
                                break;
                            case RoomType.BossRoom:
                                spriteRenderer.color = Color.red;
                                break;
                            default:
                                spriteRenderer.color = Color.white;
                                break;
                        }
                    }
                }
            }
        }


        // === GIZMOS ДЛЯ ОТЛАДКИ ===
        private void OnDrawGizmos()
        {
            if (_generationOrigin == null) return;

            // Рисуем точку генерации
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_generationOrigin.position, 0.5f);
            Gizmos.DrawWireCube(_generationOrigin.position, new Vector3(11, 11, 0));

            // Подпись
#if UNITY_EDITOR
            UnityEditor.Handles.Label(_generationOrigin.position + Vector3.up * 2,
                $"Rooms: {_roomsCount}\nGrid Size: 11x11");
#endif
        }
    }
}