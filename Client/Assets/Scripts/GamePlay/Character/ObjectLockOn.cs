using UnityEngine;

namespace GamePlay.Character
{
    public class ObjectLockOn : MonoBehaviour
    {
        public Material _highlightMat;
        public Material _targetMat;
        private Transform _highlightOrb;

        private MeshRenderer _meshRenderer;

        /// <inheritdoc cref="Start" />
        private void Start()
        {
            _highlightOrb = transform.Find("TargetHighlight");
            _meshRenderer = _highlightOrb.GetComponent<MeshRenderer>();

            if (_meshRenderer == null)
            {
                Debug.LogError("This script requires a MeshRenderer component on the GameObject.");
            }
        }

        /// <summary>
        ///     Adds this object as a potential lock on target if the player is within range of the target.
        /// </summary>
        /// <param name="otherCollider">The collider to check.</param>
        private void OnTriggerEnter(Collider otherCollider)
        {
            AnimationController animationController = otherCollider.GetComponent<AnimationController>();

            // Only interested in player collisions if they have the controller script.
            if (animationController != null)
            {
                animationController.AddTargetCandidate(gameObject);
            }
        }

        /// <summary>
        ///     Removes this object as a potential lock on target if the player is within range of the target.
        /// </summary>
        /// <param name="otherCollider">The collider to check.</param>
        private void OnTriggerExit(Collider otherCollider)
        {
            AnimationController animationController = otherCollider.GetComponent<AnimationController>();

            // Only interested in player collisions if they have the controller script.
            if (animationController != null)
            {
                animationController.RemoveTarget(gameObject);
                Highlight(false, false);
            }
        }

        /// <summary>
        ///     Sets the highlight status of this object, and which highlight to use.
        /// </summary>
        /// <param name="enable">Whether the highlight is enabled on this object; or not.</param>
        /// <param name="targetLock">Whether this object is locked on to; or not.</param>
        public void Highlight(bool enable, bool targetLock)
        {
            Material currentMaterial = targetLock ? _targetMat : _highlightMat;

            if (_highlightOrb != null)
            {
                _highlightOrb.gameObject.SetActive(enable);
                if (enable)
                {
                    _meshRenderer.material = currentMaterial;
                }
            }
        }
    }
}
