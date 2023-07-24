using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Horizon.AppointmentKeeper.Graph;
using Horizon.AppointmentKeeper.Services;

namespace Horizon.AppointmentKeeper
{
    public  class SettingsContext : INotifyPropertyChanged
    {
        private string _graphUser;
        public string GraphUser
        {
            get => _graphUser;
            set
            {
                _graphUser = value;

                OnPropertyChanged();
            }
        }
        public decimal Volume
        {
            get => Properties.Settings.Default.Volume;
            set
            {
                Properties.Settings.Default.Volume = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int VolumePercent
        {
            get => Convert.ToInt32(Math.Round(Volume * 100, 0));
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                Volume = Convert.ToDecimal(value) / 100;
            }
        }
        public decimal WarningSoundVolume
        {
            get => Properties.Settings.Default.WarningSoundVolume;
            set
            {
                if(value>=0.9m)
                {
                    value = 0.9m;
                }
                Properties.Settings.Default.WarningSoundVolume = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(WarningSoundVolumePercent));
            }
        }
        public int WarningSoundVolumePercent
        {
            get => Convert.ToInt32(Math.Round(WarningSoundVolume * MAX_VOLUME_CONVERSION, 0));
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                WarningSoundVolume = Convert.ToDecimal(value) / MAX_VOLUME_CONVERSION;
            }
        }
        public decimal ExpiredSoundVolume
        {
            get => Properties.Settings.Default.ExpiredSoundVolume;
            set
            {
                if (value >= 0.9m)
                {
                    value = 0.9m;
                }
                Properties.Settings.Default.ExpiredSoundVolume = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpiredSoundVolumePercent));
            }
        }
        public int ExpiredSoundVolumePercent
        {
            get => Convert.ToInt32(Math.Round(ExpiredSoundVolume * MAX_VOLUME_CONVERSION, 0));
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                ExpiredSoundVolume = Convert.ToDecimal(value) / MAX_VOLUME_CONVERSION;
            }
        }
        public decimal DangerSoundVolume
        {
            get => Properties.Settings.Default.DangerSoundVolume;
            set
            {
                if (value >= 0.9m)
                {
                    value = 0.9m;
                }
                Properties.Settings.Default.DangerSoundVolume = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(DangerSoundVolumePercent));
            }
        }
        private const decimal MAX_VOLUME_CONVERSION = 100m / 0.9m;
        public int DangerSoundVolumePercent
        {
            get => Convert.ToInt32(Math.Round(DangerSoundVolume * MAX_VOLUME_CONVERSION, 0));
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                DangerSoundVolume = Convert.ToDecimal(value) / MAX_VOLUME_CONVERSION;
            }
        }
        public decimal EventChangedSoundVolume
        {
            get => Properties.Settings.Default.EventChangedSoundVolume;
            set
            {
                if (value >= 0.9m)
                {
                    value = 0.9m;
                }
                Properties.Settings.Default.EventChangedSoundVolume = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(EventChangedSoundVolumePercent));
            }
        }
        public int EventChangedSoundVolumePercent
        {
            get => Convert.ToInt32(Math.Round(EventChangedSoundVolume * MAX_VOLUME_CONVERSION, 0));
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 100)
                    value = 100;

                EventChangedSoundVolume = Convert.ToDecimal(value) / MAX_VOLUME_CONVERSION;
            }
        }

        public string TimerFontFamily
        {
            get {
                var candidate = Properties.Settings.Default.TimerFontFamily;
                if(_fontService.IsFontAvailable(candidate))
                {
                    return candidate;
                }
                return FontService.DefaultFont;
            }
            set
            {
                Properties.Settings.Default.TimerFontFamily = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public bool TimerFontItalicized
        {
            get
            {
                return Properties.Settings.Default.TimerFontItalicized;
            }
            set
            {
                Properties.Settings.Default.TimerFontItalicized = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public bool TimerFontBold
        {
            get
            {
                return Properties.Settings.Default.TimerFontBold;
            }
            set
            {
                Properties.Settings.Default.TimerFontBold = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public IEnumerable<string> FontFamilies
            => _fontService.FontNames;

        public bool GraphEnabled
        {
            get => Properties.Settings.Default.GraphEnabled;
            set
            {
                Properties.Settings.Default.GraphEnabled = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public string GraphCalendar
        {
            get => Properties.Settings.Default.GraphCalendar;
            set
            {
                Properties.Settings.Default.GraphCalendar = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        private IEnumerable<Calendar> _graphCalendars;
        public IEnumerable<Calendar> GraphCalendars
        {
            get => _graphCalendars;
            set
            {
                _graphCalendars = value;
                OnPropertyChanged();
            }
        }


        public int WindowX
        {
            get => Properties.Settings.Default.WindowX;
            set
            {
                Properties.Settings.Default.WindowX = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int WindowY
        {
            get => Properties.Settings.Default.WindowY;
            set
            {
                Properties.Settings.Default.WindowY = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int WindowWidth
        {
            get => Properties.Settings.Default.WindowWidth;
            set
            {
                Properties.Settings.Default.WindowWidth = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int WindowHeight
        {
            get => Properties.Settings.Default.WindowHeight;
            set
            {
                Properties.Settings.Default.WindowHeight = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public System.Windows.Media.Color NormalColor
        {
            get => ConvertColor(Properties.Settings.Default.NormalColor);
            set
            {
                Properties.Settings.Default.NormalColor = value.ToString();
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool AlwaysOnTop
        {
            get => Properties.Settings.Default.AlwaysOnTop;
            set
            {
                Properties.Settings.Default.AlwaysOnTop = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        private System.Windows.Media.Color ConvertColor(string input)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(input);
        }

        public System.Windows.Media.Color DangerColor
        {
            get => ConvertColor(Properties.Settings.Default.DangerColor);
            set
            {
                Properties.Settings.Default.DangerColor =  value.ToString();
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public System.Windows.Media.Color WarningColor
        {
            get => ConvertColor(Properties.Settings.Default.WarningColor);
            set
            {
                Properties.Settings.Default.WarningColor = value.ToString();
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public TimeSpan WarningThreshold
        {
            get => Properties.Settings.Default.WarningThreshold;
            set
            {
                Properties.Settings.Default.WarningThreshold = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int WarningThresholdMinutes
        {
            get => Convert.ToInt32(Math.Floor(WarningThreshold.TotalMinutes));
            set => WarningThreshold = new TimeSpan(value*TimeSpan.TicksPerMinute);
        }

        public TimeSpan DangerThreshold
        {
            get => Properties.Settings.Default.DangerThreshold;
            set
            {
                Properties.Settings.Default.DangerThreshold = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public int DangerThresholdMinutes
        {
            get => Convert.ToInt32(Math.Floor(DangerThreshold.TotalMinutes));
            set => DangerThreshold = new TimeSpan(value * TimeSpan.TicksPerMinute);
        }

        public string CustomWarningSound
        {
            get => Properties.Settings.Default.CustomWarningSound;
            set
            {
                Properties.Settings.Default.CustomWarningSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomWarningSoundSet));
            }
        }
        public bool CustomWarningSoundSet
        {
            get => !String.IsNullOrWhiteSpace(CustomWarningSound);
        }

        public string CustomDangerSound
        {
            get => Properties.Settings.Default.CustomDangerSound;
            set
            {
                Properties.Settings.Default.CustomDangerSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomDangerSoundSet));
            }
        }
        public bool CustomDangerSoundSet
        {
            get => !String.IsNullOrWhiteSpace(CustomDangerSound);
        }
        public string CustomExpiredSound
        {
            get => Properties.Settings.Default.CustomExpiredSound;
            set
            {
                Properties.Settings.Default.CustomExpiredSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomExiredSoundSet));
            }
        }
        public bool CustomExiredSoundSet
        {
            get => !String.IsNullOrWhiteSpace(CustomExpiredSound);
        }
        public string CustomEventChangeSound
        {
            get => Properties.Settings.Default.CustomEventChangeSound;
            set
            {
                Properties.Settings.Default.CustomEventChangeSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CustomEventChangeSoundSet));
            }
        }
        public bool CustomEventChangeSoundSet
        {
            get => !String.IsNullOrWhiteSpace(CustomEventChangeSound);
        }
        public bool Mute
        {
            get => Properties.Settings.Default.Mute;
            set
            {
                Properties.Settings.Default.Mute = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool MuteDangerSound
        {
            get => Properties.Settings.Default.MuteDangerSound;
            set
            {
                Properties.Settings.Default.MuteDangerSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool MuteEventChangeSound
        {
            get => Properties.Settings.Default.MuteEventChangeSound;
            set
            {
                Properties.Settings.Default.MuteEventChangeSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public decimal WindowOpacity
        {
            get
            {
                return Properties.Settings.Default.WindowOpacity;
            }
            set
            {
                Properties.Settings.Default.WindowOpacity = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowOpacityPercent));
            }
        }
        public int WindowOpacityPercent
        {
            get => Convert.ToInt32(Math.Round(WindowOpacity * 100,0));
            set
            {
                if (value < 0)
                    value = 0;
                if(value>100)
                    value= 100;

                WindowOpacity = Convert.ToDecimal(value)/100;
            }
        }
        


        public System.Windows.Media.Color NextEventBackgroundColor
        {
            get => ConvertColor(Properties.Settings.Default.NextEventBackgroundColor);
            set
            {
                Properties.Settings.Default.NextEventBackgroundColor = value.ToString();
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool MuteExpiredSound
        {
            get => Properties.Settings.Default.MuteExpiredSound;
            set
            {
                Properties.Settings.Default.MuteExpiredSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool MuteWarningSound
        {
            get => Properties.Settings.Default.MuteWarningSound;
            set
            {
                Properties.Settings.Default.MuteWarningSound = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public bool ShowNextEvent
        {
            get => Properties.Settings.Default.ShowNextEvent;
            set
            {
                Properties.Settings.Default.ShowNextEvent = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }
        public string InfoStyle
        {
            get => Properties.Settings.Default.InfoStyle;
            set
            {
                Properties.Settings.Default.InfoStyle = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public int TimerSpace
        {
            get => Math.Min(Math.Max(Properties.Settings.Default.TimerSpace, 200), 800);
            set
            {
                Properties.Settings.Default.TimerSpace = Math.Min(Math.Max(value,200),800);
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public string StartupPath
        {
            get
            {
                var path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                if (String.IsNullOrWhiteSpace(path))
                {
                    path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hak.exe");
                }
                return path;
            }
        }

        public bool RunOnStartup
        {
            get
            {
                var rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk == null)
                {
                    _logger.LogWarning("null returned when opening run on startup key");
                    return false;
                }
                return rk.GetValue("HorizonTimer")?.ToString() == StartupPath;

            }
            set
            {
                var rk = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if(rk==null)
                {
                    _logger.LogWarning("null returned when opening run on startup key");
                    return;
                }
                if (value)
                {

                    rk.SetValue("HorizonTimer", StartupPath);
                }
                else
                { 
                        rk.DeleteValue("HorizonTimer", false);
                }
            }
        }

        public TimeSpan CalenderCheckTimeout
        {
            get => Properties.Settings.Default.CalenderCheckTimeout;
            set
            {
                Properties.Settings.Default.CalenderCheckTimeout = value;
                Properties.Settings.Default.Save();

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ILogger<SettingsContext> _logger;
        private readonly FontService _fontService;

        public SettingsContext(ILogger<SettingsContext> logger, FontService fontService)
        {
            _logger = logger;
            this._fontService = fontService;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
