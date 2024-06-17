using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {
        var mat = GetComponent<Renderer>().material;
        mat.SetFloat("_BaseMap", 0.5f);
    }
}
