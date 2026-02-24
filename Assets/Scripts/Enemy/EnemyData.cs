using UnityEngine;

namespace Cookie.Enemy
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Cookie/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Base Stats")]
        public string enemyName = "Slime";
        public float maxHealth = 50f;
        public float moveSpeed = 3f;
        public float attackDamage = 10f;
        public float attackRange = 1.5f;
        public float aggroRadius = 6f;

        [Header("Class Reactions")]
        [Tooltip("Eğer oyuncu Savaşçı ise bu düşman nasıl davranacak? (Örn: Kaçma eğilimi)")]
        public bool fellsFleeUrgeFromWarrior = false;
        
        [Tooltip("Eğer oyuncu Büyücü ise daha mı agresif olacak? (Örn: Daha yüksek hız/hedefleme)")]
        public bool isAggressiveToMage = true;
    }
}
