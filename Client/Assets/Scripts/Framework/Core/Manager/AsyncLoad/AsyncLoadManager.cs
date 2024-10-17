// // author:KIPKIPS
// // date:2022.06.01 23:23
// // describe:
//
// using System.Collections.Generic;
// using UnityEngine;
// using Framework.Core.Singleton;
// using System;
// using Object = UnityEngine.Object;
//
// namespace Framework.Core.Manager.AsyncLoad
// {
//     /// <summary>
//     /// 异步加载管理器
//     /// </summary>
//     [MonoSingletonPath("[Manager]/AsyncLoadManager")]
//     public class AsyncLoadManager : MonoSingleton<AsyncLoadManager>
//     {
//         private readonly Dictionary<int, Node> _assetsMap = new();
//         private readonly Dictionary<int, List<Action<Object>>> _loadingDic = new();
//
//         /// <summary>
//         /// 异步加载任务状态
//         /// </summary>
//         private enum State
//         {
//             Loading,
//             Finish
//         }
//
//         private class Node
//         {
//             public Object Asset;
//             public State State;
//             public ResourceRequest Req;
//         }
//
//         /// <summary>
//         /// 加载任务
//         /// </summary>
//         /// <param name="path"></param>
//         /// <param name="callback"></param>
//         public void Load(string path, Action<Object> callback)
//         {
//             var hash = path.GetHashCode();
//             if (_assetsMap.TryGetValue(hash, out var existNode))
//             {
//                 switch (existNode.State)
//                 {
//                     case State.Loading:
//                     {
//                         var list = _loadingDic[hash] ?? new List<Action<Object>>();
//                         list.Add(callback);
//                         break;
//                     }
//                     case State.Finish:
//                         callback(existNode.Asset);
//                         break;
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//             }
//             else
//             {
//                 var list = new List<Action<Object>> { callback };
//                 _loadingDic.Add(hash, list);
//                 var node = new Node
//                 {
//                     State = State.Loading, Req = Resources.LoadAsync<GameObject>(path)
//                 };
//                 _assetsMap.Add(hash, node);
//             }
//         }
//
//         /// <summary>
//         /// 卸载
//         /// </summary>
//         /// <param name="path"></param>
//         public void Unload(string path)
//         {
//             var hash = path.GetHashCode();
//             if (!_assetsMap.ContainsKey(hash)) return;
//             var node = _assetsMap[hash];
//             if (node.State == State.Finish)
//             {
//                 Destroy(node.Asset);
//             }
//
//             _assetsMap.Remove(hash);
//         }
//
//         /// <summary>
//         /// 
//         /// </summary>
//         public void Update()
//         {
//             if (_assetsMap.Count <= 0) return;
//             foreach (var item in _assetsMap)
//             {
//                 var node = item.Value;
//                 if (node.State != State.Loading) continue;
//                 if (!node.Req.isDone) continue;
//                 node.State = State.Finish;
//                 node.Asset = node.Req.asset;
//                 var list = _loadingDic[item.Key];
//                 foreach (var t in list)
//                 {
//                     t(node.Asset);
//                 }
//
//                 _loadingDic.Remove(item.Key);
//             }
//         }
//     }
// }