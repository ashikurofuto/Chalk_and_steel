using UnityEngine;
using UnityEngine.Tilemaps;
/// <summary>
/// Интерфейс управления движением в хабе.
/// </summary>
public interface IHubMovementService
    {
        void Initialize(Tilemap groundTilemap, Tilemap borderTilemap, Vector3Int startPosition);
        bool CanMoveTo(Vector3Int targetPosition);
        void MoveTo(Vector3Int targetPosition);
        Vector3Int GetCurrentPosition();
}



