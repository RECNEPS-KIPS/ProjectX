
using UnityEngine;
using HuHu;
using System.Collections.Generic;

public class GameBlackboard: Singleton<GameBlackboard>
{
    //<T>��ʾ����һ�����ͣ������ֶκ����ԡ�ί���ֶζ����޷���������T�ģ�ֻ�������������������������ǿ����ڷ�������������<T>,�������ͷ�������ʹ��
    //Ŀǰ��ʹ�ù�����ɫ������
    private Dictionary<string , object> GameData = new Dictionary<string , object>();

    public BindableProperty<Transform> enemy=new BindableProperty<Transform>();
   
    public void SetEnemy(Transform Enemy)
    { 
    this.enemy.Value = Enemy; 
    }
    public Transform GetEnemy()
    {
        
      return this.enemy.Value;
    }
    
    public void SetGameData<T>(string DataName, T value )where T : class
    {
        if (GameData.ContainsKey(DataName))
        {
            GameData[DataName]=value;
        }
        else
        {
            GameData.Add(DataName, value);
        }
    
    }
    public T GetGameData<T>(string DataName)where T : class
    {
        if (GameData.TryGetValue(DataName, out var e))
        { 
        return e as T;
        }
        return default(T);
        //����T�ķ������ͱ�object���������͵İ�ȫ�ԣ���Ϊ�ڵ������÷���ʱ��Ҫ˵��ָ�������ͣ��Ӷ�ֱ��ת��Ϊ������
        //����object����Ҫ��ʽת����(����)object��������e�����Ͳ���������;ͻᷢ������
        //�����T��һ���������ͣ���ô�����ֵҲ��������������
        //�������T�ȿ���Ϊֵ��Ҳ����Ϊ���ã���ô��Ҫif��e is T A��{return A} ������ת����asֻ��֧���������͵�ת��
    }
}   
