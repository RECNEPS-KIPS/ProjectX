// author:KIPKIPS
// date:2023.04.28 14:08
// describe:扩展Image
using Framework.Core.Manager.ResourcesLoad;
using UnityEditor;
using UnityEngine;

namespace Framework.Core.Manager.UI
{
    public class PrefabMounter : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        private GameObject _cameraGo;
        private GameObject _prefabGo;
        private GameObject _containerGo;

        private GameObject CameraGo
        {
            get
            {
                if (_cameraGo != null) return _cameraGo;
                _cameraGo = new GameObject("Camera");
                var t = _cameraGo.transform;
                t.SetParent(transform);
                t.localPosition = new Vector3(0, 3.8f, 8);
                t.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                t.localScale = Vector3.one;
                return _cameraGo;
            }
        }

        private GameObject ContainerGo
        {
            get
            {
                if (_containerGo != null) return _containerGo;
                _containerGo = new GameObject("Container");
                var t = _containerGo.transform;
                t.SetParent(transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                return _containerGo;
            }
        }

        private UnityEngine.Camera Camera
        {
            get
            {
                if (_camera != null) return _camera;
                _camera = CameraGo.AddComponent<UnityEngine.Camera>();
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.cullingMask = LayerMask.NameToLayer("Default"); //| LayerMask.NameToLayer("3DUI");
                return _camera;
            }
        }

        public void SetRenderTexture(RenderTexture rt)
        {
            Camera.targetTexture = rt;
        }

        private void OnDestroy()
        {
            if (_prefabGo != null)
            {
                DestroyImmediate(_prefabGo);
            }
        }

        public void SetModelPath(string modelPath)
        {
            if (_prefabGo != null)
            {
                DestroyImmediate(_prefabGo);
            }
            
            _prefabGo = Instantiate(ResourcesLoadManager.LoadAsset<GameObject>(modelPath), Vector3.zero, Quaternion.identity, ContainerGo.transform);
            _prefabGo.transform.localScale = Vector3.one;
        }

        public void SetModelLocalRotation(Vector3 v)
        {
            _prefabGo.transform.localEulerAngles = v;
        }

        public void SetModelLocalPosition(Vector3 v)
        {
            _prefabGo.transform.localPosition = v;
        }

        public void SetModelLocalScale(Vector3 v)
        {
            _prefabGo.transform.localScale = v;
        }
    }
}