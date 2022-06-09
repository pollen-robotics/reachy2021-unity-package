using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjInReachyFrame : MonoBehaviour
{
    public Transform obj;
    // Start is called before the first frame update

    void objInReachyFrame(Transform obj)
    {
        Vector3 localPos = transform.InverseTransformPoint(obj.position);   
        //Reachy prefab is turned 180° around Y-axis     
        Quaternion rotation = Quaternion.Euler(0, 180, 0);
        Matrix4x4 m = Matrix4x4.Rotate(rotation);
        Vector3 posrot = m.MultiplyPoint3x4(localPos);
        //Reachy is in a right handed coordinate frame
        Debug.Log("Object in Reachy frame: "+-posrot.y* transform.localScale.x+","+posrot.x* transform.localScale.y+","+-posrot.z* transform.localScale.z);
    }

    void Start()
    {
        objInReachyFrame(obj);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
