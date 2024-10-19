using UnityEngine;
using System.Collections;

namespace GamePlay.InGame.Enviorment
{
    public class AutoDestruct : MonoBehaviour
    {
        public float Time = 1.0f;
        public float Window = 0.5f;

        void Start()
        {
            StartCoroutine(Destroyer());
        }

        IEnumerator Destroyer()
        {
            yield return new WaitForSeconds(Time + Window * (Random.value - 0.5f));
            Destroy(gameObject);
        }
    }
}