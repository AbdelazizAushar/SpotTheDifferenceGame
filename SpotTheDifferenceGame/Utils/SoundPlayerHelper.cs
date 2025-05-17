using System.Media;
using System.IO;

namespace SpotTheDifferenceGame.Utils
{
    public static class SoundPlayerHelper
    {
        public static void PlaySound(string soundFilePath)
        {
            if (File.Exists(soundFilePath))
            {
                using (var player = new SoundPlayer(soundFilePath))
                {
                    player.Play();
                }
            }
        }
    }
}
