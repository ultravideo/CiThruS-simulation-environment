using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleObjectSpawner : MonoBehaviour
{

    public GameObject go;
    public int spawnCount = 100;
    public float offset = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        int scale = Mathf.RoundToInt(Mathf.Sqrt(spawnCount));
        for (int i = 0; i < scale; ++i)
        {
            for (int j = 0; j < scale; ++j)
            {
                Vector3 pos = gameObject.transform.position;
                pos.x += i * offset;
                pos.z += j * offset;
                Instantiate(go, pos, Quaternion.identity);
            }
        }
    }
}
