using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeAllWave()
    {
        foreach (var e in transform.GetComponentsInChildren<Axolittle>() )
        {
            e.Wave();
        }
    }
}
