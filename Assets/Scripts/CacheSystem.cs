using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CacheSystem<ItemType>
{
    private const int MAX_CACHE_COUNT = 50;
    public class CacheInfo
    {
        public long time;
        public string key;
        public ItemType item;
    }
    Dictionary<string, CacheInfo> mCache = new Dictionary<string, CacheInfo>();

    public ItemType CacheOrLoad(string fullname, Func<string, ItemType> LoadItem)
    {
        string key = fullname;
        if (!mCache.ContainsKey(key))
        {
            CacheInfo info = new CacheInfo();
            info.time = DateTime.Now.Ticks;
            info.key = key;
            info.item = LoadItem(key);
            mCache[key] = info;

            if (mCache.Count >= MAX_CACHE_COUNT)
                ReleaseItems(mCache.Count / 2);
        }
        else
        {
            mCache[key].time = DateTime.Now.Ticks;
        }

        return mCache[key].item;
    }

    private void ReleaseItems(int count)
    {
        var list = mCache.Values.ToList();
        list.Sort((lsb, rsb) => { return (lsb.time > rsb.time) ? 1 : ((lsb.time < rsb.time) ? -1 : 0); });
        foreach (var item in list)
        {
            mCache.Remove(item.key);
            count--;
            if (count <= 0)
                break;
        }

    }

}
