using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackCube : MonoBehaviour
{
    public GameObject MainCube;
    Vector3 RotateAxis;
    Vector3 PosFromMainCube;
    public int Depth;

    // Start is called before the first frame update
    void Start()
    {
        RotateAxis = new Vector3();
        RotateAxis.x = Random.Range(-1, 1);
        RotateAxis.y = Random.Range(-1, 1);
        RotateAxis.z = Random.Range(-1, 1);
        RotateAxis.Normalize();
        PosFromMainCube = transform.position - MainCube.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //PosFromMainCube.y -= (0.1f * Time.deltaTime);
        Vector3 offPos = MainCube.transform.position * Depth * 0.1f;
        //transform.position = MainCube.transform.position + PosFromMainCube;
        //transform.position -= offPos;
        transform.Rotate(RotateAxis, 1.0f);
    }
}
