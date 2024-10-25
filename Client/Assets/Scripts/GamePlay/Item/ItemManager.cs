using System.Collections.Generic;
using Framework.Core.Manager.Config;
using Framework.Core.Singleton;

namespace GamePlay.Item
{
    public enum EItemMainType
    {
        None = 0,
        Food = 1,
    }
    public enum EItemSubType
    {
        
    }

    public class ItemConfig
    {
        public int ID;
        public string Name;
        public EItemMainType MainType;
        public EItemSubType SubType;
        public string Path;
        public int Weight;
        public List<int> Params;

        public override string ToString()
        {
            return $"Item: ID{ID},Name:{Name},MainType:{MainType.ToString()},SubType:{SubType.ToString()},Path:{Path},Weight:{Weight},Params:{Params}";
        }
    }
    public class ItemManager:Singleton<ItemManager>
    {

        private Dictionary<int, ItemConfig> _itemConfigMap;
        private const string LOGTag = "ItemManager";
        private Dictionary<int, ItemConfig> ItemConfigMap => _itemConfigMap ??= new Dictionary<int, ItemConfig>();
        
        private Dictionary<EItemMainType, List<ItemConfig>> _itemConfigTypeMap;
        private Dictionary<EItemMainType, List<ItemConfig>> ItemConfigTypeMap => _itemConfigTypeMap ??= new Dictionary<EItemMainType, List<ItemConfig>>();

        private Dictionary<EItemMainType, Dictionary<EItemSubType, List<ItemConfig>>> _itemConfigTypeSubMap;
        private Dictionary<EItemMainType, Dictionary<EItemSubType,List<ItemConfig>>> ItemConfigTypeSubMap => _itemConfigTypeSubMap ??= new Dictionary<EItemMainType, Dictionary<EItemSubType,List<ItemConfig>>>();

        public void Launch()
        {
            InitItemConfig();
        }
        private void InitItemConfig()
        {
            var cfList = ConfigManager.GetConfig(EConfig.Item);
            foreach (var cf in cfList)
            {
                var itemCf = new ItemConfig
                {
                    ID = cf["id"],
                    Name = cf["name"],
                    MainType = (EItemMainType)cf["mainType"],
                    SubType = (EItemSubType)cf["subType"],
                    Path = cf["path"],
                    Weight = cf["weight"],
                    Params = cf["params"],
                };
                LogManager.Log(LOGTag,$"itemCf:{itemCf}");
                //添加到<id,config> map
                ItemConfigMap.TryAdd(cf["id"],itemCf);
                
                //按照mainType添加
                if (!ItemConfigTypeMap.ContainsKey(itemCf.MainType))
                {
                    ItemConfigTypeMap.Add(itemCf.MainType,new List<ItemConfig>());
                }
                ItemConfigTypeMap[itemCf.MainType].Add(itemCf);
                
                //按照mainType subType添加
                if (!ItemConfigTypeSubMap.ContainsKey(itemCf.MainType))
                {
                    ItemConfigTypeSubMap.Add(itemCf.MainType,new Dictionary<EItemSubType, List<ItemConfig>>());
                }

                if (!ItemConfigTypeSubMap[itemCf.MainType].ContainsKey(itemCf.SubType))
                {
                    ItemConfigTypeSubMap[itemCf.MainType].Add(itemCf.SubType,new List<ItemConfig>());
                }
                ItemConfigTypeSubMap[itemCf.MainType][itemCf.SubType].Add(itemCf);
            }
        }

        public ItemConfig GetItemConfigByID(int itemID)
        {
            return ItemConfigMap.GetValueOrDefault(itemID);
        }
        
        public List<ItemConfig> GetItemListConfigByMainType(EItemMainType mainType)
        {
            return ItemConfigTypeMap.GetValueOrDefault(mainType);
        }
        public List<ItemConfig> GetItemListConfigByMainSubType(EItemMainType mainType,EItemSubType subType)
        {
            return !ItemConfigTypeSubMap.TryGetValue(mainType, out var subDict) ? null : subDict.GetValueOrDefault(subType);
        }
    }
}