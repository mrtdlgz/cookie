using UnityEngine;

namespace Cookie.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private float _damage;
        private float _speed;
        private bool _isPlayerAttack;

        public void Initialize(float damage, float speed, float lifetime, bool isPlayerAttack)
        {
            _damage = damage;
            _speed = speed;
            _isPlayerAttack = isPlayerAttack;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; 
            
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
            
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            // Do not hit the player if the player fired this
            if (_isPlayerAttack && other.CompareTag("Player")) return;

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
                
                // Add Juice on successful hit
                if (Cookie.Core.JuiceManager.Instance != null)
                    Cookie.Core.JuiceManager.Instance.Hitstop(0.03f, 0.2f);
                    
                if (Cookie.Core.CameraController.Instance != null)
                    Cookie.Core.CameraController.Instance.Shake(0.1f, 0.05f);
            }

            Destroy(gameObject);
        }
    }
}
