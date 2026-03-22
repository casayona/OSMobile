using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace speedometer
{
public class Speedometer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float Value;         //This is the variable necessary to modify to change the parameter of the speed, it has a range of 0 to 1

    public int MaxAngle=270;    //This variable adjusts the maximum point to where the needle reaches

    public Transform needleArea;
    public Slider slider;

    void Update()
    {
        Value = slider.value;
        needleArea.eulerAngles = new Vector3(0, 0, Value*-MaxAngle);
    }
}
}