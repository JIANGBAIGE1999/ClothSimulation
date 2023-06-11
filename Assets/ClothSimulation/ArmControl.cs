using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmControl : MonoBehaviour
{
    public Transform endEffector;  // 机械臂的末端部分

    public float rotationSpeed = 100f;  // 旋转速度

    private bool rotateClockwise = false;
    private bool rotateCounterClockwise = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            rotateClockwise = true;
            rotateCounterClockwise = false;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rotateCounterClockwise = true;
            rotateClockwise = false;
        }
        else
        {
            rotateClockwise = false;
            rotateCounterClockwise = false;
        }

        if (rotateClockwise)
        {
            RotateClockwise();
        }
        else if (rotateCounterClockwise)
        {
            RotateCounterClockwise();
        }
    }

    void RotateClockwise()
    {
        float rotationAmount = rotationSpeed * Time.deltaTime;
        endEffector.Rotate(Vector3.up, rotationAmount);
    }

    void RotateCounterClockwise()
    {
        float rotationAmount = -rotationSpeed * Time.deltaTime;
        endEffector.Rotate(Vector3.up, rotationAmount);
    }
}


