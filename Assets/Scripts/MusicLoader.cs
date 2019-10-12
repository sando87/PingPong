using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
    public bool mIsCloude;
    public GameObject prefabListItem;
    List<GameObject> mMusics = new List<GameObject>();

    private void OnEnable()
    {
        for (int i = 0; i < mMusics.Count; ++i)
            Destroy(mMusics[i]);
        mMusics.Clear();

        if (mIsCloude)
            LoadSongFromServer();
        else
            LoadSongList();
    }

    void LoadSongList()
    {
        string path = Application.persistentDataPath;
        DirectoryInfo dir = new DirectoryInfo(path);
        FileSystemInfo[] items = dir.GetFileSystemInfos();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Extension != ".bytes")
                continue;

            byte[] bytes = File.ReadAllBytes(items[i].FullName);
            Song song = Utils.Deserialize<Song>(bytes);
            GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
            mMusics.Add(obj);
            ItemDisplay item = obj.GetComponent<ItemDisplay>();
            item.SongInfo = song;
        }


        //TextAsset[] assets = Resources.LoadAll<TextAsset>("MetaInfo/Basic");
        //foreach (TextAsset asset in assets)
        //{
        //    Song song = Utils.Deserialize<Song>(asset.bytes);
        //    GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
        //    ItemDisplay item = obj.GetComponent<ItemDisplay>();
        //    item.SongInfo = song;
        //}

    }

    void LoadSongFromServer()
    {
        ICD.CMD_MusicList msg = new ICD.CMD_MusicList();
        msg.FillHeader(ICD.ICDDefines.CMD_MusicList);
        NetworkClient.Inst().SendMsgToServer(msg);
    }

    public void OnRecvMusicList(ICD.stHeader _msg, string _info)
    {
        if (_msg.head.cmd != ICD.ICDDefines.CMD_MusicList)
            return;

        ICD.CMD_MusicList msg = (ICD.CMD_MusicList)_msg;
        for(int i = 0; i < msg.body.count; ++i)
        {
            Song song = msg.body.musics[i];
            GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
            mMusics.Add(obj);
            ItemDisplay item = obj.GetComponent<ItemDisplay>();
            item.SongInfo = song;
        }
    }
}
