// author:KIPKIPS
// describe:加密float类型

using System;

namespace Framework.Core.Manager.AnitCheat
{
    /// <summary>
    /// 加密浮点数
    /// </summary>
    public struct EncryptFloat
    {
        private float _obscuredFloat;
        private int _obscuredKey;

        private float _originalValue;

        // 封装的加密解密数值
        internal float Value
        {
            get
            {
                float result = _obscuredFloat - _obscuredKey;
                if (!_originalValue.Equals(result))
                {
                    AntiCheatManager.Instance.Detected();
                }

                return result;
            }
            set
            {
                _originalValue = value;
                unchecked
                {
                    _obscuredKey = EncryptRandom.RandomNum(int.MaxValue - Convert.ToInt32(value));
                    _obscuredFloat = value + _obscuredKey;
                }
            }
        }

        /// <summary>
        /// 加密浮点数构造函数
        /// </summary>
        /// <param name="val"></param>
        public EncryptFloat(float val = 0)
        {
            _obscuredFloat = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }

        public static implicit operator EncryptFloat(float val) => new EncryptFloat(val);

        public static implicit operator float(EncryptFloat val) => val.Value;

        //重载操作符 加减乘除求余数 全等不等 大于小于
        public static bool operator ==(EncryptFloat a, EncryptFloat b) => a.Value == b.Value;
        public static bool operator ==(EncryptFloat a, float b) => a.Value == b;
        public static bool operator !=(EncryptFloat a, EncryptFloat b) => a.Value != b.Value;
        public static bool operator !=(EncryptFloat a, float b) => a.Value != b;
        public static EncryptFloat operator +(EncryptFloat a, EncryptFloat b) => new EncryptFloat(a.Value + b.Value);
        public static EncryptFloat operator +(EncryptFloat a, float b) => new EncryptFloat(a.Value + b);
        public static EncryptFloat operator -(EncryptFloat a, EncryptFloat b) => new EncryptFloat(a.Value - b.Value);
        public static EncryptFloat operator -(EncryptFloat a, float b) => new EncryptFloat(a.Value - b);
        public static EncryptFloat operator *(EncryptFloat a, EncryptFloat b) => new EncryptFloat(a.Value * b.Value);
        public static EncryptFloat operator *(EncryptFloat a, float b) => new EncryptFloat(a.Value * b);
        public static EncryptFloat operator /(EncryptFloat a, EncryptFloat b) => new EncryptFloat(a.Value / b.Value);
        public static EncryptFloat operator /(EncryptFloat a, float b) => new EncryptFloat(a.Value / b);
        public static EncryptFloat operator %(EncryptFloat a, EncryptFloat b) => new EncryptFloat(a.Value % b.Value);
        public static EncryptFloat operator %(EncryptFloat a, float b) => new EncryptFloat(a.Value % b);
        public static bool operator ==(EncryptFloat a, int b) => a.Value == b;
        public static bool operator ==(EncryptFloat a, EncryptInt b) => a.Value == b.Value;
        public static bool operator !=(EncryptFloat a, int b) => a.Value != b;
        public static bool operator !=(EncryptFloat a, EncryptInt b) => a.Value != b.Value;
        public static EncryptFloat operator +(EncryptFloat a, int b) => new EncryptFloat(a.Value + b);
        public static EncryptFloat operator +(EncryptFloat a, EncryptInt b) => new EncryptFloat(a.Value + b.Value);
        public static EncryptFloat operator -(EncryptFloat a, int b) => new EncryptFloat(a.Value - b);
        public static EncryptFloat operator -(EncryptFloat a, EncryptInt b) => new EncryptFloat(a.Value - b.Value);
        public static EncryptFloat operator *(EncryptFloat a, int b) => new EncryptFloat(a.Value * b);
        public static EncryptFloat operator *(EncryptFloat a, EncryptInt b) => new EncryptFloat(a.Value * b.Value);
        public static EncryptFloat operator /(EncryptFloat a, int b) => new EncryptFloat(a.Value / b);
        public static EncryptFloat operator /(EncryptFloat a, EncryptInt b) => new EncryptFloat(a.Value / b.Value);
        public static EncryptFloat operator %(EncryptFloat a, int b) => new EncryptFloat(a.Value % b);

        public static EncryptFloat operator %(EncryptFloat a, EncryptInt b) => new EncryptFloat(a.Value % b.Value);

        //重载ToString,GetHashCode,Equals
        public override string ToString() => Value.ToString();
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => Value.Equals(obj is EncryptFloat ? ((EncryptFloat)obj).Value : obj);
    }
}