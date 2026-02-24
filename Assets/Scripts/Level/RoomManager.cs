using UnityEngine;
using System.Collections.Generic;

namespace Cookie.Level
{
    public class RoomManager : MonoBehaviour
    {
        [Header("Room Settings")]
        public bool isRoomCleared = false;
        
        [Tooltip("The trigger volume that detects when player enters the room")]
        public Collider entranceTrigger;

        [Header("Gates / Doors")]
        public GameObject[] roomDoors; // Doors to lock the player inside during combat

        [Header("Enemy Waves")]
        [Tooltip("List of enemy prefabs to spawn in this room")]
        public List<GameObject> enemyPrefabsToSpawn;
        public Transform[] spawnPoints;
        
        private int _aliveEnemiesCount = 0;
        private bool _roomActivated = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!_roomActivated && !isRoomCleared)
            {
                if (other.CompareTag("Player"))
                {
                    ActivateRoom();
                }
            }
        }

        private void ActivateRoom()
        {
            _roomActivated = true;
            Debug.Log("Room Activated! Locking doors and spawning enemies.");

            // 1. Lock Doors
            LockDoors();

            // 2. Spawn Enemies
            SpawnEnemies();
        }

        private void LockDoors()
        {
            foreach (var door in roomDoors)
            {
                if (door != null) door.SetActive(true); // Assuming active means door is solid/closed
            }
        }

        private void UnlockDoors()
        {
            foreach (var door in roomDoors)
            {
                if (door != null) door.SetActive(false); // Open doors to next corridor
            }
            Debug.Log("Room Cleared! Doors Opened.");
        }

        private void SpawnEnemies()
        {
            if (enemyPrefabsToSpawn == null || enemyPrefabsToSpawn.Count == 0)
            {
                CompleteRoom(); // Empty room immediately clears
                return;
            }

            for (int i = 0; i < enemyPrefabsToSpawn.Count; i++)
            {
                if (spawnPoints.Length == 0) break;
                
                Transform sp = spawnPoints[i % spawnPoints.Length];
                GameObject enemy = Instantiate(enemyPrefabsToSpawn[i], sp.position, sp.rotation);
                
                // Keep track of how many enemies are spawned
                _aliveEnemiesCount++;
                
                // TODO: Hook into Enemy death event
                // Current mock-up assumes an EnemyHealth script triggers OnEnemyDeath
            }
        }

        public void OnEnemyDefeated()
        {
            _aliveEnemiesCount--;
            if (_aliveEnemiesCount <= 0 && _roomActivated)
            {
                CompleteRoom();
            }
        }

        private void CompleteRoom()
        {
            isRoomCleared = true;
            UnlockDoors();
        }
    }
}
