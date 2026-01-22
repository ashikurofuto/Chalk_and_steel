using UnityEngine;
using ChalkAndSteel.Services;

/// <summary>
///     .
/// </summary>
public interface IMovementService
{
    void Initialize(DualLayerTile[,] roomGrid, Vector3Int startPosition);
    bool CanMoveTo(Vector3Int targetPosition);
    void MoveTo(Vector3Int targetPosition);
    Vector3Int GetCurrentPosition();
}
