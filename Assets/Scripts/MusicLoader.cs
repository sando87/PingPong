using ICD;
using PP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
    int mSortMethod = 0; // 이름순, 인기순, 신작순, 최근순 ...
    public GameObject prefabListItem;
    private Dictionary<int, bool> mSongIDs = new Dictionary<int, bool>();

    private void Start()
    {
        List<Song> songs = ScanSongs();
        SortSongs(songs);
        InstancateSongs(songs);

        RequestSongsFromServer();
    }

    public void AddNewSong(Song song)
    {
        GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
        obj.transform.SetAsFirstSibling();
        ItemDisplay item = obj.GetComponent<ItemDisplay>();
        item.SongInfo = song;
    }

    List<Song> ScanSongs()
    {
        List<Song> list = new List<Song>();
        if (transform.childCount == 0)
            return LoadSongList();

        for (int i = 0; i < transform.childCount; ++i)
        {
            ItemDisplay item = transform.GetChild(i).GetComponent<ItemDisplay>();
            Destroy(item.gameObject);
            if (item.SongInfo.Playable())
                list.Add(item.SongInfo);
        }
        return list;
    }
    void SortSongs(List<Song> list)
    {
        switch(mSortMethod)
        {
            case 0:
                list.Sort((left, right) => { return left.StarCount > right.StarCount ? 1 : -1; }); //인기도 순
                break;
            case 1:
            case 2:
            case 3:
            default:
                break;
        }
        //list.Sort((left, right) => { return String.Compare(left.Title, right.Title); }); //이름순(노래제목)
        //list.Sort((left, right) => { return left.CreateDate > right.CreateDate ? 1 : -1; }); //신작 순
        //list.Sort((left, right) => { return left.LastPlayDate > right.LastPlayDate ? 1 : -1; }); //최근 플레이 순
    }
    void InstancateSongs(List<Song> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
            ItemDisplay item = obj.GetComponent<ItemDisplay>();
            item.SongInfo = list[i];
        }
    }

    List<Song> LoadSongList()
    {
        string path = Application.persistentDataPath;
        DirectoryInfo dir = new DirectoryInfo(path);
        FileSystemInfo[] items = dir.GetFileSystemInfos();
        List<Song> songs = new List<Song>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Extension != ".bytes")
                continue;

            byte[] bytes = File.ReadAllBytes(items[i].FullName);
            Song song = new Song();
            Utils.Deserialize(ref song, bytes);
            Bar[] bars = new Bar[song.BarCount];
            Array.Copy(song.Bars, bars, song.BarCount);
            song.Bars = bars;
            songs.Add(song);
            mSongIDs[song.DBID] = true;
        }

        TextAsset[] assets = Resources.LoadAll<TextAsset>(PathInfo.DefaultMusics);
        foreach(TextAsset asset in assets)
        {
            Song song = new Song();
            Utils.Deserialize(ref song, asset.bytes);
            Bar[] bars = new Bar[song.BarCount];
            Array.Copy(song.Bars, bars, song.BarCount);
            song.Bars = bars;
            if(song.UserID == Defs.ADMIN_USERNAME)
                songs.Add(song);
        }

        return songs;
    }

    void RequestSongsFromServer()
    {
        ICD.CMD_MusicList msg = new ICD.CMD_MusicList();
        msg.method = mSortMethod;
        msg.FillHeader(ICD.ICDDefines.CMD_MusicList);
        NetworkClient.Inst().SendMsgToServer(msg);
        NetworkClient.Inst().mOnRecv.AddListener(OnRecvMusicList);
    }
    void OnRecvMusicList(ICD.stHeader _msg, string _info)
    {
        if (_msg.head.cmd != ICD.ICDDefines.CMD_MusicList)
            return;

        ICD.CMD_MusicList msg = (ICD.CMD_MusicList)_msg;
        for(int i = 0; i < msg.musics.Count; ++i)
        {
            int DBID = msg.musics[i].DBID;
            if (mSongIDs.ContainsKey(DBID))
                continue;

            GameObject obj = Instantiate(prefabListItem, new Vector2(0, 0), Quaternion.identity, transform);
            ItemDisplay item = obj.GetComponent<ItemDisplay>();
            item.SongInfo = msg.musics[i];
        }
        NetworkClient.Inst().mOnRecv.RemoveListener(OnRecvMusicList);
    }
}
