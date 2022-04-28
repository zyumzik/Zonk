using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    /// <summary>
    /// Dice's points value
    /// </summary>
    public int Value
    {
        get
        {
            return value;
        }
        set
        {
            this.value = value;
            RotateToValue();          
        }
    }

    private int value;

    /// <summary>
    /// Game object which visualizes if dice selected
    /// </summary>
    public GameObject Selection;

    /// <summary>
    /// Set gameObject's rotation for {Value}
    /// </summary>
    public void RotateToValue()
    {
        float xRot = 0;
        float zRot = 0;
        switch (Value)
        {
            case 1:
                zRot = -90;
                break;
            case 2:
                // in right rotation
                break;
            case 3:
                zRot = 90;
                break;
            case 4:
                zRot = 180;
                break;
            case 5:
                xRot = -90;
                break;
            case 6:
                xRot = 90;
                break;
        }
        transform.rotation = Quaternion.Euler(
            xRot, Random.Range(0, 360), zRot);
    }
}
