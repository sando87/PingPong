using PP;
using System.Collections.Generic;
using UnityEngine;

public class CubePlayer : MonoBehaviour
{
    ParticleSystem ps;
    AudioSource audioSource;
    Camera cam;
    Setting Setting;

    CubeState State;

    private float TabResetTime = 0;
    private float TabPositionY = 0;
    private int DirRight = 1;
    private int TabIndex = 0;

    void Start()
    {
        Setting = GameObject.Find("SystemObject").GetComponent<Setting>();
        ps = GameObject.Find("Particle System").GetComponent<ParticleSystem>();
        audioSource = GameObject.Find("AudioPlayer").GetComponent<AudioSource>();
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
        audioSource.Stop();
        Destroy(gameObject);
        Debug.Log("Fail!!");
    }
    void DoSuccess()
    {
        audioSource.Stop();
        Destroy(gameObject);
        Debug.Log("Success!!");
    }

    void UpdateCubeGraph()
    {
        TabInfo tp = Setting.GetTapInfo(TabIndex);
        TabInfo tpNext = Setting.GetTapInfo(TabIndex + 1);
        float xx = tp.worldPos.x - transform.position.x;
        float yetTime = Setting.GetLinearT(xx); //나가는 방향으로 안쪽에 있으면 +

        float nextTime = tp.idxStepToNext * Setting.TimePerBar * 0.25f;
        float time = nextTime + yetTime;
        float dist = xx - nextTime * Setting.SpeedMoveX * DirRight;
        Setting.UpdateLinear(dist, time); //X축 방향 그래프 기울기 조정


        float offsetY = transform.position.y - tp.worldPos.y;
        float baseT = 0;
        if (tp.idxStepToNext == 4)
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = -offsetY;
            baseT = Setting.CalcBaseT(fixedH, fixedT, fixedY, false);
        }
        else if (tp.idxStepToNext == 3)
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = Setting.JumpHeightHalf - offsetY;
            baseT = Setting.CalcBaseT(fixedH, fixedT, fixedY, false);
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
            baseT = Setting.CalcBaseT(fixedH, fixedT, fixedY, true);
        }
        else
        {
            float fixedH = Setting.JumpHeight - offsetY;
            float fixedT = nextTime + yetTime;
            float fixedY = tpNext.worldPos.y - tp.worldPos.y - offsetY;
            baseT = Setting.CalcBaseT(fixedH, fixedT, fixedY, false);
        }

        float baseY = Setting.JumpHeight - offsetY;
        Setting.UpdateCurve(new Vector2(baseT, baseY), new Vector2(0, 0)); //Y축 방향 곡선 그래프 계수 조정
    }
    bool CheckPassFail()
    {
        TabPoint tp = Setting.GetTapInfo(TabIndex).script;
        float x = tp.transform.position.x - transform.position.x;
        float time = Setting.GetLinearT(x);
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
        pos.x += Setting.GetLinearX(Time.deltaTime);
        TabResetTime += Time.deltaTime;
        pos.y = TabPositionY + Setting.GetCurveY(TabResetTime);
        transform.position = pos;
        transform.Rotate(new Vector3(0, 0, -1), DirRight * Setting.SpeedRotate * Time.deltaTime);
    }
    void TouchTabPoint()
    {
        TabPoint tp = Setting.GetTapInfo(TabIndex).script;
        float x = tp.transform.position.x - transform.position.x;
        float time = Setting.GetLinearT(x);
        float tolerance = Setting.TimePerBar * Setting.RateAccuracy;
        if (Mathf.Abs(time) < tolerance)
        {
            tp.CleanHit();
        }

    }

}
