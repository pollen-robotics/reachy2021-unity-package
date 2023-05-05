using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointController : MonoBehaviour
{
    private float targetPosition;
    private bool changed;
    private ArticulationBody articulation;

    private bool compliant = false;

    void Start()
    {
        articulation = GetComponent<ArticulationBody>();
    }

    void FixedUpdate()
    {
        if (changed)
        {
            var drive = articulation.xDrive;
            drive.target = targetPosition;
            drive.stiffness = compliant ? 0 : 1000;
            articulation.xDrive = drive;

            changed = false;
        }
    }

    public void RotateTo(float newTargetPosition)
    {
        targetPosition = newTargetPosition;
        changed = true;
    }

    public void IsCompliant(bool comp)
    {
        compliant = comp;
        changed = true;
    }

    public float GetPresentPosition()
    {
        return Mathf.Rad2Deg * articulation.jointPosition[0];
    }
}
