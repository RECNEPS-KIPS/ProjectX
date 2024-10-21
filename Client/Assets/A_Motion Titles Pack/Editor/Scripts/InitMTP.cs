using UnityEditor;

namespace Michsky.UI.MTP
{
    public class InitMUIP
    {
        [InitializeOnLoad]
        public class InitOnLoad
        {
            static InitOnLoad()
            {
                if (!EditorPrefs.HasKey("MTPv1.Installed"))
                {
                    EditorPrefs.SetInt("MTPv1.Installed", 1);
                    EditorUtility.DisplayDialog("Hello there!", "Thank you for purchasing Motion Titles Pack." +
                        "\r\rIf you need help, feel free to contact us through our support channels or Discord.", "Got it!");
                }

                if (!EditorPrefs.HasKey("MTP.StyleCreator.Upgraded"))
                {
                    EditorPrefs.SetInt("MTP.StyleCreator.Upgraded", 1);
                    EditorPrefs.SetString("MTP.StyleCreator.RootFolder", "Motion Titles Pack/Style Creator/");
                }
            }
        }
    }
}