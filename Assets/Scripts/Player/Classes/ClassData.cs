using UnityEngine;

namespace Cookie.Player.Classes
{
    [CreateAssetMenu(fileName = "New Class Data", menuName = "Cookie/Player Class Data")]
    public class ClassData : ScriptableObject
    {
        [Header("Class Information")]
        public string className;
        [TextArea] public string classDescription;

        [Header("Base Stats")]
        public float baseHealth = 100f;
        public float baseDamage = 10f;
        public float baseMoveSpeed = 5f;
        public float armorRating = 0f;

        [Header("Combat Settings")]
        public bool isRanged = false;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        
        // This can be used in the dialogue system or story events to check the player's class
        public ClassType classType;
    }

    public enum ClassType
    {
        Warrior,
        Mage
    }
}
