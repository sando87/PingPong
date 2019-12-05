using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public Text Title;
    public Text Message;
    public delegate void ResultMB(bool isOK);
    public ResultMB mResultMB;
    
    public static void Show(string title, string message, ResultMB func)
    {
        GameObject parent = GameObject.Find("RootUI");
        GameObject prefab = Resources.Load< GameObject>("Prefabs/pnMessageBox");
        GameObject obj = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity, parent.transform);
        obj.transform.SetAsLastSibling();
        MessageBox script = obj.GetComponent<MessageBox>();
        script.Title.text = title;
        script.Message.text = message;
        script.mResultMB = func;
    }
    public void OnOK()
    {
        mResultMB?.Invoke(true);
        Destroy(gameObject);
    }
    public void OnCancle()
    {
        mResultMB?.Invoke(false);
        Destroy(gameObject);
    }
}
