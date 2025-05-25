using System.Media;
using System.IO;

namespace SpotTheDifferenceGame.Utils
{
    public static class SoundPlayerHelper
    {
        private static readonly string basePath = @"..\..\Assets\Sounds\";

        public static void PlayCorrect()
        {
            string path = Path.Combine(basePath, "correct.wav");
            PlaySound(path);
        }

        public static void PlayWrong()
        {
            string path = Path.Combine(basePath, "fail.wav");
            PlaySound(path);
        }

        public static void PlaySound(string soundFilePath)
        {
            if (File.Exists(soundFilePath))
            {
                try
                {
                    using (var player = new SoundPlayer(soundFilePath))
                    {
                        player.Play();
                    }
                }
                catch
                {
                    // Optional: log or ignore sound errors
                }
            }
        }
    }
}
