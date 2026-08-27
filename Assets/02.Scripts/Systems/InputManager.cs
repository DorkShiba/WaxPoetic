using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Systems
{
    public class InputManager
    {
        PlayerInput _playerInput = null;

        public Vector2 MoveDirection { get; private set; }
        public Vector2 MousePosition { get; private set; }

        public Action OnInteractPerformed;
        public Action OnAvoidPerformed;
        public Action<int> OnAttackPerformed;
        public Action OnMouseClickPerformed;
        public Action OnMouseRClickPerformed;

        public InputManager()
        {
            _playerInput = new @PlayerInput();

            _playerInput.Player.Move.performed -= OnMovePerformed;
            _playerInput.Player.Move.performed += OnMovePerformed;
            _playerInput.Player.Move.canceled += _ => MoveDirection = Vector2.zero;

            _playerInput.Player.MousePos.performed += context => MousePosition = context.ReadValue<Vector2>();
            _playerInput.Player.MousePos.canceled += _ => MousePosition = Vector2.zero;

            _playerInput.Player.MouseClick.performed += _ => OnMouseClickPerformed?.Invoke();
            _playerInput.Player.MouseRClick.performed += _ => OnMouseRClickPerformed?.Invoke();

            _playerInput.Player.Interact.performed += _ => OnInteractPerformed?.Invoke();

            _playerInput.Player.Avoid.performed += _ => OnAvoidPerformed?.Invoke();

            _playerInput.Player.Skill1.performed += _ => OnAttackPerformed?.Invoke(0);
            _playerInput.Player.Skill2.performed += _ => OnAttackPerformed?.Invoke(1);
            _playerInput.Player.Skill3.performed += _ => OnAttackPerformed?.Invoke(2);
            _playerInput.Player.Skill4.performed += _ => OnAttackPerformed?.Invoke(3);

            _playerInput.Enable();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            Vector2 currentDirection = context.ReadValue<Vector2>();
            MoveDirection = currentDirection;
        }

        public void OnUpdate()
        {
        }
    }
}
