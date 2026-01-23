using UnityEngine;

namespace Architecture.GlobalModules.Events
{
    /// <summary>
    /// ������� ����� �������� ������.
    /// ����������� ��� ��������� ������� ��������.
    /// </summary>
    public record MoveInputEvent
    {
        public Vector2 Direction { get; }

        public MoveInputEvent(Vector2 direction)
        {
            Direction = direction;
        }
    }
}



