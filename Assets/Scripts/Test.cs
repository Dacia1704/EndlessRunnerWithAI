using System;
using UnityEngine;

public class Test: MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Test");
        }
    }
}