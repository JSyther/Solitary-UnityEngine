using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestruct : MonoBehaviour
{

    [SerializeField] private float _time = 10.0f;

    // Use this for initialization
    void Awake()
    {
        Invoke( "DestroyNow", _time );
    }

    void DestroyNow()
    {
        DestroyObject (gameObject);
    }
}

