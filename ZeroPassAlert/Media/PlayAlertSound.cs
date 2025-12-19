using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ZeroPassAlert.Media
{
    public static class PlayAlertSound
    {
        private static DateTime _lastSoundTime = DateTime.MinValue;
        private static MediaPlayer _player = new MediaPlayer();

        public static void PlayAlert()
        {
            if ((DateTime.Now - _lastSoundTime).TotalMilliseconds < 800)
                return; // 0.8초 내 중복 재생 방지

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Alert", "notification.mp3");
                _player.Open(new Uri(path));
                _player.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Sound Error] " + ex.Message);
            }
        }
    }
}
