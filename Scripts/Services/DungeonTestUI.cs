using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using ChalkAndSteel.Services;
using System.Collections.Generic;

namespace ChalkAndSteel.UI
{
    public class DungeonTestUI : MonoBehaviour
    {
        [SerializeField] private Button generateDungeonButton;
        [SerializeField] private Transform buttonsParent; // Родитель для кнопок перехода
        [SerializeField] private Button roomButtonPrefab; // Префаб кнопки комнаты
        [SerializeField] private TMP_Text debugText; // Для отображения информации
        
        private IRoomService _roomService;
        private IPlayerService _playerService;
        private List<Button> _roomButtons = new();
        
        private void Awake()
        {
            SetupUI();
        }
        
        private void SetupUI()
        {
            if (generateDungeonButton != null)
            {
                generateDungeonButton.onClick.AddListener(GenerateDungeon);
            }
        }
        
        [Inject]
        public void Construct(IRoomService roomService, IPlayerService playerService)
        {
            _roomService = roomService;
            _playerService = playerService;
        }
        
        private void GenerateDungeon()
        {
            Debug.Log("Generating dungeon...");
            
            if (_playerService == null || _roomService == null)
            {
                Debug.LogError("Services not set! Please call SetServices method first.");
                return;
            }
            
            // Получаем текущую стадию игрока
            var currentStage = _playerService.GetCurrentStage();
            
            // Инициализируем подземелье
            _roomService.InitializeDungeon((int)currentStage);
            
            // Отображаем информацию о текущей комнате
            DisplayRoomInfo();
            
            // Создаем кнопки для доступных комнат
            CreateRoomTransitionButtons();
            
            Debug.Log($"Dungeon generated for stage: {currentStage}");
        }
        
        private void GoToNextRoom()
        {
            Debug.Log("Trying to enter next room...");
            
            if (_roomService == null)
            {
                Debug.LogError("Room service not set! Please call SetServices method first.");
                return;
            }
            
            // Получаем текущую комнату
            var currentRoom = _roomService.GetCurrentRoom();
            if (currentRoom == null)
            {
                Debug.LogWarning("No current room found. Generate dungeon first.");
                return;
            }
            
            // Пытаемся перейти в следующую комнату
            // Для теста берем первую доступную комнату из соединений
            if (currentRoom.Connections.Length > 0)
            {
                int nextRoomId = currentRoom.Connections[0];
                
                bool success = _roomService.TryEnterRoom(nextRoomId);
                if (success)
                {
                    Debug.Log($"Entered room {nextRoomId}");
                    DisplayRoomInfo();
                    // Обновляем кнопки перехода
                    CreateRoomTransitionButtons();
                }
                else
                {
                    Debug.LogWarning($"Failed to enter room {nextRoomId}");
                }
            }
            else
            {
                Debug.LogWarning("No connections from current room");
            }
        }

        private void CreateRoomTransitionButtons()
        {
            // Очищаем предыдущие кнопки
            ClearRoomTransitionButtons();
            
            var currentRoom = _roomService.GetCurrentRoom();
            if (currentRoom == null || currentRoom.Connections.Length == 0) return;
            
            // Создаем кнопки для каждой доступной комнаты
            foreach (int roomId in currentRoom.Connections)
            {
                var button = Instantiate(roomButtonPrefab, buttonsParent);
                button.GetComponentInChildren<TMP_Text>().text = $"Room {roomId}";
                
                int targetRoomId = roomId; // Захватываем переменную для замыкания
                button.onClick.AddListener(() => EnterSpecificRoom(targetRoomId));
                
                _roomButtons.Add(button);
            }
        }

        private void EnterSpecificRoom(int targetRoomId)
        {
            if (_roomService != null)
            {
                bool success = _roomService.TryEnterRoom(targetRoomId);
                if (success)
                {
                    Debug.Log($"Entered room {targetRoomId}");
                    DisplayRoomInfo();
                    // Обновляем кнопки перехода
                    CreateRoomTransitionButtons();
                }
                else
                {
                    Debug.LogWarning($"Failed to enter room {targetRoomId}");
                }
            }
        }

        private void ClearRoomTransitionButtons()
        {
            foreach (var button in _roomButtons)
            {
                if (button != null)
                    DestroyImmediate(button.gameObject);
            }
            _roomButtons.Clear();
        }
        
        private void DisplayRoomInfo()
        {
            if (debugText == null) return;
            
            if (_roomService == null)
            {
                debugText.text = "Services not set. Call SetServices method first.";
                return;
            }
            
            var currentRoom = _roomService.GetCurrentRoom();
            if (currentRoom != null)
            {
                debugText.text = $"Current Room: {currentRoom.Id}\n" +
                               $"Type: {currentRoom.Type}\n" +
                               $"Connections: {currentRoom.Connections.Length}\n" +
                               $"Generated: {currentRoom.IsGenerated}";
            }
            else
            {
                debugText.text = "No current room. Generate dungeon first.";
            }
        }
        
        private void OnDestroy()
        {
            if (generateDungeonButton != null)
            {
                generateDungeonButton.onClick.RemoveListener(GenerateDungeon);
            }
            
            // Очищаем все созданные кнопки перехода
            ClearRoomTransitionButtons();
        }
    }
}