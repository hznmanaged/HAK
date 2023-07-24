using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Horizon.AppointmentKeeper
{
    public class SoundService
    {
        public const string DEFAULT_EVENT_CHANGED_FILE = "Sounds/Boys Choir Ahs ^.m4a";
        public const string DEFAULT_WARNING_FILE = "Sounds/choir full vibratto ^.m4a";
        public const string DEFAULT_DANGER_FILE = "Sounds/cluster disaster.m4a";
        public const string DEFAULT_EXPIRED_FILE = "Sounds/the big nasty.m4a";


        private readonly ILogger<SoundService> _logger;
        private readonly SettingsContext _settingsContext;
        public SoundService(ILogger<SoundService> logger, SettingsContext settingsContext)
        {
            this._logger = logger;
            _settingsContext = settingsContext;
        }

        public void PlaySoundWithFallback(string? primaryFilePath, string fallbackFilePath, decimal volume)
        {
            if (!String.IsNullOrWhiteSpace(primaryFilePath) &&
                !System.IO.File.Exists(primaryFilePath))
            {
                primaryFilePath = "";
            }
            var fallbackFile = new FileInfo(fallbackFilePath);


            var player = new MediaPlayer();
            player.Volume = Math.Round(Convert.ToDouble(volume * _settingsContext.Volume),2);

            if (!String.IsNullOrWhiteSpace(primaryFilePath))
            {
                var primaryFile = new FileInfo(primaryFilePath);
                try
                {
                    player.Open(new Uri(primaryFile.FullName));
                    player.Play();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while trying to play file: {primaryFilePath}");
                    player.Open(new Uri(fallbackFile.FullName));
                    player.Play();

                }
            }
            else
            {
                player.Open(new Uri(fallbackFile.FullName));
                player.Play();
            }

        }
    }
}
