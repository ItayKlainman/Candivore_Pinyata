using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public string key;
        public GameObject prefab;
        public int initialSize = 10;
    }

   [SerializeField] public List<PoolEntry> entries;
    private Dictionary<string, Queue<GameObject>> poolDict = new();

    public static ObjectPool Instance;

    private void Awake()
    {
        Instance = this;

        foreach (var entry in entries)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict[entry.key] = queue;
        }
    }

    public GameObject GetFromPool(string key, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with key '{key}' not found.");
            return null;
        }

        var obj = poolDict[key].Count > 0 ? poolDict[key].Dequeue()
            : Instantiate(GetPrefab(key));

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        if (poolDict.ContainsKey(key))
        {
            poolDict[key].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    private GameObject GetPrefab(string key)
    {
        foreach (var entry in entries)
        {
            if (entry.key == key) return entry.prefab;
        }
        return null;
    }
}