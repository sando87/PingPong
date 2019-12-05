using UnityEngine;

public class ExitApp : MonoBehaviour
{
    int ClickCount = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClickCount++;
            if (!IsInvoking("ResetDoubleClick"))
                Invoke("ResetDoubleClick", 1.0f);

        }
        else if (ClickCount >= 2)
        {
            ICD.CMD_UserInfo info = new ICD.CMD_UserInfo();
            info.body.username = Setting.Inst().UserName;
            info.body.reserve = 0; //LOGOUT
            info.FillHeader(ICD.ICDDefines.CMD_LoggingUser);
            NetworkClient.Inst().SendMsgToServer(info);

            CancelInvoke("ResetDoubleClick");
            Application.Quit();
        }

    }

    void ResetDoubleClick()
    {
        ClickCount = 0;
    }
}
