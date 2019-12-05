using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackCube : MonoBehaviour
{
    public Background mBackground;
    private Vector3 mRotateAxis;
    private GameObject MainCube;
    private Vector3 mMainCubePosAtFirst;
    private Vector2 mStartPos;
    private float rate = 0;
    private MeshRenderer mMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        mMeshRenderer = GetComponent<MeshRenderer>();
        float ranX = Random.Range(-1f, 1f);
        float ranY = Random.Range(-1f, 1f);
        float ranZ = Random.Range(-1f, 1f);
        mRotateAxis = new Vector3(ranX, ranY, ranZ);
        mRotateAxis.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        mMeshRenderer.material.color = mBackground.CurrentColor;
        transform.Rotate(mRotateAxis, 45 * Time.deltaTime);
    }

    private void LateUpdate()
    {
        Vector2 dir1 = MainCube.transform.position - mMainCubePosAtFirst;
        float dist = dir1.magnitude * (1 - rate);
        dir1.Normalize();
        Vector3 newPos = mStartPos + dir1 * dist;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    public void SetPosition(GameObject _mainCube, Vector3 pos)
    {
        MainCube = _mainCube;
        mMainCubePosAtFirst = MainCube.transform.position;
        mStartPos = pos;
        transform.position = pos;

        float maxZ = mBackground.transform.position.z;
        float normal = 1 - pos.z / maxZ;

        Vector3 scale = new Vector3(1, 1, 1) * normal;
        transform.localScale = scale;

        rate = (normal * 15) / maxZ; // MainCube가 100움질이면 normal만큼의 차이가 나도록하는 조정 값
    }
}
