using PP;
using System.Collections.Generic;
using UnityEngine;

public class CubePlayer : MonoBehaviour
{
    ParticleSystem ps;
    Camera cam;
    SystemManager SysMgr;

    CubeState State;

    private float TabResetTime = 0;
    private float TabPositionY = 0;
    private int DirRight = 1;
    private int TabIndex = 0;

    void Start()
    {
        SysMgr = GameObject.Find("SystemObject").GetComponent<SystemManager>();
        ps = GameObject.Find("Particle System").GetComponent<ParticleSystem>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        State = CubeState.Ready;
    }

    void Update()
    {
        switch(State)
        {
            case CubeState.Ready: break;
            case CubeState.Jump: DoingJump(); break;
            case CubeState.Fail: DoFail();  break;
            case CubeState.Success: DoSuccess();  break;
            default: break;
        }
    }

    public void ResetCube()
    {
        State = CubeState.Ready;
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;
        MoveCamera();
        TabResetTime = 0;
        TabPositionY = 0;
        DirRight = 1;
        TabIndex = 0;
        gameObject.SetActive(true);
    }
    public void StartJump()
    {
        State = CubeState.Jump;
    }
    void DoingJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (CheckPassFail())
                return;

            UpdateCubeGraph();

            TouchTabPoint();

            ps.transform.position = transform.position;
            ps.Play();

            TabResetTime = 0;
            TabPositionY = transform.position.y;
            DirRight *= -1;
            TabIndex++;
        }

        MoveCube();
        MoveCamera();
    }
    void DoFail()
    {
        State = CubeState.Ready;
        SysMgr.StopJump();
        gameObject.SetActive(false);
    }
    void DoSuccess()
    {
        State = CubeState.Ready;
        SysMgr.StopJump();
        gameObject.SetActive(false);
    }

    void UpdateCubeGraph()
    {
        TabInfo tp = SysMgr.GetTapInfo(TabIndex);
        TabInfo tpNext = SysMgr.GetTapInfo(TabIndex + 1);
        float xx = tp.worldPos.x - transform.position.x;
        float yetTime = Curve.GetLinearT(xx); //나가는 방향으로 안쪽에 있으면 +

        float nextTime = tp.idxStepToNext * Setting.TimePerBar * 0.25f;
        float time = nextTime + yetTime;
        float dist = xx - nextTime * Setting.SpeedMoveX * DirRight;
        Curve.UpdateLinear(dist, time); //X축 방향 그래프 기울기 조정


        float offsetY = transform.position.y - tp.worldPos.y;
        float baseT = 0;
        if (tp.idxStepToNext == 4)
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = -offsetY;
            baseT = Curve.CalcBaseT(fixedH, fixedT, fixedY, false);
        }
        else if (tp.idxStepToNext == 3)
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = Setting.JumpHeightHalf - offsetY;
            baseT = Curve.CalcBaseT(fixedH, fixedT, fixedY, false);
        }
        else if (tp.idxStepToNext == 2)
        {
            baseT = (nextTime + yetTime);
        }
        else if (tp.idxStepToNext == 1)
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = Setting.JumpHeightHalf - offsetY;
            baseT = Curve.CalcBaseT(fixedH, fixedT, fixedY, true);
        }
        else
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = tpNext.worldPos.y - tp.worldPos.y - offsetY;
            baseT = Curve.CalcBaseT(fixedH, fixedT, fixedY, false);
        }

        float baseY = Setting.JumpHeight - offsetY;
        Curve.UpdateCurve(new Vector2(baseT, baseY), new Vector2(0, 0)); //Y축 방향 곡선 그래프 계수 조정
    }
    bool CheckPassFail()
    {
        TabPoint tp = SysMgr.GetTapInfo(TabIndex).script;
        float x = tp.transform.position.x - transform.position.x;
        float time = Curve.GetLinearT(x);
        float tolerance = Setting.TimePerBar * Setting.RatePassFail;
        if (Mathf.Abs(time) > tolerance)
        {
            State = CubeState.Fail;
            return true;
        }
        else if (tp.IsFinalTab())
        {
            State = CubeState.Success;
            return true;
        }
        return false;
    }
    void MoveCamera()
    {
        Vector3 newPos = transform.position;
        newPos.z = cam.transform.position.z;
        cam.transform.position = newPos;
        return;
    }
    void MoveCube()
    {
        Vector3 pos = transform.position;
        pos.x += Curve.GetLinearX(Time.deltaTime);
        TabResetTime += Time.deltaTime;
        pos.y = TabPositionY + Curve.GetCurveY(TabResetTime);
        transform.position = pos;
        transform.Rotate(new Vector3(0, 0, -1), DirRight * Setting.SpeedRotate * Time.deltaTime);
    }
    void TouchTabPoint()
    {
        TabPoint tp = SysMgr.GetTapInfo(TabIndex).script;
        float x = tp.transform.position.x - transform.position.x;
        float time = Curve.GetLinearT(x);
        float tolerance = Setting.TimePerBar * Setting.RateAccuracy;
        if (Mathf.Abs(time) < tolerance)
        {
            tp.CleanHit();
        }

    }
}
