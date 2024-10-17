// author:KIPKIPS
// describe:加密int类型

namespace Framework.Core.Manager.AnitCheat
{
    /// <summary>
    /// 加密整型
    /// </summary>
    public struct EncryptInt
    {
        private int _obscuredInt;
        private int _obscuredKey;

        private int _originalValue;

        // 封装的加密解密数值
        internal int Value
        {
            get
            {
                var result = _obscuredInt ^ _obscuredKey;
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
                    _obscuredKey = EncryptRandom.RandomNum(int.MaxValue - value);
                    _obscuredInt = value ^ _obscuredKey;
                }
            }
        }

        /// <summary>
        /// 加密整型构造函数
        /// </summary>
        /// <param name="val"></param>
        public EncryptInt(int val = 0)
        {
            _obscuredInt = 0;
            _obscuredKey = 0;
            _originalValue = 0;
            Value = val;
        }

        public static implicit operator EncryptInt(int val) => new EncryptInt(val);

        public static implicit operator int(EncryptInt val) => val.Value;

        //重载操作符 加减乘除求余数 全等不等 大于小于
        public static bool operator ==(EncryptInt a, EncryptInt b) => a.Value == b.Value;
        public static bool operator ==(EncryptInt a, int b) => a.Value == b;
        public static bool operator !=(EncryptInt a, EncryptInt b) => a.Value != b.Value;
        public static bool operator !=(EncryptInt a, int b) => a.Value != b;

        public static EncryptInt operator ++(EncryptInt a)
        {
            a.Value++;
            return a;
        }

        public static EncryptInt operator --(EncryptInt a)
        {
            a.Value--;
            return a;
        }

        public static EncryptInt operator +(EncryptInt a, EncryptInt b) => new EncryptInt(a.Value + b.Value);
        public static EncryptInt operator +(EncryptInt a, int b) => new EncryptInt(a.Value + b);
        public static EncryptInt operator -(EncryptInt a, EncryptInt b) => new EncryptInt(a.Value - b.Value);
        public static EncryptInt operator -(EncryptInt a, int b) => new EncryptInt(a.Value - b);
        public static EncryptInt operator *(EncryptInt a, EncryptInt b) => new EncryptInt(a.Value * b.Value);
        public static EncryptInt operator *(EncryptInt a, int b) => new EncryptInt(a.Value * b);
        public static EncryptInt operator /(EncryptInt a, EncryptInt b) => new EncryptInt(a.Value / b.Value);
        public static EncryptInt operator /(EncryptInt a, int b) => new EncryptInt(a.Value / b);
        public static EncryptInt operator %(EncryptInt a, EncryptInt b) => new EncryptInt(a.Value % b.Value);

        public static EncryptInt operator %(EncryptInt a, int b) => new EncryptInt(a.Value % b);

        //重载ToString,GetHashCode,Equals
        public override string ToString() => Value.ToString();
        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => Value.Equals(obj is EncryptInt ? ((EncryptInt)obj).Value : obj);
    }
}