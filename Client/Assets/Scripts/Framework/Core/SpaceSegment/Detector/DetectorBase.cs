// author:KIPKIPS
// date:2024.10.26 18:25
// describe:
using UnityEngine;
using System.Collections;

namespace Framework.Core.SpaceSegment
{
    public abstract class DetectorBase : MonoBehaviour, IDetector
    {
        public Vector3 Position => transform.position;
        public abstract bool UseCameraCulling { get; }
        public abstract bool IsDetected(Bounds bounds);
        public abstract int GetDetectedCode(float x, float y, float z, bool ignoreY);

        //public abstract int DetecedCode2D(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ);
        //public abstract int DetecedCode3D(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ);

        //public abstract int DetectedCode(Bounds bounds, SceneSeparateTreeType treeType);

        //public abstract int DetecedCode(float centerX, float centerY, float centerZ, float sizeX, float sizeY, float sizeZ, SceneSeparateTreeType treeType);
    }
}