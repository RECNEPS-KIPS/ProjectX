using Framework.Common;
using UnityEngine;

namespace GamePlay.Character
{
    public class MountController:MonoBehaviour
    {
        private Transform modelRoot;

        public Transform ModelRoot
        {
            get
            {
                if (modelRoot == null)
                {
                    modelRoot = GameObject.Find("ModelRoot").transform;
                }

                return modelRoot;
            }
        }

        public void MountModel(Transform model)
        {
            model.SetParent(ModelRoot);
            CommonUtils.ResetGO(model);
        }
    }
}