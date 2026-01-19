using Architecture.GlobalModules;
using ChalkAndSteel.Services;
using UnityEngine;
using VContainer;

namespace ChalkAndSteel.Handlers
{
    /// <summary>
    /// Обработчик генерации подземелья.
    /// Связывает событие запроса генерации с сервисом генерации.
    /// </summary>
    public class DungeonGenerationHandler : MonoBehaviour
    {
        // === ЗАВИСИМОСТИ (инжектятся) ===
        private IEventBus _eventBus;
        private IDungeonGenerationService _dungeonGenerationService;

        // === ССЫЛКИ НА СЦЕНУ (настраиваются в инспекторе) ===
        [SerializeField] private Transform _generationOrigin;
        [SerializeField] private int _defaultRoomsCount = 5;
        [SerializeField] private GameObject prefabRoom;

        // === ИНЖЕКЦИЯ ===
        [Inject]
        private void Construct(IEventBus eventBus, IDungeonGenerationService dungeonGenerationService)
        {
            _eventBus = eventBus;
            _dungeonGenerationService = dungeonGenerationService;
        }

        // === ПОДПИСКА/ОТПИСКА НА EVENTBUS ===
        private void OnEnable()
        {
            _eventBus.Subscribe<DungeonGenerationRequestedEvent>(OnDungeonGenerationRequested);
            _dungeonGenerationService.Initialize(prefabRoom);
        }
        private void OnDisable() => _eventBus.Unsubscribe<DungeonGenerationRequestedEvent>(OnDungeonGenerationRequested);

        // === ОБРАБОТЧИК СОБЫТИЯ ГЕНЕРАЦИИ ===
        private void OnDungeonGenerationRequested(DungeonGenerationRequestedEvent e)
        {
            // Используем параметры из события или значения по умолчанию
            int roomsCount = e.RoomsCount > 0 ? e.RoomsCount : _defaultRoomsCount;
            Transform origin = e.GenerationOrigin ?? _generationOrigin;

            // Запускаем генерацию
            _dungeonGenerationService.GenerateDungeon(roomsCount, origin);
        }

        public void Generate()
        {
            _dungeonGenerationService.GenerateDungeon(_defaultRoomsCount, _generationOrigin);
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===
        private void OnDrawGizmosSelected()
        {
            if (_generationOrigin == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_generationOrigin.position, 1f);
            Gizmos.DrawIcon(_generationOrigin.position + Vector3.up, "dungeon_icon");
        }
    }
}

