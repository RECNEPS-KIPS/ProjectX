// author:KIPKIPS
// date:2023.04.28 12:12
// describe:挂载管理器

using Framework.Core.Singleton;
using UnityEngine;

namespace Framework.Core.Manager.Mount
{
    public class MountManager : Singleton<MountManager>
    {
        private Transform _modelMountRoot;

        public Transform ModelMountRoot
        {
            get
            {
                if (_modelMountRoot != null) return _modelMountRoot;
                var go = new GameObject
                {
                    name = "[ModelMountRoot]"
                };
                _modelMountRoot = go.transform;
                _modelMountRoot.localPosition = Vector3.zero;
                _modelMountRoot.localRotation = Quaternion.identity;
                _modelMountRoot.localScale = Vector3.one;
                return _modelMountRoot;
            }
        }

        public override void Dispose()
        {
            Object.Destroy(_modelMountRoot);
        }
    }
}