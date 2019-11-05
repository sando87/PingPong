using UnityEngine;

public class Setting : MonoBehaviour
{
    public static float TimePerBar = 0;
    public static float JumpHeightHalf = 0;

    public static float JumpHeight = 10f;
    public static float SpeedMoveX = 10f;
    public static float SpeedRotate = 210f;
    public static float RatePassFail = 0.125f; //0, 0.03125f, 0.0625f, 0.125f, 0.25f, 0.5f, 1.0f
    public static float RateAccuracy = 0.0625f;
    public static float MusicDelay = 0;

    public void SetMusicDelay(float value)
    {
        MusicDelay = value; // -0.5f ~ 0.5f
    }
}
