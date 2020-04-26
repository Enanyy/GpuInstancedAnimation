using UnityEngine;
using System.Collections;

public class SampleRotate : MonoBehaviour
{
    public float rotateSpeed = 100;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
