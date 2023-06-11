using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSimulation : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 windDirection = Vector3.forward;
    public float windForce = 1f;

    private Rigidbody clothRigidbody;


    private void Start()
    {
        clothRigidbody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        Vector3 wind = windDirection * windForce;
        clothRigidbody.AddForce(wind);
    }

   
}
