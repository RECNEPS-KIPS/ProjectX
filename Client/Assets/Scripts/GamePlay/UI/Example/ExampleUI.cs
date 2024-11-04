// author:KIPKIPS
// date:2023.04.13 17:30
// describe:示例界面

using Framework;
using Framework.Core.Manager.UI;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.UI
{
    public class ExampleUI : BaseUI
    {
        public override void OnInit()
        {
            base.OnInit();
            VBind("profession1");
            VBind("profession2");
            VBind("profession3"); /*MBind("onDe",() => LogManager.Log("onLast"));*/
            VBind("profession4"); //VBind("profession4");

            MBind("closeBtn", () => { UIManager.Close(EUI.ExampleUI); });
            /*MBind("onDe",() => LogManager.Log("onLast"));*/
            MBind("onLast", () => LogManager.Log("onLast"));
            MBind("onNext", () => LogManager.Log("onNext"));
            MBind<Vector2>("onDragBegin", OnDragBegin);
            MBind<Vector2>("onDrag", OnDrag);
            MBind<Vector2>("onDragEnd", OnDragEnd);
        }


        private float _beginRotation;
        private float _curRotateY;

        void OnDragBegin(Vector2 pos)
        {
            _beginRotation = pos.x;
            _curRotateY = 0;
        }

        void OnDrag(Vector2 pos)
        {
            Bind("modelRot", new Vector3(0, _curRotateY - (pos.x - _beginRotation) * 0.2f, 0));
        }

        void OnDragEnd(Vector2 pos)
        {
            _beginRotation = 0;
            _curRotateY = 0;
        }

        public override void OnEnter(dynamic args)
        {
            base.OnEnter();
            LogManager.Log("TestUI -> OnEnter");
            Bind("profession1", "战士");
            Bind("profession2", "刺客");
            Bind("profession3", "坦克");
            Bind("profession4", "射手");

            // Bind("modelPath","Assets/ResourcesAssets/Character/pre_liam.prefab");
        }
    }
}