using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager {
    PlayerInput _input = null;
    bool _canMove = true;

    public void EnableInput(bool enable) {
    }
    public void EnableMove(bool enable) {
        _canMove = enable;
    }
    public bool CheckInput(Define.InputType inputType, bool isTrigger) {
        return false;
    }

    public void OnUpdate()
    {
        
    }
}
