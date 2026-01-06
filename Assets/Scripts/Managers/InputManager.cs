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
    public bool CheckInput() {
        return false;
    }

    public void OnUpdate(Define.InputType inputType, bool isTrigger)
    {
        
    }
}
