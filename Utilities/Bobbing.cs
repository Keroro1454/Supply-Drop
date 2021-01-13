using UnityEngine;
using System.Collections;

// Makes objects float up & down while gently spinning.
namespace K1454.Utils
{
    public class Bobbing : MonoBehaviour
    {
        public float degreesPerSecond = 15.0f;
        public float amplitude = 0.3f;
        public float frequency = 1f;

        Vector3 posOffset = new Vector3();
        Vector3 tempPos = new Vector3();

        void Start()
        {
            posOffset = transform.position;
        }
        void Update()
        {
            if (Time.deltaTime == 0f | Time.fixedTime == 0f)
            {
                return;
            }
            transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);
            tempPos = posOffset;
            tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

            transform.position = tempPos;
        }
    }
}
