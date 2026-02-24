using UnityEngine;
using System.Collections;

namespace Cookie.Core
{
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void Hitstop(float duration, float timeScale = 0.1f)
        {
            StartCoroutine(HitstopCoroutine(duration, timeScale));
        }

        private IEnumerator HitstopCoroutine(float duration, float timeScale)
        {
            Time.timeScale = timeScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
}
