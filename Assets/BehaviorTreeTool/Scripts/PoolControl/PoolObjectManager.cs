using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DataControl.ObjectKeyControl;
using UnityEngine;

namespace PoolObjectControl
{
    [Serializable]
    public class Pool
    {
#if UNITY_EDITOR
        [HideInInspector] public string poolObjectName;
#endif
        public PoolObjectKey poolObjectKey;
        public GameObject prefab;
        public byte initSize;
    }

    public class PoolObjectManager : MonoBehaviour
    {
        private static PoolObjectManager _inst;
        private Camera _cam;
        private Dictionary<PoolObjectKey, Stack<GameObject>> _poolStackTable;
        private Dictionary<PoolObjectKey, GameObject> _prefabTable;

        [SerializeField] private Transform canvas;
        [SerializeField] private byte poolMaxSize;
        [SerializeField] private Pool[] pools;

        private void Start()
        {
            _inst = this;
            _cam = Camera.main;
            _poolStackTable = new Dictionary<PoolObjectKey, Stack<GameObject>>();
            _prefabTable = new Dictionary<PoolObjectKey, GameObject>();
            PoolInit();
        }

#if UNITY_EDITOR

        [ContextMenu("Set Prefab key from Pool")]
        private void MatchPoolKeyToPrefabKey()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                var t = pools[i];
                t.poolObjectName = t.poolObjectKey.ToString();
                t.prefab.GetComponent<PoolObject>().poolObjKey = t.poolObjectKey;
            }
        }
#endif
        private void PoolInit()
        {
            for (var i = 0; i < pools.Length; i++)
            {
                var t = pools[i];
                _prefabTable.Add(t.poolObjectKey, t.prefab);
                t.prefab.GetComponent<PoolObject>().poolObjKey = t.poolObjectKey;

                if (t.prefab == null) throw new Exception($"{t.poolObjectKey} doesn't exist");
                _poolStackTable.Add(t.poolObjectKey, new Stack<GameObject>());
                for (var j = 0; j < t.initSize; j++)
                {
                    CreateNewObject(t.poolObjectKey, t.prefab);
                }
            }
        }

        public static void Get(PoolObjectKey poolPoolObjectKey, Transform t) =>
            _inst.Spawn(poolPoolObjectKey, t.position, t.rotation);

        public static void Get(PoolObjectKey poolPoolObjectKey, Vector3 position) =>
            _inst.Spawn(poolPoolObjectKey, position, Quaternion.identity);

        public static void Get(PoolObjectKey poolPoolObjectKey, Vector3 position, Quaternion rotation) =>
            _inst.Spawn(poolPoolObjectKey, position, rotation);

        public static T Get<T>(PoolObjectKey poolPoolObjectKey, Transform t) where T : Component
        {
            var obj = _inst.Spawn(poolPoolObjectKey, t.position, t.rotation);
            obj.TryGetComponent(out T component);
            return component;
        }

        public static T Get<T>(PoolObjectKey poolPoolObjectKey, Vector3 position) where T : Component
        {
            var obj = _inst.Spawn(poolPoolObjectKey, position, Quaternion.identity);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolPoolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }

        public static T Get<T>(PoolObjectKey poolPoolObjectKey, Vector3 position, Quaternion rotation)
            where T : Component
        {
            var obj = _inst.Spawn(poolPoolObjectKey, position, rotation);
#if UNITY_EDITOR
            if (obj.TryGetComponent(out T component)) return component;
            obj.SetActive(false);
            throw new Exception($"{poolPoolObjectKey} Component not found");
#else
            obj.TryGetComponent(out T component);
            return component;
#endif
        }


        public static void ReturnToPool(GameObject obj, PoolObjectKey poolObjKey)
        {
            if (!_inst._poolStackTable.TryGetValue(poolObjKey, out var poolStack)) return;
            poolStack.Push(obj);
        }

        public static async UniTaskVoid PoolCleaner()
        {
            foreach (var poolObjectKey in _inst._poolStackTable.Keys)
            {
                var pool = _inst._poolStackTable[poolObjectKey];
                if (pool.Count > _inst.poolMaxSize)
                {
                    var itemToRemoveCount = pool.Count - _inst.poolMaxSize;
                    for (var i = 0; i < itemToRemoveCount; i++)
                    {
                        Destroy(pool.Pop());
                        await UniTask.Delay(1000, cancellationToken: _inst.GetCancellationTokenOnDestroy());
                    }
                }
            }
        }


        private GameObject Spawn(PoolObjectKey poolObjKey, Vector3 position, Quaternion rotation)
        {
            var poolStack = _poolStackTable[poolObjKey];
            if (poolStack.Count <= 0)
            {
                if (_prefabTable.TryGetValue(poolObjKey, out var prefab))
                {
                    CreateNewObject(poolObjKey, prefab);
                }
            }

            var poolObj = poolStack.Pop();
            poolObj.transform.SetPositionAndRotation(position, rotation);
            poolObj.SetActive(true);
            return poolObj;
        }


        private void CreateNewObject(PoolObjectKey poolPoolObjectKey, GameObject prefab)
        {
            var obj = Instantiate(prefab, transform);
            if (obj.TryGetComponent(out PoolObject poolObject)) poolObject.poolObjKey = poolPoolObjectKey;
            obj.SetActive(false);
#if UNITY_EDITOR
            if (!obj.TryGetComponent(out PoolObject _))
                throw new Exception($"You have to attach PoolObject.cs in {prefab} prefab");
            obj.name = poolPoolObjectKey.ToString();
#endif
        }

#if UNITY_EDITOR
        private void SortObject(GameObject obj)
        {
            var isFind = false;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (i == transform.childCount - 1)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }

                if (transform.GetChild(i).name == obj.name) isFind = true;
                else if (isFind)
                {
                    obj.transform.SetSiblingIndex(i);
                    break;
                }
            }
        }

#endif
    }
}