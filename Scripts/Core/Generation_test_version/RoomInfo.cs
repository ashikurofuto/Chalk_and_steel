using UnityEngine;
using System.Collections.Generic;

namespace ChalkAndSteel.Services
{
    [RequireComponent(typeof(Grid))]
    public class RoomInfo : MonoBehaviour
    {
        [Header("Wall GameObject References")]
        [SerializeField] private GameObject _wallLeft;
        [SerializeField] private GameObject _wallRight;
        [SerializeField] private GameObject _wallTop;
        [SerializeField] private GameObject _wallBottom;

        [Header("Room Settings")]
        [SerializeField] private RoomType _roomType = RoomType.NormalRoom;
        [SerializeField] private Vector3Int _gridPosition = Vector3Int.zero;
        [SerializeField] private DoorDirections _doors = DoorDirections.None;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = true;
        [SerializeField] private bool _drawGizmos = true;

        private DoorHelper _doorHelper;
        private SpriteRenderer _mainRenderer;

        public RoomType RoomType
        {
            get => _roomType;
            set
            {
                _roomType = value;
                UpdateRoomColor();
            }
        }

        public Vector3Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public DoorDirections Doors
        {
            get => _doors;
            private set => _doors = value;
        }

        private void Awake()
        {
            _doorHelper = new DoorHelper();
            _mainRenderer = GetComponent<SpriteRenderer>();
            UpdateRoomColor();
        }

        public void SetDoors(DoorDirections doors)
        {
            var oldDoors = _doors;
            _doors = doors;

            if (_showDebugInfo && oldDoors != doors)
            {
                Debug.Log($"Комната {gameObject.name} ({GridPosition}): двери = {_doorHelper.DoorDirectionsToString(doors)}");
            }

            UpdateWallsForDoors();
            UpdateRoomColor();
        }

        private void UpdateWallsForDoors()
        {
            // Убедимся что все стены найдены
            if (_wallLeft == null) _wallLeft = FindWall("Left");
            if (_wallRight == null) _wallRight = FindWall("Right");
            if (_wallTop == null) _wallTop = FindWall("Top");
            if (_wallBottom == null) _wallBottom = FindWall("Bottom");

            // Отключаем стены там, где есть двери
            if (_wallLeft != null) _wallLeft.SetActive(!_doorHelper.HasDoor(_doors, DoorDirections.West));
            if (_wallRight != null) _wallRight.SetActive(!_doorHelper.HasDoor(_doors, DoorDirections.East));
            if (_wallTop != null) _wallTop.SetActive(!_doorHelper.HasDoor(_doors, DoorDirections.North));
            if (_wallBottom != null) _wallBottom.SetActive(!_doorHelper.HasDoor(_doors, DoorDirections.South));
        }

        private GameObject FindWall(string wallName)
        {
            foreach (Transform child in transform)
            {
                if (child.name.ToLower().Contains(wallName.ToLower()))
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        private void UpdateRoomColor()
        {
            if (_mainRenderer == null) return;

            Color color = Color.white;
            switch (_roomType)
            {
                case RoomType.StartRoom:
                    color = Color.green;
                    break;
                case RoomType.BossRoom:
                    color = Color.red;
                    break;
                case RoomType.TreasureRoom:
                    color = Color.yellow;
                    break;
                case RoomType.SpecialRoom:
                    color = Color.magenta;
                    break;
                case RoomType.NormalRoom:
                    color = Color.white;
                    break;
            }

            _mainRenderer.color = color;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos) return;

            // Рисуем линии для дверей
            Gizmos.color = Color.green;
            float lineLength = 2.5f;

            if (_doorHelper.HasDoor(_doors, DoorDirections.North))
            {
                Vector3 start = transform.position;
                Vector3 end = start + Vector3.up * lineLength;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.2f);
            }

            if (_doorHelper.HasDoor(_doors, DoorDirections.East))
            {
                Vector3 start = transform.position;
                Vector3 end = start + Vector3.right * lineLength;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.2f);
            }

            if (_doorHelper.HasDoor(_doors, DoorDirections.South))
            {
                Vector3 start = transform.position;
                Vector3 end = start + Vector3.down * lineLength;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.2f);
            }

            if (_doorHelper.HasDoor(_doors, DoorDirections.West))
            {
                Vector3 start = transform.position;
                Vector3 end = start + Vector3.left * lineLength;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.2f);
            }

            // Рисуем контур комнаты
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(11, 11, 0));

            // Подписываем комнату
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3,
                $"{gameObject.name}\n{_doorHelper.DoorDirectionsToString(_doors)}");
        }
#endif
    }
}