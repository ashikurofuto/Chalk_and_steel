using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ChalkAndSteel.Services;
using Architecture.GlobalModules;

public class RoomSceneHandler : MonoBehaviour
{
    [SerializeField] private Transform _tilesParent; // Родительский объект для тайлов
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _entrancePrefab;
    [SerializeField] private GameObject _exitPrefab;
    [SerializeField] private GameObject _trapPrefab;
    [SerializeField] private GameObject _interactiveObjectPrefab;
    [SerializeField] private GameObject _pillarPrefab;

    private IEventBus _eventBus;
    private IRoomService _roomService;
    private GameObject[,] _tileObjects; // Для хранения созданных объектов

    [Inject]
    private void Construct(IEventBus eventBus, IRoomService roomService)
    {
        _eventBus = eventBus;
        _roomService = roomService;
    }

    private void OnEnable()
    {
        if (_eventBus != null)
            _eventBus.Subscribe<RoomTransitionEvent>(OnRoomTransitioned);
    }
    
    private void OnDisable()
    {
        if (_eventBus != null)
            _eventBus.Unsubscribe<RoomTransitionEvent>(OnRoomTransitioned);
    }

    private void OnRoomTransitioned(RoomTransitionEvent e)
    {
        // Выгрузка предыдущей комнаты
        ClearCurrentRoomVisualization();

        // Загрузка новой комнаты
        LoadNewRoomVisualization();
    }

    private void ClearCurrentRoomVisualization()
    {
        if (_tileObjects != null)
        {
            for (int x = 0; x < _tileObjects.GetLength(0); x++)
            {
                for (int y = 0; y < _tileObjects.GetLength(1); y++)
                {
                    if (_tileObjects[x, y] != null)
                    {
                        DestroyImmediate(_tileObjects[x, y]); // Используем DestroyImmediate для корректной очистки
                    }
                }
            }
            _tileObjects = null;
        }
    }

    private void LoadNewRoomVisualization()
    {
        var currentRoom = _roomService.GetCurrentRoom();
        if (currentRoom == null || currentRoom.Grid == null) return;

        int width = currentRoom.Grid.GetLength(0);
        int height = currentRoom.Grid.GetLength(1);
        
        // Очищаем предыдущую визуализацию, если она была
        ClearCurrentRoomVisualization();
        
        _tileObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = currentRoom.Grid[x, y];
                GameObject prefabToUse = null;

                switch (tile.Type)
                {
                    case TileType.Floor: prefabToUse = _floorPrefab; break;
                    case TileType.Wall: prefabToUse = _wallPrefab; break;
                    case TileType.Entrance: prefabToUse = _entrancePrefab; break;
                    case TileType.Exit: prefabToUse = _exitPrefab; break;
                    case TileType.Trap: prefabToUse = _trapPrefab; break;
                    case TileType.InteractiveObject: prefabToUse = _interactiveObjectPrefab; break;
                    case TileType.Pillar: prefabToUse = _pillarPrefab; break;
                    // Add cases for other types if needed
                }

                if (prefabToUse != null)
                {
                    Vector3 position = new Vector3(x, y, 0); // В 2D используем X-Y плоскость
                    
                    // Для ловушек и интерактивных объектов, размещаем на той же позиции, что и пол
                    // чтобы они не оказались "в воздухе" над полом
                    _tileObjects[x, y] = Instantiate(prefabToUse, position, Quaternion.identity, _tilesParent);
                }
            }
        }
    }
}