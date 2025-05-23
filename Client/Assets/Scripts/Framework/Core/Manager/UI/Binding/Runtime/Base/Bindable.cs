// author:KIPKIPS
// date:2023.04.17 23:27
// describe:可绑定处理

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Core.Manager.UI
{
    public class Bindable
    {
        //保存真正的值
        private dynamic _value;

        private bool _isInit;

        //get时返回真正的值,set时顺便调用值改变事件
        public dynamic Value
        {
            get => _value;
            set
            {
                if (!_isInit && _value is not null && Equals(value, _value)) return;
                _value = value;
                _isInit = false;
                OnValueChanged?.Invoke(value);
            }
        }

        //用event存储值改变的事件
        public event Action<dynamic> OnValueChanged;
        private readonly string _key;

        private readonly UIBinding _uiBinding;

        //初始化
        public Bindable(UIBinding uiBinding, string key, dynamic value)
        {
            OnValueChanged = UpdateBind;
            _value = value;
            _isInit = true;
            _key = key;
            _uiBinding = uiBinding;
        }

        private void UpdateBind(dynamic value)
        {
            if (!_uiBinding.BinderDataDict.TryGetValue(_key, out var data)) return;
            var baseBinder = UIBinding.GetBaseBinder(UIBinding.GetType(data.bindObj));
            switch ((LinkerType)data.fieldType)
            {
                case LinkerType.Vector2:
                    if (value is Vector2 v2Value) baseBinder.SetVector2(data.bindObj, data.bindFieldId, v2Value);
                    break;
                case LinkerType.Vector3:
                    if (value is Vector3 v3Value) baseBinder.SetVector3(data.bindObj, data.bindFieldId, v3Value);
                    break;
                case LinkerType.Quaternion:
                    if (value is Quaternion quaternionValue)
                        baseBinder.SetQuaternion(data.bindObj, data.bindFieldId, quaternionValue);
                    break;
                case LinkerType.Boolean:
                    if (value is bool boolValue) baseBinder.SetBoolean(data.bindObj, data.bindFieldId, boolValue);
                    break;
                case LinkerType.Int32:
                    if (value is int intValue) baseBinder.SetInt32(data.bindObj, data.bindFieldId, intValue);
                    break;
                case LinkerType.String:
                    if (value is string stringValue) baseBinder.SetString(data.bindObj, data.bindFieldId, stringValue);
                    break;
                case LinkerType.Single:
                    if (value is float floatValue) baseBinder.SetSingle(data.bindObj, data.bindFieldId, floatValue);
                    break;
                case LinkerType.Color:
                    if (value is Color colorValue) baseBinder.SetColor(data.bindObj, data.bindFieldId, colorValue);
                    break;
                case LinkerType.Sprite:
                    if (value is Sprite spriteValue) baseBinder.SetSprite(data.bindObj, data.bindFieldId, spriteValue);
                    break;
                case LinkerType.Char:
                    if (value is char charValue) baseBinder.SetChar(data.bindObj, data.bindFieldId, charValue);
                    break;
                case LinkerType.Rect:
                    if (value is Rect rectValue) baseBinder.SetRect(data.bindObj, data.bindFieldId, rectValue);
                    break;
                case LinkerType.Object:
                    if (value is object objValue) baseBinder.SetSystemObject(data.bindObj, data.bindFieldId, objValue);
                    break;
                case LinkerType.UnityAction:
                    if (value is UnityAction action) baseBinder.SetAction(data.bindObj, data.bindFieldId, action);
                    break;
                case LinkerType.UnityActionVector2:
                    if (value is UnityAction<Vector2> actionVector2)
                        baseBinder.SetActionVector2(data.bindObj, data.bindFieldId, actionVector2);
                    break;
                case LinkerType.RectTransform:
                    break;
                case LinkerType.UIBinding:
                    break;
                default:
                    LogManager.LogWarning("Failure Binding",
                        $"Unregistered binding type : {((LinkerType)data.fieldType).ToString()}");
                    break;
            }
        }
    }
}