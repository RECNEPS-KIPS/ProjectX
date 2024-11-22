// using Framework.Core.Manager.Event;
// using Framework.Core.Manager.Input;
// using GamePlay.Player;
// using UnityEngine;
//
// namespace GamePlay.Character
// {
//     public class InteractionController:MonoBehaviour
//     {
//         private Collider _characterCollider;
//         private Collider CharacterCollider
//         {
//             get
//             {
//                 if (_characterCollider == null)
//                 {
//                     _characterCollider = GetComponent<Collider>();
//                 }
//                 return _characterCollider;
//             }
//         }
//         private Transform CharacterColliderTrs => CharacterCollider.transform;
//         private const string LOGTag = "InteractionController";
//
//         private void Update()
//         {
//             if (CharacterCollider != null && InputManager.Instance.IsPickKeySinglePressed())
//             {
//                 PickItem();
//             }
//         }
//
//         private void PickItem()
//         {
//             // LogManager.Log(LOGTag,"IsPickKeyPressed");
//             // var list = SceneManager.Instance.CheckBounds(CharacterCollider.bounds);
//             // list.Sort((a, b) =>
//             // {
//             //     var position = CharacterColliderTrs.position;
//             //     var disA = (position - a.SelfTrs.position).sqrMagnitude;
//             //     var disB = (position - b.SelfTrs.position).sqrMagnitude;
//             //     return disA > disB ? 1 : -1;
//             // });
//             // foreach (var item in list)
//             // {
//             //     LogManager.Log(LOGTag,$"GameObject in same octree leaf node:{item.SelfTrs.name}");
//             //     var dis = (CharacterColliderTrs.position - item.SelfTrs.position).magnitude;
//             //     //按照距离找到最近的距离小于pick dis的物品
//             //     if (dis <= 1f)
//             //     {
//             //         // item
//             //         EventManager.Dispatch(EEvent.PLAYER_PICK_ITEM);
//             //         break;
//             //     }
//             // }
//         }
//     }
// }