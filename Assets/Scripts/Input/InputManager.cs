using UnityEngine;
using UnityEngine.InputSystem;

namespace Cookie.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool AttackTriggered { get; private set; }
        public bool DashTriggered { get; private set; }
        public bool IsShielding => _playerInput != null && _playerInput.actions["Dodge"].IsPressed();
        
        private PlayerInput _playerInput;
        private float _lastDodgeTime;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        public void OnMove(InputValue value)
        {
            // Only reads inputs assigned to Gamepad/Touch through the Unity Input Actions Map
            MoveInput = value.Get<Vector2>();
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                AttackTriggered = true;
            }
        }
        
        public void ConsumeAttack()
        {
            AttackTriggered = false;
        }

        public void OnDodge(InputValue value)
        {
            if (value.isPressed)
            {
                if (Time.time - _lastDodgeTime < 0.3f)
                {
                    DashTriggered = true;
                }
                _lastDodgeTime = Time.time;
            }
        }

        public void ConsumeDash()
        {
            DashTriggered = false;
        }
    }
}
