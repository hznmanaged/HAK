using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Horizon.AppointmentKeeper.Exceptions;
using Horizon.AppointmentKeeper.Services;
using Microsoft.Graph.Extensions;
using System.Configuration;

namespace Horizon.AppointmentKeeper.Context
{
    public enum TimerState
    {
        Normal,
        Warning,
        Danger,
        Expired
    }

    public class TimerContext : INotifyPropertyChanged, IDisposable
    {
        public DateTimeOffset TargetTime = DateTimeOffset.Now;

        private TimerState TimerState = TimerState.Normal;
        public decimal? _windowOpacityOverride = null;
        public decimal? WindowOpacityOvrride
        {
            get => _windowOpacityOverride;
            set
            {
                _windowOpacityOverride = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowOpacity));
            }
        }
        public decimal WindowOpacity
        {
            get => WindowOpacityOvrride.GetValueOrDefault(Settings.WindowOpacity);
        }
        public int WindowX
        {
            get => Settings.WindowX;
            set
            {
                Settings.WindowX = value;
                OnPropertyChanged();
            }
        }
        public int WindowY
        {
            get => Settings.WindowY;
            set
            {
                Settings.WindowY = value;
                OnPropertyChanged();
            }
        }
        public int WindowWidth
        {
            get => Settings.WindowWidth;
            set
            {
                Settings.WindowWidth = value;
                OnPropertyChanged();
            }
        }
        public int WindowHeight
        {
            get => Settings.WindowHeight;
            set
            {
                Settings.WindowHeight = value;
                OnPropertyChanged();
            }
        }
        public bool AlwaysOnTop
        {
            get => Settings.AlwaysOnTop;
            set
            {
                Settings.AlwaysOnTop = value;
                OnPropertyChanged();
            }
        }

        public int EventInfoHeight
        {
            get => Settings.ShowNextEvent ? 50 : 0;
        }

        private string _timeRemaining;
        public string TimeRemaining
        {
            get => String.IsNullOrWhiteSpace(OverrideMessage) ? _timeRemaining : OverrideMessage;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }
        public string OverrideMessage { get; set; }

        public SolidColorBrush CurrentColor
        {
            get
            {
                Color color = Settings.NormalColor;
                switch (this.TimerState)
                {
                    case TimerState.Warning:
                        color = Settings.WarningColor;
                        break;
                    case TimerState.Danger:
                    case TimerState.Expired:
                        color = Settings.DangerColor;
                        break;
                }

                return new SolidColorBrush(color);
            }

        }
        public Brush BackgroundColor
        {
            get
            {
                //var c = CurrentColor.Color;
                //var l = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;

                //return l < 0.5 ? Brushes.White : Brushes.Black;

                return new SolidColorBrush(Settings.NextEventBackgroundColor);
            }
        }
        public Color ShadowColor
        {
            get
            {
                //var c = CurrentColor.Color;
                //var l = 0.2126 * c.ScR + 0.7152 * c.ScG + 0.0722 * c.ScB;

                //return l < 0.5 ? Brushes.White : Brushes.Black;

                return Settings.NextEventBackgroundColor;
            }
        }

        public FontFamily FontFamily
        {
            get => new FontFamily(Settings.TimerFontFamily);
        }
        public FontStyle FontStyle
        {
            get => Settings.TimerFontItalicized ? FontStyles.Italic : FontStyles.Normal;
        }
        public FontWeight FontWeight
        {
            get => Settings.TimerFontBold ? FontWeights.Bold : FontWeights.Normal;
        }

        public Visibility IsInfoStyleSolid
        {
            get => Settings.InfoStyle == "Solid" ? Visibility.Visible : Visibility.Hidden;
        }
        public Visibility IsInfoStyleShadow
        {
            get => Settings.InfoStyle == "Shadow" ? Visibility.Visible : Visibility.Hidden;
        }

        public string TimerSpace
        {
            get => $"{Settings.TimerSpace}*";
        }
        public string InfoSpace
        {
            get => $"{(1000-Settings.TimerSpace)}*";
        }

        private CalendarEvent _targetEvent;
        public CalendarEvent TargetEvent
        {
            get => _targetEvent;
            set
            {
                _targetEvent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LowerSectionText));
            }
        }
        public string LowerSectionText { 
            get  {
                if(!String.IsNullOrWhiteSpace(OverrideMessage))
                {
                    return "";
                }
                var output = new StringBuilder();
                output.Append("Target time: ");
                output.Append(TargetTime.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt"));
                if(Settings.ShowNextEvent)
                {
                    if(TargetEvent.StartTime>DateTimeOffset.Now)
                    {
                        //TargetEvent event IS next event
                        output.AppendLine();
                        output.Append(TargetEvent.Name);
                    } else if(TargetEvent.NextEvent!=null) {
                        output.AppendLine();
                        output.Append(TargetEvent.NextEvent.Name);
                        if (TargetEvent.NextEvent.StartTime != TargetTime)
                        {
                            output.AppendLine();
                            output.Append("Start time: ");
                            output.Append(TargetEvent.NextEvent.StartTime.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt"));
                        }

                    }
                }
                return output.ToString();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<TimerState>? TimerStateChanged;

        private readonly SettingsContext Settings;
        private Timer _timer;
        private Timer _calenderTimer;
        private readonly CalendarService _calendarService;
        private readonly SoundService _soundService;
        private readonly ILogger<TimerContext> _logger;
        private readonly int calenderTimeout;

        public TimerContext(SettingsContext settingsContext, ILogger<TimerContext> logger, CalendarService calendarService, SoundService soundService)
        {
            OverrideMessage = "Loading...";
            this.Settings = settingsContext;
            this._logger = logger;
            settingsContext.PropertyChanged += SettingsContext_PropertyChanged;
            _calendarService = calendarService;
            _soundService = soundService;
            calenderTimeout = Convert.ToInt32(settingsContext.CalenderCheckTimeout.TotalMilliseconds);
        }
        private bool started = false;
        public void StartTimers()
        {
            if(started)
            {
                throw new Exception("Already started");
            }
            started = true;
            this._timer = new Timer(TimerCallback, state: null, dueTime: 0, period: 1000);
            this._calenderTimer = new Timer(CalenderTimerCallback, state: null, dueTime: 0, period: calenderTimeout);
        }

        private void SettingsContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
            if (e.PropertyName.Contains("Color"))
            {
                OnPropertyChanged(nameof(CurrentColor));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ShadowColor));
            }
            switch(e.PropertyName)
            {
                case nameof(Settings.TimerFontFamily):
                    OnPropertyChanged(nameof(FontFamily));
                    break;
                case nameof(Settings.TimerFontItalicized):
                    OnPropertyChanged(nameof(FontStyle));
                    break;
                case nameof(Settings.TimerFontBold):
                    OnPropertyChanged(nameof(FontWeight));
                    break;
                case nameof(Settings.ShowNextEvent):
                    OnPropertyChanged(nameof(EventInfoHeight));
                    OnPropertyChanged(nameof(LowerSectionText));
                    break;
                case nameof(Settings.WindowOpacity):
                    OnPropertyChanged(nameof(WindowOpacity));
                    break;
                case nameof(Settings.InfoStyle):
                    OnPropertyChanged(nameof(IsInfoStyleShadow));
                    OnPropertyChanged(nameof(IsInfoStyleSolid));
                    break;
                case nameof(Settings.TimerSpace):
                    OnPropertyChanged(nameof(TimerSpace));
                    OnPropertyChanged(nameof(InfoSpace));
                    break;
            }
        }

        private bool CalendarRefreshActive = false;

        private async void CalenderTimerCallback(object? state)
        {
            if (CalendarRefreshActive)
            {
                return;
            }

            try
            {
                CalendarRefreshActive = true;
                var candidateEvent = await _calendarService.GetNextCalendarEvent();
                TargetTime = (candidateEvent.StartTime > DateTimeOffset.Now)?
                    candidateEvent.StartTime : candidateEvent.EndTime;
                OverrideMessage = null;
                if(TargetEvent != null && candidateEvent.StartTime!= TargetEvent.StartTime)
                {
                    try
                    {
                        _soundService.PlaySoundWithFallback(primaryFilePath: Settings.CustomEventChangeSound, 
                            fallbackFilePath: SoundService.DEFAULT_EVENT_CHANGED_FILE,
                            volume: Settings.EventChangedSoundVolume);
                    }
                    catch(Exception e)
                    {
                        _logger.LogWarning(e, "Error while playing event change sound");
                    }
                }
                TargetEvent = candidateEvent;
            }
            catch (NoCalenderEventsException)
            {
                TargetEvent = null;
                OverrideMessage = "No events";
            }
            catch (CalendarNotFoundException)
            {
                TargetEvent = null;
                OverrideMessage = "No calendar";
            }
            catch (CalendarNotSetException)
            {
                TargetEvent = null;
                OverrideMessage = "Set calendar";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during CalenderTimerCallback");
            }
            finally
            {
                CalendarRefreshActive = false;
            }
        }

        private void TimerCallback(object? state)
        {
            if (!String.IsNullOrWhiteSpace(OverrideMessage))
            {
                TimeRemaining = OverrideMessage;
                return;
            }



            TimeSpan diff = TargetTime - DateTime.Now;
            if (diff.TotalMilliseconds < 0)
            {
                diff = new TimeSpan(0);
            }
            var newState = TimerState.Normal;
            if (diff <= Settings.WarningThreshold.Add(new TimeSpan(hours: 0, minutes: 0, seconds: 1)))
            {
                newState = TimerState.Warning;
            }
            if (diff <= Settings.DangerThreshold.Add(new TimeSpan(hours: 0, minutes: 0, seconds: 1)))
            {
                newState = TimerState.Danger;
            }
            if (diff.TotalSeconds < 1)
            {
                newState = TimerState.Expired;
            }
            if (this.TimerState != newState)
            {
                try
                {
                    string? defaultAudioFile= null;
                    string? customAudioFile = null;
                    decimal volume = 0.5M;
                    switch (newState)
                    {
                        case TimerState.Warning:
                            if (!Settings.MuteWarningSound && !Settings.Mute)
                            {
                                customAudioFile = Settings.CustomWarningSound;
                                defaultAudioFile = SoundService.DEFAULT_WARNING_FILE;
                                volume = Settings.WarningSoundVolume;
                            }
                            break;
                        case TimerState.Danger:
                            if (!Settings.MuteDangerSound && !Settings.Mute)
                            {
                                customAudioFile = Settings.CustomDangerSound;
                                defaultAudioFile = SoundService.DEFAULT_DANGER_FILE;
                                volume = Settings.DangerSoundVolume;
                            }
                            break;
                        case TimerState.Expired:
                            if (!Settings.MuteExpiredSound && !Settings.Mute)
                            {
                                customAudioFile = Settings.CustomExpiredSound;
                                defaultAudioFile = SoundService.DEFAULT_EXPIRED_FILE;
                                volume = Settings.ExpiredSoundVolume;
                            }
                            break;
                    }
                    if (defaultAudioFile != null)
                    {

                        _soundService.PlaySoundWithFallback(primaryFilePath: customAudioFile, fallbackFilePath: defaultAudioFile, volume: volume);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while trying to play sound for state {newState}");
                }
                TimerStateChanged?.Invoke(this, newState);
                this.TimerState = newState;
                OnPropertyChanged(nameof(CurrentColor));
                OnPropertyChanged(nameof(BackgroundColor));
            }
            var timeFormat = new StringBuilder(@"mm\:ss");

            if(diff.TotalHours>=1)
            {
                timeFormat.Insert(0, @"hh\:");
            }

            if(diff.TotalDays>=1)
            {
                timeFormat.Insert(0, @"d\.");
            }

            TimeRemaining = diff.ToString(timeFormat.ToString());
        }




        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            this.Settings.PropertyChanged -= SettingsContext_PropertyChanged;

            _timer.Dispose();
            _calenderTimer.Dispose();
        }
    }
}
