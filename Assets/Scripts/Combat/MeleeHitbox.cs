using UnityEngine;
using System.Collections.Generic;

namespace Cookie.Combat
{
    [RequireComponent(typeof(Collider))]
    public class MeleeHitbox : MonoBehaviour
    {
        private float _damage;
        private bool _isPlayerAttack;
        private HashSet<Collider> _hitObjects = new HashSet<Collider>();

        public void Initialize(float damage, float duration, bool isPlayerAttack)
        {
            _damage = damage;
            _isPlayerAttack = isPlayerAttack;
            _hitObjects.Clear();
            
            // Ensure collider is set up properly
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
            
            Destroy(gameObject, duration);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hitObjects.Contains(other)) return;
            
            // Don't hit the person who spawned this
            if (_isPlayerAttack && other.CompareTag("Player")) return;

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
                _hitObjects.Add(other);
                
                // Add Juice on successful hit
                if (Cookie.Core.JuiceManager.Instance != null)
                    Cookie.Core.JuiceManager.Instance.Hitstop(0.05f, 0.1f);
                    
                if (Cookie.Core.CameraController.Instance != null)
                    Cookie.Core.CameraController.Instance.Shake(0.1f, 0.1f);
            }
        }
    }
}
