// Managers/InputService.cs ()
using Architecture.GlobalModules;
using Architecture.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class InputService : IInputService
{
    private readonly IEventBus _eventBus;

    private Vector2 _cachedMoveDirection;
    private bool _isEnabled;
    private BaseInput baseInput;
    //   
    private Vector2 _lastMoveInput;
    private bool _moveKeyWasPressed;

    public InputService(
        IEventBus eventBus)
    {
        _eventBus = eventBus;

        Enable();
    }

    public void Enable()
    {
        if (_isEnabled) return;
        _isEnabled = true;
    }

    public void Disable()
    {
        if (!_isEnabled) return;

        _isEnabled = false;
    }

    public Vector2 GetMoveDirection() => _cachedMoveDirection;

    public bool IsInteractPressed()
    {
        throw new System.NotImplementedException();
    }

    public bool IsInventoryPressed()
    {
        throw new System.NotImplementedException();
    }

    public bool IsPausePressed()
    {
        throw new System.NotImplementedException();
    }

    public bool IsUndoPressed()
    {
        throw new System.NotImplementedException();
    }
    /*
   public bool IsInteractPressed() => _inputWrapper.Interact;

   public bool IsInventoryPressed() => _inputWrapper.Inventory;

   public bool IsPausePressed() => _inputWrapper.Pause;

   public bool IsUndoPressed()
   {
       //   Ctrl+Z   Input System
       var keyboard = Keyboard.current;
       if (keyboard == null) return false;

       return (keyboard.ctrlKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) 
              && keyboard.zKey.wasPressedThisFrame;
   }

   //  : ,     
   public bool TryGetDiscreteMove(out Vector2 direction)
   {
       direction = Vector2.zero;

       Vector2 currentInput = _cachedMoveDirection;
       bool isPressed = currentInput.magnitude > 0.1f;

       if (isPressed && !_moveKeyWasPressed)
       {
           //   
           direction = currentInput;
           _moveKeyWasPressed = true;
           return true;
       }

       if (!isPressed)
       {
           //  
           _moveKeyWasPressed = false;
       }

       return false;
   }

   private void OnMoveChanged(Vector2 direction)
   {
       _cachedMoveDirection = direction;
       _eventBus.Publish(new MoveInputEvent(direction));

       //   (  )
       if (TryGetDiscreteMove(out Vector2 discreteDirection))
       {
           Vector3Int discreteDirectionInt = new Vector3Int((int)discreteDirection.x, (int)discreteDirection.y, 0);
           _eventBus.Publish(new DiscreteMoveInputEvent(discreteDirectionInt));
       }
   }

   private void OnInteract()
   {
       _eventBus.Publish(new InteractInputEvent());
   }

   private void OnInventory()
   {
       _eventBus.Publish(new InventoryInputEvent());
   }

   private void OnPause()
   {
       _eventBus.Publish(new PauseInputEvent());
   }

   public void Dispose()
   {
       Disable();
       _inputWrapper.OnMoveChanged -= OnMoveChanged;
       _inputWrapper.OnInteract -= OnInteract;
       _inputWrapper.OnInventory -= OnInventory;
       _inputWrapper.OnPause -= OnPause;
       _inputWrapper.Dispose();
   }
*/
}