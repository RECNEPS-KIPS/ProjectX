// author:KIPKIPS
// describe:BaseUI UI面板的基类

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;
using UnityEngine.Events;
using Framework.Common;
using Framework.Core.Manager.Timer;

namespace Framework.Core.Manager.UI
{
    /// <summary>
    /// 窗口基类
    /// </summary>
    public class BaseUI : MonoBehaviour
    {
        private const string LOGTag = "BaseUI";

        /// <summary>
        /// window id
        /// </summary>
        public EUI UIId { get; set; }

        private Animator _animator;

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; private set; }

        private Canvas _canvas;

        /// <summary>
        /// 
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                _canvas ??= GetComponent<Canvas>();
                _canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Tangent |
                                                   AdditionalCanvasShaderChannels.TexCoord1 |
                                                   AdditionalCanvasShaderChannels.Normal;
                return _canvas;
            }
        }

        #region Find接口列表

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePath"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find<T>(string namePath, Action func) where T : Button
        {
            return Find<T>(namePath, transform, func);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePath"></param>
        /// <param name="trs"></param>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find<T>(string namePath, Transform trs, Action func) where T : Button
        {
            var resObj = CommonUtils.Find<T>(trs ? trs : transform, namePath);
            if (typeof(T) == typeof(Button))
            {
                //button支持绑定函数方法
                resObj.onClick.AddListener(() => { func(); });
            }

            return resObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namePath"></param>
        /// <param name="trs"></param>
        /// <param name="mouseEnter"></param>
        /// <param name="mouseExit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find<T>(string namePath, Transform trs = null, UnityAction<BaseEventData> mouseEnter = null,
            UnityAction<BaseEventData> mouseExit = null) where T : Button
        {
            var resObj = CommonUtils.Find<T>(trs ? trs : transform, namePath);
            if (mouseEnter == null && mouseExit == null)
            {
                return resObj;
            }

            if (typeof(T) == typeof(Button))
            {
                //button支持绑定函数方法
                BindBtn(resObj, mouseEnter, mouseExit);
            }

            return resObj;
        }

        #endregion

        #region Bind接口列表

        /// <summary>
        /// 
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="click"></param>
        /// <param name="mouseEnter"></param>
        /// <param name="mouseExit"></param>
        public void Bind(Button btn, Action click, UnityAction<BaseEventData> mouseEnter = null,
            UnityAction<BaseEventData> mouseExit = null)
        {
            btn.onClick.AddListener(() => { click(); });
            BindBtn(btn, mouseEnter, mouseExit);
        }

        private void BindBtn(Button btn, UnityAction<BaseEventData> mouseEnter = null,
            UnityAction<BaseEventData> mouseExit = null)
        {
            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            trigger = trigger ? trigger : btn.gameObject.AddComponent<EventTrigger>();
            // 实例化delegates(trigger.trigger是注册在EventTrigger组件上的所有功能)  
            trigger.triggers = new List<EventTrigger.Entry>();
            // 在EventSystem委托列表中进行登记 
            if (mouseEnter != null)
            {
                var enterEntry = new EventTrigger.Entry
                {
                    // 设置 事件类型  
                    eventID = EventTriggerType.PointerEnter,
                    // 实例化回调函数  
                    callback = new EventTrigger.TriggerEvent()
                };
                //UnityAction 本质上是delegate,且有数个泛型版本(参数最多是四个),一个UnityAction可以添加多个函数(多播委托)  
                //将方法绑定在回调上(给回调方法添加监听)  
                enterEntry.callback.AddListener(mouseEnter);
                // 添加事件触发记录到GameObject的事件触发组件  
                trigger.triggers.Add(enterEntry);
            }

            if (mouseExit != null)
            {
                var exitEntry = new EventTrigger.Entry
                {
                    // 设置 事件类型  
                    eventID = EventTriggerType.PointerExit,
                    // 实例化回调函数  
                    callback = new EventTrigger.TriggerEvent()
                };
                //将方法绑定在回调上(给回调方法添加监听)  
                exitEntry.callback.AddListener(mouseExit);
                // 添加事件触发记录到GameObject的事件触发组件  
                trigger.triggers.Add(exitEntry);
            }
        }

        #endregion

        #region Window life cycle

        //界面生命周期流程,这里只提供虚方法,具体的逻辑由各个业务界面进行重写

        /// <summary>
        /// 界面初始化
        /// </summary>
        public virtual void OnInit()
        {
            UIBinding = GetComponent<UIBinding>();
            LogManager.Log(LOGTag, "OnInit");
            // UIBinding.BinderDataList
            // foreach (var dict in UIBinding.BinderDataList)
            // {
            //     LogManager.Log("Layer1",dict.bindKey,dict.fieldType,dict.bindFieldId,dict.bindObj);
            // }
            BindDict.Clear();
        }

        /// <summary>
        /// 界面销毁
        /// </summary>
        public virtual void OnUnInit()
        {
            LogManager.Log(LOGTag, "OnUnInit");
            BindDict.Clear();
        }

        /// <summary>
        /// 进入界面 
        /// </summary>
        public virtual void OnEnter()
        {
            LogManager.Log(LOGTag, "OnEnter");
            IsShow = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 进入界面 
        /// </summary>
        /// <param name="options"></param>
        public virtual void OnEnter(dynamic options)
        {
            if (options == null)
            {
                OnEnter();
                return;
            }

            IsShow = true;
            gameObject.SetActive(true);
        }


        /// <summary>
        /// 暂停界面
        /// </summary>
        public virtual void OnPause()
        {
            LogManager.Log(LOGTag, "OnPause");
            IsShow = false;
        }

        /// <summary>
        /// 恢复界面
        /// </summary>
        public virtual void OnResume()
        {
            LogManager.Log(LOGTag, "OnResume");
            //print("on resume");
            IsShow = true;
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        public virtual void OnExit()
        {
            LogManager.Log(LOGTag, "OnExit");
            UIManager.UIStackPop();
            IsShow = false;
        }

        #endregion

        private UIBinding _uiBinding;

        /// <summary>
        /// 
        /// </summary>
        public UIBinding UIBinding
        {
            get => _uiBinding;
            set => _uiBinding = value;
        }

        private Dictionary<string, Bindable> BindDict = new();

        /// <summary>
        /// 更新绑定字段的值
        /// </summary>
        /// <param name="key">绑定字段</param>
        /// <param name="value">绑定的值</param>
        protected void Bind(string key, dynamic value = null)
        {
            if (value == null)
            {
                return;
            }
            BindDict[key].Value = value;
        }

        private void VMBind(string key, dynamic value = default)
        {
            if (BindDict.TryAdd(key, new Bindable(_uiBinding, key, value)))
            {
                Bind(key, value);
            }
        }

        /// <summary>
        /// 字段值绑定
        /// </summary>
        /// <param name="key">绑定字段</param>
        /// <param name="value">绑定值</param>
        protected void VBind(string key, dynamic value = default)
        {
            VMBind(key, value);
        }

        /// <summary>
        /// 字段值绑定
        /// </summary>
        /// <param name="key">绑定字段</param>
        /// <param name="value">绑定值</param>
        protected void MBind<T>(string key, UnityAction<T> value = default)
        {
            VMBind(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void MBind(string key, UnityAction value = default)
        {
            VMBind(key, value);
        }
    }
}