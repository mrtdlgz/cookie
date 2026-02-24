using UnityEngine;
using Cookie.Input;
using Cookie.Player.Classes;

namespace Cookie.Core
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputManager))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Class Data Configuration")]
        [Tooltip("Assign Warrior or Mage scriptable object here")]
        [SerializeField] private ClassData currentClass;
        
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _characterController;
        private InputManager _inputManager;
        
        // Runtime Stats
        private float _currentHealth;
        private float _currentMoveSpeed;
        
        // Gravity settings (for character controller on slopes/stairs)
        private float _gravity = -9.81f;
        private float _verticalVelocity;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputManager = GetComponent<InputManager>();
        }

        private void Start()
        {
            if (currentClass == null)
            {
                Debug.LogError("No Player Class Data Assigned to PlayerController!");
                return;
            }

            // Initialize player stats from chosen class
            _currentHealth = currentClass.baseHealth;
            _currentMoveSpeed = currentClass.baseMoveSpeed;
        }

        private void Update()
        {
            HandleMovement();
            HandleCombatInputs();
        }

        private void HandleMovement()
        {
            if (_inputManager.MoveInput.sqrMagnitude < 0.01f) return;

            // Normalize input so diagonal movement isn't faster
            Vector3 inputDirection = new Vector3(_inputManager.MoveInput.x, 0f, _inputManager.MoveInput.y).normalized;

            // Calculate movement direction relative to camera's forward
            Vector3 targetDirection = inputDirection;

            if (cameraTransform != null)
            {
                // Align movement correctly in 2.5D Isometric space
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;

                camForward.y = 0;
                camRight.y = 0;

                camForward.Normalize();
                camRight.Normalize();

                targetDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
            }

            // Face the character towards movement direction
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Fast smooth rotation
            }

            // Apply Gravity (Basic Implementation)
            if (_characterController.isGrounded)
            {
                _verticalVelocity = -0.5f; // Small constant downward force to stay grounded
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            Vector3 finalMove = targetDirection * _currentMoveSpeed;
            finalMove.y = _verticalVelocity;

            // Move character using CharacterController
            _characterController.Move(finalMove * Time.deltaTime);
        }

        private void HandleCombatInputs()
        {
            if (_inputManager.AttackTriggered)
            {
                PerformAttack();
                _inputManager.ConsumeAttack();
            }

            if (_inputManager.DodgeTriggered)
            {
                PerformDodge();
                _inputManager.ConsumeDodge();
            }
        }

        private void PerformAttack()
        {
            // The attack logic will branch here based on the player's class type
            if (currentClass != null)
            {
                Debug.Log($"Attacking with class: {currentClass.className}. Damage: {currentClass.baseDamage}");
                
                // TODO: Instantiate spell projectile if Mage, trigger sword swing hitbox if Warrior
            }
        }

        private void PerformDodge()
        {
             // TODO: Trigger dodge animation and apply quick dash force.
             // Mages might dash further or have a teleport animation, Warriors might do a heavy roll.
             Debug.Log("Dodge button pressed!");
        }
    }
}
