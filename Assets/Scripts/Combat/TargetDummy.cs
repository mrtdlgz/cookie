using UnityEngine;
using System.Collections;

namespace Cookie.Combat
{
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 100f;
        private Renderer _renderer;
        private Color _originalColor;
        
        // AI State Machine
        private enum AIState { Patrol, Chase, Knockback }
        private AIState _currentState = AIState.Patrol;
        
        // Movement Params
        private float _patrolSpeed = 1.5f;
        private float _chaseSpeed = 4f;
        private Vector3 _patrolTarget;
        private float _patrolTimer;
        private float _knockbackTimer;
        private Vector3 _knockbackVelocity;

        private Transform _playerTransform;
        private Rigidbody _rb;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
                _originalColor = _renderer.material.color;
                
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody>();
            }
            // Constraint rotation for 2.5D dummy
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            FindPlayer();
            SetNewPatrolTarget();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (_playerTransform == null) FindPlayer();

            switch (_currentState)
            {
                case AIState.Patrol:
                    HandlePatrol();
                    break;
                case AIState.Chase:
                    HandleChase();
                    break;
                case AIState.Knockback:
                    HandleKnockbackTimer();
                    break;
            }
        }

        private void HandlePatrol()
        {
            _patrolTimer -= Time.deltaTime;
            
            // Move towards patrol target
            Vector3 direction = (_patrolTarget - transform.position).normalized;
            direction.y = 0; // Keep on ground plane
            
            _rb.velocity = new Vector3(direction.x * _patrolSpeed, _rb.velocity.y, direction.z * _patrolSpeed);

            if (_patrolTimer <= 0f || Vector3.Distance(transform.position, _patrolTarget) < 0.5f)
            {
                SetNewPatrolTarget();
            }
        }

        private void SetNewPatrolTarget()
        {
            // Random point within a 5 unit radius
            Vector2 randomCircle = Random.insideUnitCircle * 5f;
            _patrolTarget = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
            _patrolTimer = Random.Range(2f, 4f); // Change patrol target every 2-4 seconds
        }

        private void HandleChase()
        {
            if (_playerTransform == null) return;
            
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0; // Keep on ground plane
            
            _rb.velocity = new Vector3(direction.x * _chaseSpeed, _rb.velocity.y, direction.z * _chaseSpeed);
        }

        private void HandleKnockbackTimer()
        {
            _knockbackTimer -= Time.deltaTime;
            
            // Apply dramatic slow down to knockback slide
            _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
            _rb.velocity = new Vector3(_knockbackVelocity.x, _rb.velocity.y, _knockbackVelocity.z);

            if (_knockbackTimer <= 0f)
            {
                // Once knockback ends, they are angry, so CHASE
                _currentState = AIState.Chase;
            }
        }

        public void TakeDamage(float amount)
        {
            health -= amount;
            Debug.Log($"<color=orange>{gameObject.name} took {amount} damage! Health left: {health}</color>");
            
            if (_renderer != null)
                StartCoroutine(FlashColor());

            // Apply KNOCKBACK
            if (_playerTransform != null && _currentState != AIState.Knockback)
            {
                _currentState = AIState.Knockback;
                _knockbackTimer = 0.3f; // 0.3 seconds of stun/knockback
                Vector3 knockbackDir = (transform.position - _playerTransform.position).normalized;
                knockbackDir.y = 0; // Keep flat
                _knockbackVelocity = knockbackDir * 15f; // Initial explosive thrust
            }

            if (health <= 0)
                Die();
        }

        public void Die()
        {
            Debug.Log($"<color=red>{gameObject.name} was destroyed!</color>");
            Destroy(gameObject);
        }

        private IEnumerator FlashColor()
        {
            _renderer.material.color = Color.white; // Hit feedback visually
            yield return new WaitForSeconds(0.1f);
            if (_renderer != null)
                _renderer.material.color = _originalColor;
        }
    }
}
