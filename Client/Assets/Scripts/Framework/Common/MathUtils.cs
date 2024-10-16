// author:KIPKIPS
// describe:数学工具
namespace Framework.Common {
    /// <summary>
    /// 数学工具
    /// </summary>
    public static class MathUtils {
        /// <summary>
        /// 洗牌算法,传入目标列表,返回打乱顺序之后的列表,支持泛型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="seed"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ShuffleCoords<T>(T[] list, int seed = 0) {
            var random = new System.Random(seed); //根据随机种子获得一个随机数
            //遍历并随机交换
            for (var i = 0; i < list.Length - 1; i++) {
                var randomIndex = random.Next(i, list.Length); //返回一个随机的索引
                (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
            }
            return list;
        }
    }
}