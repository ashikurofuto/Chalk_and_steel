using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для работы с UI элементами
using TMPro; // Для работы с TextMeshPro
using Architecture.GlobalModules;
using Architecture.GlobalModules.Handlers;
using VContainer;

/// <summary>
/// Контроллер UI для переходов между комнатами
/// </summary>
public class RoomTransitionUIController : MonoBehaviour
{
    [SerializeField] private Button _topButton;    // Кнопка для перехода вверх
    [SerializeField] private Button _bottomButton; // Кнопка для перехода вниз
    [SerializeField] private Button _leftButton;   // Кнопка для перехода влево
    [SerializeField] private Button _rightButton;  // Кнопка для перехода вправо
    [SerializeField] private RoomGenerationHandler _roomGenerationHandler;
    private IRoomGeneratorService _roomGeneratorService;
    private IEventBus _eventBus;
    
    private TMP_Text _topButtonText;
    private TMP_Text _bottomButtonText;
    private TMP_Text _leftButtonText;
    private TMP_Text _rightButtonText;
    
    [Inject]
    public void Construct(
        IRoomGeneratorService roomGeneratorService,
        IEventBus eventBus)
    {
        _roomGeneratorService = roomGeneratorService;
        _eventBus = eventBus;
    }
    
    private void Start()
    {
        // Находим RoomGenerationHandler на сцене, если он не назначен в инспекторе
        if (_roomGenerationHandler == null)
        {
            _roomGenerationHandler = FindObjectOfType<RoomGenerationHandler>();
        }
        
        if (_roomGenerationHandler == null)
        {
            Debug.LogError("RoomGenerationHandler не найден на сцене");
            return;
        }
        
        // Находим текстовые компоненты для каждой кнопки
        InitializeButtonTexts();
        
        // Назначаем обработчики кликов для каждой кнопки
        AssignButtonClickHandlers();
        
        // Подписываемся на событие генерации комнаты для обновления UI
        _eventBus?.Subscribe<RoomGeneratedEvent>(OnRoomGenerated);
        
        // Обновляем кнопки сразу при старте
        UpdateTransitionButtons();
    }
    
    private void OnDestroy()
    {
        // Отписываемся от события
        _eventBus?.Unsubscribe<RoomGeneratedEvent>(OnRoomGenerated);
    }
    
    /// <summary>
    /// Инициализирует текстовые компоненты для кнопок
    /// </summary>
    private void InitializeButtonTexts()
    {
        if (_topButton != null)
        {
            _topButtonText = _topButton.GetComponentInChildren<TMP_Text>();
            if (_topButtonText == null)
            {
                var textComponent = _topButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    // Пытаемся преобразовать обычный Text в TMP_Text
                    _topButtonText = _topButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (_topButtonText == null)
                    {
                        // Если не найден TMP_Text, добавляем его
                        _topButtonText = _topButton.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                        _topButtonText.text = textComponent.text;
                        Destroy(textComponent);
                    }
                }
            }
            if (_topButtonText != null)
            {
                _topButtonText.text = "Вверх";
            }
        }
        
        if (_bottomButton != null)
        {
            _bottomButtonText = _bottomButton.GetComponentInChildren<TMP_Text>();
            if (_bottomButtonText == null)
            {
                var textComponent = _bottomButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    _bottomButtonText = _bottomButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (_bottomButtonText == null)
                    {
                        _bottomButtonText = _bottomButton.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                        _bottomButtonText.text = textComponent.text;
                        Destroy(textComponent);
                    }
                }
            }
            if (_bottomButtonText != null)
            {
                _bottomButtonText.text = "Вниз";
            }
        }
        
        if (_leftButton != null)
        {
            _leftButtonText = _leftButton.GetComponentInChildren<TMP_Text>();
            if (_leftButtonText == null)
            {
                var textComponent = _leftButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    _leftButtonText = _leftButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (_leftButtonText == null)
                    {
                        _leftButtonText = _leftButton.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                        _leftButtonText.text = textComponent.text;
                        Destroy(textComponent);
                    }
                }
            }
            if (_leftButtonText != null)
            {
                _leftButtonText.text = "Влево";
            }
        }
        
        if (_rightButton != null)
        {
            _rightButtonText = _rightButton.GetComponentInChildren<TMP_Text>();
            if (_rightButtonText == null)
            {
                var textComponent = _rightButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    _rightButtonText = _rightButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (_rightButtonText == null)
                    {
                        _rightButtonText = _rightButton.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                        _rightButtonText.text = textComponent.text;
                        Destroy(textComponent);
                    }
                }
            }
            if (_rightButtonText != null)
            {
                _rightButtonText.text = "Вправо";
            }
        }
    }
    
    /// <summary>
    /// Назначает обработчики кликов для каждой кнопки
    /// </summary>
    private void AssignButtonClickHandlers()
    {
        if (_topButton != null)
        {
            _topButton.onClick.RemoveAllListeners();
            _topButton.onClick.AddListener(() => OnTransitionToDirectionClick(DoorDirection.Top));
        }
        
        if (_bottomButton != null)
        {
            _bottomButton.onClick.RemoveAllListeners();
            _bottomButton.onClick.AddListener(() => OnTransitionToDirectionClick(DoorDirection.Bottom));
        }
        
        if (_leftButton != null)
        {
            _leftButton.onClick.RemoveAllListeners();
            _leftButton.onClick.AddListener(() => OnTransitionToDirectionClick(DoorDirection.Left));
        }
        
        if (_rightButton != null)
        {
            _rightButton.onClick.RemoveAllListeners();
            _rightButton.onClick.AddListener(() => OnTransitionToDirectionClick(DoorDirection.Right));
        }
    }
    
    /// <summary>
    /// Обработчик события генерации комнаты
    /// </summary>
    /// <param name="event">Событие с информацией о сгенерированной комнате</param>
    private void OnRoomGenerated(RoomGeneratedEvent @event)
    {
        UpdateTransitionButtons();
    }
    
    /// <summary>
    /// Обновляет кнопки перехода между комнатами
    /// </summary>
    private void UpdateTransitionButtons()
    {
        // Получаем текущую комнату
        var currentRoom = _roomGeneratorService.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogWarning("Текущая комната отсутствует");
            return;
        }
        
        // Проверяем доступные направления и включаем/выключаем соответствующие кнопки
        UpdateButtonVisibility(DoorDirection.Top, _topButton, currentRoom);
        UpdateButtonVisibility(DoorDirection.Bottom, _bottomButton, currentRoom);
        UpdateButtonVisibility(DoorDirection.Left, _leftButton, currentRoom);
        UpdateButtonVisibility(DoorDirection.Right, _rightButton, currentRoom);
    }
    
    /// <summary>
    /// Обновляет видимость кнопки в зависимости от доступности направления
    /// </summary>
    /// <param name="direction">Направление для проверки</param>
    /// <param name="button">Кнопка для обновления</param>
    /// <param name="currentRoom">Текущая комната</param>
    private void UpdateButtonVisibility(DoorDirection direction, Button button, RoomNode currentRoom)
    {
        if (button == null) return;
        
        // Проверяем, есть ли сосед в этом направлении или можно ли создать соседа
        bool hasNeighbor = currentRoom.HasNeighbor(direction);
        bool canHaveNeighbor = !currentRoom.IsMaxNeighborsReached();
        
        // Кнопка активна, если уже есть сосед или можно создать нового
        bool isActive = hasNeighbor || canHaveNeighbor;
        
        button.gameObject.SetActive(isActive);
    }
    
    /// <summary>
    /// Обработчик клика по кнопке перехода в определенном направлении
    /// </summary>
    /// <param name="direction">Направление перехода</param>
    private void OnTransitionToDirectionClick(DoorDirection direction)
    {
        Debug.Log($"Клик по кнопке перехода в направлении: {direction}");
        
        // Выполняем переход в указанном направлении через RoomGenerationHandler
        _roomGenerationHandler?.GoToNeighborRoom(direction);
        
        Debug.Log($"После перехода в направлении {direction}, обновляем кнопки");
        
        // После перехода обновляем UI кнопок
        UpdateTransitionButtons();
    }
}