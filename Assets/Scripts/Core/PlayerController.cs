using UnityEngine;
using Cookie.Input;
using Cookie.Player.Classes;
using Cookie.Combat;

namespace Cookie.Core
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputManager))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Class Data Configuration")]
        [Tooltip("Assign Warrior or Mage scriptable object here")]
        public ClassData currentClass;
        
        [Header("Camera Settings")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController _characterController;
        private InputManager _inputManager;
        
        // Runtime Stats
        private float _currentHealth;
        private float _currentMoveSpeed;
        
        // Dash / Dodge Settings
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.2f;
        private bool _isDashing = false;
        private float _dashTimer = 0f;
        private Vector3 _dashDirection;
        
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
            // OOB Fall Catcher
            if (transform.position.y < -5f)
            {
                _characterController.enabled = false;
                transform.position = new Vector3(0, 1.1f, 0);
                _characterController.enabled = true;
            }
            
            if (_isDashing)
            {
                HandleDash();
                return; // Normal movement and combat inputs are blocked while dashing
            }

            HandleCombatInputs();
            HandleMovement();
        }

        private void HandleMovement()
        {
            float speedToUse = _currentMoveSpeed;
            if (_inputManager.IsShielding) 
            {
                speedToUse = _currentMoveSpeed * 0.4f; // Slower while shielding
            }

            if (_inputManager.MoveInput.sqrMagnitude < 0.01f)
            {
                // Just apply gravity if standing still
                ApplyGravity(Vector3.zero, speedToUse);
                return;
            }

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

            ApplyGravity(targetDirection, speedToUse);
        }

        private void ApplyGravity(Vector3 moveDirection, float speed)
        {
            // Apply Gravity (Basic Implementation)
            if (_characterController.isGrounded)
            {
                _verticalVelocity = -0.5f; // Small constant downward force to stay grounded
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            Vector3 finalMove = moveDirection * speed;
            finalMove.y = _verticalVelocity;

            // Move character using CharacterController
            _characterController.Move(finalMove * Time.deltaTime);
        }
        
        private void HandleDash()
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }

            // Keep gravity updated during dash
            if (!_characterController.isGrounded)
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
            else
            {
                _verticalVelocity = -0.5f;
            }

            Vector3 finalMove = _dashDirection * dashSpeed;
            finalMove.y = _verticalVelocity;
            
            _characterController.Move(finalMove * Time.deltaTime);
        }

        private void HandleCombatInputs()
        {
            if (_inputManager.AttackTriggered)
            {
                PerformAttack();
                _inputManager.ConsumeAttack();
            }

            if (_inputManager.DashTriggered)
            {
                PerformDodge();
                _inputManager.ConsumeDash();
            }
            
            // Visual feedback for shielding
            MeshRenderer rnd = GetComponentInChildren<MeshRenderer>();
            if (rnd != null)
            {
                if (_inputManager.IsShielding)
                    rnd.material.color = Color.blue;
                else
                    rnd.material.color = new Color(0.9f, 0.4f, 0.1f); // Default orange
            }
        }

        private void PerformAttack()
        {
            // The attack logic branches here based on the player's class type
            if (currentClass != null)
            {
                if (currentClass.className.Contains("Savasci") || currentClass.className.Contains("Warrior"))
                {
                    // Melee Hitbox Prototype
                    GameObject hitboxObj = GameObject.CreatePrimitive(PrimitiveType.Cube); // Made it visible!
                    hitboxObj.name = "MeleeHitbox";
                    hitboxObj.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 0.2f; // Lowered to hit small slimes!
                    hitboxObj.transform.rotation = transform.rotation;
                    hitboxObj.transform.localScale = new Vector3(1.5f, 1f, 1.5f); // Smaller, snappier size!
                    
                    hitboxObj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.5f); // Semi transparent red
                    
                    BoxCollider col = hitboxObj.GetComponent<BoxCollider>();
                    col.isTrigger = true;
                    
                    Rigidbody rb = hitboxObj.AddComponent<Rigidbody>();
                    rb.isKinematic = true; // Required by Unity for OnTriggerEnter to work!
                    
                    Cookie.Combat.MeleeHitbox hitbox = hitboxObj.AddComponent<Cookie.Combat.MeleeHitbox>();
                    hitbox.Initialize(currentClass.baseDamage, 0.2f, true);
                }
                else
                {
                    // Mage Projectile Prototype
                    GameObject projObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    projObj.name = "MagicProjectile";
                    projObj.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1f;
                    projObj.transform.rotation = transform.rotation;
                    projObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    
                    projObj.GetComponent<MeshRenderer>().material.color = Color.cyan;
                    
                    Cookie.Combat.Projectile proj = projObj.AddComponent<Cookie.Combat.Projectile>();
                    proj.Initialize(currentClass.baseDamage, 15f, 2f, true);
                }
            }
        }

        private void PerformDodge()
        {
            if (_isDashing) return; // Prevent spamming dash

            _isDashing = true;
            _dashTimer = dashDuration;

            // Determine dash direction
            Vector3 inputDirection = new Vector3(_inputManager.MoveInput.x, 0f, _inputManager.MoveInput.y).normalized;
            
            if (inputDirection.magnitude > 0.1f && cameraTransform != null)
            {
                // Dash in the direction of the joystick
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                
                _dashDirection = (camForward.normalized * inputDirection.z + camRight.normalized * inputDirection.x).normalized;
            }
            else
            {
                // Dash in the direction we are currently facing
                _dashDirection = transform.forward;
            }
            
            // Snap rotation to dash direction
            transform.rotation = Quaternion.LookRotation(_dashDirection);

            Debug.Log("Dodging in direction: " + _dashDirection);
        }

        // --- IDamageable Implementation ---
        public void TakeDamage(float amount)
        {
            float actualDamage = amount;

            if (_inputManager.IsShielding)
            {
                // %80 Hasar İndirimi
                actualDamage = amount * 0.2f;
                Debug.Log($"<color=blue>Kalkan Aktif! Bloklanan hasar: {amount - actualDamage}. Alınan: {actualDamage}</color>");
            }

            _currentHealth -= actualDamage;
            Debug.Log($"<color=red>Oyuncu {actualDamage} hasar aldı! Kalan Can: {_currentHealth}</color>");

            // Visual feedback (Flash red)
            MeshRenderer rnd = GetComponentInChildren<MeshRenderer>();
            if (rnd != null)
            {
                StartCoroutine(FlashColorRoutine(rnd, Color.red, 0.15f));
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            Debug.Log("<color=black>OYUNCU ÖLDÜ!</color>");
            // Trigger death animation/UI later
            gameObject.SetActive(false); // Temporary death state
        }
        
        private System.Collections.IEnumerator FlashColorRoutine(MeshRenderer rnd, Color flashColor, float duration)
        {
            Color orig = rnd.material.color;
            rnd.material.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (rnd != null)
                rnd.material.color = _inputManager.IsShielding ? Color.blue : new Color(0.9f, 0.4f, 0.1f);
        }
    }
}
