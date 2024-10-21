using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.MTP
{
    [RequireComponent(typeof(Animator))]
    public class StyleManager : MonoBehaviour
    {
        // Content
        public List<TextItem> textItems = new List<TextItem>();
        public List<ImageItem> imageItems = new List<ImageItem>();

        // Animation
        public Animator styleAnimator;
        public AnimationClip inAnim;
        public AnimationClip outAnim;
        public bool playOnEnable = true;
        [Range(0.1f, 2.5f)] public float animationSpeed = 1;
        public float showFor = 2.5f;

        // Settings
        public bool forceUpdate = true;
        public bool customContent = false;
        public bool customizableWidth = false;
        public bool customizableHeight = false;
        public bool customScale = false;
        public bool playOutAnimation = true;
        public bool disableOnOut = true;
        public bool loopAnimations = false;
        [Range(0.1f, 10)] public float scale = 1;

        // Events
        public UnityEvent onEnable;
        public UnityEvent onDisable;

#if UNITY_EDITOR
        public bool editMode = false;
        public bool inspectAnim = false;
        public float tempAnimTime = 0;
#endif

        void OnEnable()
        {
            // Check for play on enable
            if (playOnEnable == true) { Play(); }
        }

        public void Play()
        {
            // Enable the object
            gameObject.SetActive(true);

            // Initialize the speed
            InitializeSpeedInternal();

            // Call Play function
            PlayIn();

            // Process OnEnable events
            onEnable.Invoke();

            // Start out timer if enabled
            if (playOutAnimation == true)
            {
                StartCoroutine("StartTimer");
                StartCoroutine("DisableStyle");
            }
        }

        public void PlayIn()
        {
            // Enable the object
            gameObject.SetActive(true);

            // Stop prev coroutines
            StopCoroutine("StartTimer");
            StopCoroutine("DisableStyle");

            // Play the animation
            styleAnimator.Play("In");
        }

        public void PlayOut()
        {
            // Check for animation states and play out
            if (!styleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
                styleAnimator.Play("Out");
        }

        void InitializeSpeedInternal()
        {
            styleAnimator.SetFloat("Anim Speed", animationSpeed);
        }

        public void InitializeSpeed(float speed)
        {
            animationSpeed = speed;
            styleAnimator.SetFloat("Anim Speed", animationSpeed);
        }

        IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(showFor);
            PlayOut();
            if (loopAnimations == true) { StartCoroutine("DisableStyle"); }
        }

        IEnumerator DisableStyle()
        {
            while (!styleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
                yield return new WaitForSeconds(1f);

            if (loopAnimations == true) 
            {
                styleAnimator.Play("In");
                StartCoroutine("StartTimer");
            }

            else if (styleAnimator.GetCurrentAnimatorStateInfo(0).IsName("Finished"))
            {
                onDisable.Invoke();
                if (disableOnOut == true) { gameObject.SetActive(false); }
            }
        }
    }
}