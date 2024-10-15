// author:KIPKIPS
// describe:数学工具
namespace Framework.Common {
    public static class MathUtils {
        // 洗牌算法,传入目标列表,返回打乱顺序之后的列表,支持泛型
        public static T[] ShuffleCoords<T>(T[] list, int seed = 0) {
            System.Random random = new System.Random(seed); //根据随机种子获得一个随机数
            //遍历并随机交换
            for (int i = 0; i < list.Length - 1; i++) {
                int randomIndex = random.Next(i, list.Length); //返回一个随机的索引
                T tempItem = list[randomIndex]; //swap操作
                list[randomIndex] = list[i];
                list[i] = tempItem;
            }
            return list;
        }
    }
}