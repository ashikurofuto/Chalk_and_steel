using UnityEngine;

public record HubInteractionEvent
{
    public Vector3Int InteractionPosition { get; }

    public HubInteractionEvent(Vector3Int position)
    {
        InteractionPosition = position;
    }
}
