using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Michsky.UI.MTP
{
    public class StyleVideoPreview : EditorWindow
    {
        static StyleVideoPreview window;
        private VideoPlayer player;
        private Texture currentRT;
        public VideoClip videoClip;
        public GameObject tempGO;

        void OnEnable()
        {
            window = GetWindow<StyleVideoPreview>();
            window.minSize = new Vector2(480, 320);
        }

        void OnDisable()
        {
            if (player != null)
                DestroyImmediate(player);

            if (tempGO != null)
                DestroyImmediate(tempGO);
        }

        void OnGUI()
        {
            Repaint();

            if (currentRT != null)
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), currentRT);
        }

        private void PlayerFrameReady(VideoPlayer source, long frameIdx)
        {
            currentRT = source.texture;
        }

        public void UpdateVideo()
        {
            if (tempGO == null)
            {
                var newTempGO = new GameObject("[MTP - Temp Object]");
                tempGO = newTempGO;
            }

            if (player == null)
                player = tempGO.AddComponent<VideoPlayer>();

            player.audioOutputMode = VideoAudioOutputMode.None;
            player.playOnAwake = false;
            player.clip = videoClip;
            player.isLooping = true;
            player.Prepare();
            player.sendFrameReadyEvents = true;
            player.frameReady += PlayerFrameReady;
            player.Play();
        }
    }
}