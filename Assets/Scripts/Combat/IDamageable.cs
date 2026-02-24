using UnityEngine;

namespace Cookie.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void Die();
    }
}
