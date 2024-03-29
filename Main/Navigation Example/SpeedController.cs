using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour
{
    // Public variables
    [Range(0f, 6f)]
    public float    Speed = 0.0f;

    // Private variables
    private Animator _controller = null;

    private void Start()
    {
        _controller = GetComponent<Animator>();
    }

    private void Update()
    {
        _controller.SetFloat("Speed", Speed);
    }
}
