using UnityEngine;
using UnityEngine.InputSystem;

namespace Cookie.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputManager : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool AttackTriggered { get; private set; }
        public bool DodgeTriggered { get; private set; }

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
                DodgeTriggered = true;
            }
        }

        public void ConsumeDodge()
        {
            DodgeTriggered = false;
        }
    }
}
