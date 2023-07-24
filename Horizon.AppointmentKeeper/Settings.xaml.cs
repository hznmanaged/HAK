using Horizon;
using Microsoft.Graph;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Horizon.AppointmentKeeper.Context;
using Horizon.AppointmentKeeper.Services;

namespace Horizon.AppointmentKeeper
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private SettingsContext settingsContext => (SettingsContext)this.DataContext;
        private readonly SoundService soundService;

        private readonly GraphService graphService;

        public Settings(SettingsContext context, GraphService graphService, SoundService soundService)
        {
            this.graphService = graphService;
            this.DataContext = context;
            InitializeComponent();
            this.soundService = soundService;
        }

        private string? lastFolder = null;
        private string? browseForFile(string start)
        {
            var fileDialog = new OpenFileDialog();
            if(!String.IsNullOrWhiteSpace(lastFolder))
            {
                fileDialog.InitialDirectory = lastFolder;
            } else
            {
                fileDialog.InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
            }
            fileDialog.Filter = "All compatible audio|*.wav;*.mp3;*.m4a|Waveform Audio File Format|*.wav|MPEG-1/2 Audio Layer III|*.mp3|MPEG-4 Audio|*.m4a|All files|*.*";
            if (fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                lastFolder = System.IO.Path.GetDirectoryName(fileDialog.FileName);
                return fileDialog.FileName;
            }
            return null;
        }

        private void BrowseCustomWarningSound_Click(object sender, RoutedEventArgs e)
        {
            var newFile = browseForFile(settingsContext.CustomWarningSound);
            if (!String.IsNullOrWhiteSpace(newFile))
            {
                settingsContext.CustomWarningSound = newFile;
            }
        }

        private void BrowseCustomDangerSound_Click(object sender, RoutedEventArgs e)
        {
            var newFile = browseForFile(settingsContext.CustomDangerSound);
            if (!String.IsNullOrWhiteSpace(newFile))
            {
                settingsContext.CustomDangerSound = newFile;
            }

        }

        private void BrowseCustomExpiredSound_Click(object sender, RoutedEventArgs e)
        {
            var newFile = browseForFile(settingsContext.CustomExpiredSound);
            if (!String.IsNullOrWhiteSpace(newFile))
            {
                settingsContext.CustomExpiredSound = newFile;
            }
        }

        private void ResetCustomWarningSound_Click(object sender, RoutedEventArgs e)
        {
            settingsContext.CustomWarningSound = "";
        }

        private void ResetCustomDangerSound_Click(object sender, RoutedEventArgs e)
        {
            settingsContext.CustomDangerSound = "";

        }
        private void ResetCustomExpiredSound_Click(object sender, RoutedEventArgs e)
        {
            settingsContext.CustomExpiredSound = "";

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await graphService.Test();
                MessageBox.Show("Test succeeded", "Success", button: MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Format(), "Test error", button: MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool graphRefreshing = false;

        private async Task refreshGraph()
        {
            if (graphRefreshing)
                return;
            graphRefreshing = true;
            try
            {
                var client = await graphService.GetClient();
                settingsContext.GraphCalendars = await graphService.GetCalendars(client);
            }
            finally { graphRefreshing = false; }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            refreshGraph();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshGraph();
        }

        private void ResetCustomEventChangedSound_Click(object sender, RoutedEventArgs e)
        {
            settingsContext.CustomEventChangeSound = "";

        }

        private void BrowseCustomEventChangedSound_Click(object sender, RoutedEventArgs e)
        {
            var newFile = browseForFile(settingsContext.CustomEventChangeSound);
            if (!String.IsNullOrWhiteSpace(newFile))
            {
                settingsContext.CustomEventChangeSound = newFile;
            }
        }




        private void WarningSoundVolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            soundService.PlaySoundWithFallback(primaryFilePath: settingsContext.CustomWarningSound,
                fallbackFilePath: SoundService.DEFAULT_WARNING_FILE,
                volume: settingsContext.WarningSoundVolume);

        }

        private void ExpiredSoundVolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            soundService.PlaySoundWithFallback(primaryFilePath: settingsContext.CustomExpiredSound,
    fallbackFilePath: SoundService.DEFAULT_EXPIRED_FILE,
    volume: settingsContext.ExpiredSoundVolume);

        }

        private void DangerSoundVolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            soundService.PlaySoundWithFallback(primaryFilePath: settingsContext.CustomDangerSound,
    fallbackFilePath: SoundService.DEFAULT_DANGER_FILE,
    volume: settingsContext.DangerSoundVolume);

        }

        private void EventChangedSoundVolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            soundService.PlaySoundWithFallback(primaryFilePath: settingsContext.CustomEventChangeSound,
                fallbackFilePath: SoundService.DEFAULT_EVENT_CHANGED_FILE,
                volume: settingsContext.EventChangedSoundVolume);

        }

        private void numberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AllSoundVolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            soundService.PlaySoundWithFallback(primaryFilePath: settingsContext.CustomEventChangeSound,
                fallbackFilePath: SoundService.DEFAULT_EVENT_CHANGED_FILE,
                volume: settingsContext.EventChangedSoundVolume);

        }

        private async void graphSignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(owner: this,
                messageBoxText: "Are you sure you want to sign out? This will close HAK.",
                caption: "Graph sign out",
                button: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Hand
                );
            if(result==MessageBoxResult.Yes)
            {
                await graphService.ClearAuthCache();
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
