using UnityEngine;
using Cookie.Combat;
using Cookie.Core;
using Cookie.Player.Classes;

namespace Cookie.Enemy
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class EnemyAI : MonoBehaviour, IDamageable
    {
        [Header("References")]
        public EnemyData data;
        
        // AI State Machine
        private enum AIState { Patrol, Chase, Flee, Attack, Knockback, Dead }
        private AIState _currentState = AIState.Patrol;
        
        // Internal Tracking
        private float _currentHealth;
        private Vector3 _patrolTarget;
        private float _stateTimer;
        private Rigidbody _rb;
        private Renderer _renderer;
        private Color _originalColor;
        
        // Player Reaction Logic
        private Transform _playerTransform;
        private PlayerController _playerController;
        private bool _fleeFromWarrior;
        private bool _rageAgainstMage;
        
        private float _attackCooldown;

        private void Start()
        {
            if (data == null)
            {
                Debug.LogError($"{gameObject.name} lacks EnemyData!");
                Destroy(this);
                return;
            }

            _currentHealth = data.maxHealth;
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            _renderer = GetComponent<Renderer>();
            if (_renderer != null) _originalColor = _renderer.material.color;

            FindPlayerAndEvaluateClass();
            SetNewPatrolTarget();
        }

        private void FindPlayerAndEvaluateClass()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerController = player.GetComponent<PlayerController>();

                if (_playerController != null && _playerController.currentClass != null)
                {
                    string pClass = _playerController.currentClass.className;
                    
                    // Specific Class Checks
                    if (pClass.Contains("Savasci") || pClass.Contains("Warrior"))
                    {
                        _fleeFromWarrior = data.fellsFleeUrgeFromWarrior;
                    }
                    else if (pClass.Contains("Buyucu") || pClass.Contains("Mage"))
                    {
                        _rageAgainstMage = data.isAggressiveToMage;
                    }
                }
            }
        }

        private void Update()
        {
            // OOB Fall Catcher
            if (transform.position.y < -5f)
            {
                transform.position = new Vector3(0, 1.5f, 0);
                _rb.velocity = Vector3.zero;
            }

            if (_currentState == AIState.Dead) return;
            if (_playerTransform == null) FindPlayerAndEvaluateClass();

            _attackCooldown -= Time.deltaTime;

            switch (_currentState)
            {
                case AIState.Patrol: HandlePatrol(); break;
                case AIState.Chase: HandleChase(); break;
                case AIState.Flee: HandleFlee(); break;
                case AIState.Attack: HandleAttack(); break;
                case AIState.Knockback: HandleKnockback(); break;
            }
        }

        private void HandlePatrol()
        {
            _stateTimer -= Time.deltaTime;
            MoveTowards(_patrolTarget, data.moveSpeed * 0.5f); // Walk slowly while patrolling

            if (PlayerInAggroRange())
            {
                // Reaction branching
                if (_fleeFromWarrior) _currentState = AIState.Flee;
                else _currentState = AIState.Chase;
                return;
            }

            if (_stateTimer <= 0f || Vector3.Distance(transform.position, _patrolTarget) < 0.5f)
            {
                SetNewPatrolTarget();
            }
        }

        private void HandleChase()
        {
            if (_playerTransform == null) return;

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            
            if (dist <= data.attackRange)
            {
                _currentState = AIState.Attack;
                return;
            }
            
            if (dist > data.aggroRadius * 1.5f)
            {
                SetNewPatrolTarget(); // Player escaped
                return;
            }

            float chaseSpeed = _rageAgainstMage ? data.moveSpeed * 1.5f : data.moveSpeed;
            MoveTowards(_playerTransform.position, chaseSpeed);
        }

        private void HandleFlee()
        {
            if (_playerTransform == null) return;
            
            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            if (dist > data.aggroRadius * 2f)
            {
                SetNewPatrolTarget(); // Successfully fled
                return;
            }

            // Run away from player
            Vector3 fleeDir = (transform.position - _playerTransform.position).normalized;
            Vector3 targetSafeSpot = transform.position + fleeDir * 3f;
            MoveTowards(targetSafeSpot, data.moveSpeed * 1.2f);
        }

        private void HandleAttack()
        {
            if (_playerTransform == null) return;
            
            _rb.velocity = Vector3.zero; // Stop to attack
            
            if (Vector3.Distance(transform.position, _playerTransform.position) > data.attackRange)
            {
                _currentState = AIState.Chase;
                return;
            }

            if (_attackCooldown <= 0f)
            {
                // Perform Attack
                Debug.Log($"<color=red>{data.enemyName} attacks player for {data.attackDamage} damage!</color>");
                IDamageable pDamage = _playerTransform.GetComponent<IDamageable>();
                if (pDamage != null) pDamage.TakeDamage(data.attackDamage);
                
                _attackCooldown = 1.5f; // Cooldown between hits
            }
        }

        private void HandleKnockback()
        {
            _stateTimer -= Time.deltaTime;
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, Time.deltaTime * 6f); // Slide to a stop

            if (_stateTimer <= 0f)
            {
                if (_fleeFromWarrior) _currentState = AIState.Flee;
                else _currentState = AIState.Chase;
            }
        }

        private void MoveTowards(Vector3 targetPos, float speed)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0;
            _rb.velocity = new Vector3(direction.x * speed, _rb.velocity.y, direction.z * speed);
            
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }
        }

        private bool PlayerInAggroRange()
        {
            if (_playerTransform == null) return false;
            return Vector3.Distance(transform.position, _playerTransform.position) <= data.aggroRadius;
        }

        private void SetNewPatrolTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * data.aggroRadius;
            _patrolTarget = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
            _stateTimer = Random.Range(2f, 4f);
            _currentState = AIState.Patrol;
        }

        public void TakeDamage(float amount)
        {
            if (_currentState == AIState.Dead) return;

            _currentHealth -= amount;
            Debug.Log($"<color=orange>{data.enemyName} hit! Health: {_currentHealth}</color>");
            
            if (_renderer != null)
                StartCoroutine(FlashColorRoutine());

            if (_currentHealth <= 0)
            {
                Die();
            }
            else if (_playerTransform != null)
            {
                // Apply Knockback
                _currentState = AIState.Knockback;
                _stateTimer = 0.4f; // Knockback duration
                
                Vector3 knockbackDir = (transform.position - _playerTransform.position).normalized;
                knockbackDir.y = 0;
                _rb.velocity = knockbackDir * 12f; // Knockback force
            }
        }

        public void Die()
        {
            _currentState = AIState.Dead;
            _rb.velocity = Vector3.zero;
            _rb.detectCollisions = false;
            
            Debug.Log($"<color=gray>{data.enemyName} defeated!</color>");
            
            // Temporary death visual (shrink & destroy)
            transform.localScale = Vector3.one * 0.1f;
            Destroy(gameObject, 1f);
        }

        private System.Collections.IEnumerator FlashColorRoutine()
        {
            _renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null) _renderer.material.color = _originalColor;
        }
    }
}
