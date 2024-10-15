// author:KIPKIPS
// date:2022.06.01 23:23
// describe:
using System.Collections.Generic;
using UnityEngine;
using Framework.Core.Singleton;
using System;

namespace Framework.Core.Manager.AsyncLoad {
    [MonoSingletonPath("[Manager]/AsyncLoadManager")]
    public class AsyncLoadManager : MonoSingleton<AsyncLoadManager> {
        private Dictionary<int, Node> assetsMap = new ();
        private Dictionary<int, List<Action<UnityEngine.Object>>> loadingDic = new ();
        public enum State {
            Loading,
            Finish
        }
        private class Node {
            public UnityEngine.Object asset;
            public State state;
            public ResourceRequest req;
        }
        public void Load(string path, Action<UnityEngine.Object> callback) {
            int hash = path.GetHashCode();
            if (assetsMap.ContainsKey(hash)) {
                Node node = assetsMap[hash];
                if (node.state == State.Loading) {
                    var list = loadingDic[hash];
                    if (list == null) list = new List<Action<UnityEngine.Object>>();
                    list.Add(callback);
                } else if (node.state == State.Finish) {
                    callback(node.asset);
                }
            } else {
                var list = new List<Action<UnityEngine.Object>>();
                list.Add(callback);
                loadingDic.Add(hash, list);
                Node node = new Node() {
                    state = State.Loading, req = Resources.LoadAsync<GameObject>(path)
                };
                assetsMap.Add(hash, node);
            }
        }
        public void Unload(string path) {
            int hash = path.GetHashCode();
            if (assetsMap.ContainsKey(hash)) {
                Node node = assetsMap[hash];
                if (node.state == State.Finish) {
                    UnityEngine.Object.Destroy(node.asset);
                }
                assetsMap.Remove(hash);
            }
        }
        public void Update() {
            if (assetsMap.Count > 0) {
                foreach (var item in assetsMap) {
                    Node node = item.Value;
                    if (node.state == State.Loading) {
                        if (node.req.isDone) {
                            node.state = State.Finish;
                            node.asset = node.req.asset;
                            var list = loadingDic[item.Key];
                            for (int i = 0; i < list.Count; i++) {
                                list[i](node.asset);
                            }
                            loadingDic.Remove(item.Key);
                        }
                    }
                }
            }
        }
    }
    
}
