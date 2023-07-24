using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Horizon.AppointmentKeeper.Context;
using Horizon.AppointmentKeeper.Services;
using Microsoft.Graph;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Horizon.AppointmentKeeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private TimerContext TimerContext => (TimerContext)DataContext;
        Settings? settingsWindow = null;
        About? aboutWindow = null;
        private readonly IServiceProvider serviceProvider;
        private readonly System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly SettingsContext settingsContext;
        private readonly ILogger<MainWindow> logger;

        public MainWindow(TimerContext timerContext, FontService fontService, IServiceProvider serviceProvider, SettingsContext settingsContext, 
            ILogger<MainWindow> logger)
        {
            this.settingsContext = settingsContext;
            this.DataContext = timerContext;
            this.logger = logger;

            var screenSize = System.Windows.SystemParameters.WorkArea;

            if (TimerContext.WindowX > screenSize.Width)
            {
                TimerContext.WindowX = 0;
            }

            if (TimerContext.WindowY > screenSize.Height)
            {
                TimerContext.WindowY = 0;
            }

            this.Left = TimerContext.WindowX;
            this.Top = TimerContext.WindowY;
            this.Width = TimerContext.WindowWidth;
            this.Height = TimerContext.WindowHeight;
            TimerContext.TimerStateChanged += TimerContext_TimerStateChanged;
            InitializeComponent();
            this.serviceProvider = serviceProvider;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(this.settingsContext.StartupPath);
            notifyIcon.Visible = false;
            notifyIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            notifyIcon.Text = "HAK";
            notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add(
                text: "Settings",
                image: null,
                onClick: delegate (object sender, EventArgs args)
                {
                    this.Settings_Click(null, null);
                });
            notifyIcon.ContextMenuStrip.Items.Add(
                text: "Reset Position",
                image: null,
                onClick: delegate (object sender, EventArgs args)
                {
                    this.Left = 0;
                    this.Top = 0;
                });
            notifyIcon.ContextMenuStrip.Items.Add(
                text: "About",
                image: null,
                onClick: delegate (object sender, EventArgs args)
                {
                    this.About_Click(null, null);
                });
            notifyIcon.ContextMenuStrip.Items.Add(
                text: "Close",
                image: null,
                onClick: delegate (object sender, EventArgs args)
                {
                    this.Close();
                });
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            await LoginToGraph();
        }

        bool signedIn = false;
        private async Task LoginToGraph()
        {
            var graph = await serviceProvider.GetRequiredService<GraphService>().GetClient();

            while (true)
            {
                try
                {
                    var response = await graph.Me.Calendar.Request().GetResponseAsync();
                    signedIn = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while signing in to graph");
                    var result = MessageBox.Show(messageBoxText: "There was an issue signing in. Try again?",
                        caption: "HAK Sign-in issue", button: MessageBoxButton.YesNo, owner: this,
                        icon: MessageBoxImage.Error);
                    if (result == MessageBoxResult.No)
                    {
                        this.Close();
                        return;
                    }
                }
            }

            notifyIcon.Visible = true;
            TimerContext.StartTimers();
        }

        private void TimerContext_TimerStateChanged(object? sender, TimerState e)
        {
            if(e==TimerState.Expired)
            {
                //this.Dispatcher.Invoke(new Action(() => { new Robrise().Show(); }));
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background.Opacity = 0.5;
            this.ResizeGripper.Visibility = Visibility.Visible;
            this.RatioGripper.Visibility = Visibility.Visible;
            this.Close.Visibility = Visibility.Visible;
            if (signedIn)
            { 
                this.Settings.Visibility = Visibility.Visible;
            }
            TimerContext.WindowOpacityOvrride = 1;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background.Opacity = 0;
            this.ResizeGripper.Visibility = Visibility.Hidden;
            this.RatioGripper.Visibility = Visibility.Hidden;
            this.Close.Visibility = Visibility.Hidden;
            this.Settings.Visibility = Visibility.Hidden;
            TimerContext.WindowOpacityOvrride = null;

        }

        private void ResizeGripper_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Width = Math.Max(0,Width+e.HorizontalChange);
            Height = Math.Max(0,Height+e.VerticalChange);
            TimerContext.WindowHeight = (int)Height;
            TimerContext.WindowWidth = (int)Width;
        }

        private void RatioGripper_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var heightChange = Convert.ToInt32(Math.Round((e.VerticalChange/Height)*1000));
            settingsContext.TimerSpace += heightChange;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow?.Close();
            this.Close();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            TimerContext.WindowX = (int)this.Left;
            TimerContext.WindowY = (int)this.Top;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                this.Dispatcher.Invoke(new Action(() => {
                    aboutWindow = serviceProvider.GetRequiredService<About>();
                    aboutWindow.Closed += AboutWindow_Closed;
                    aboutWindow.Show();
                }));
            }
            else
            {
                aboutWindow.Dispatcher.Invoke(new Action(() => { aboutWindow.Focus(); }));
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null)
            {
                this.Dispatcher.Invoke(new Action(() => {
                    settingsWindow = serviceProvider.GetRequiredService<Settings>();
                    settingsWindow.Closed += SettingsWindow_Closed;
                    settingsWindow.Show();
                }));
            }
            else
            {
                settingsWindow.Dispatcher.Invoke(new Action(() => { settingsWindow.Focus(); }));
            }
        }

        private void AboutWindow_Closed(object? sender, EventArgs e)
        {
            aboutWindow = null;
        }
        private void SettingsWindow_Closed(object? sender, EventArgs e)
        {
            settingsWindow = null;
        }

        private void TargetTimePicker_Click(object sender, RoutedEventArgs e)
        {
            var picker = new TimePicker(DateTime.Now.AddHours(1));
            if(picker.ShowDialog().GetValueOrDefault())
            {
                TimerContext.TargetTime = picker.SelectedTime;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(this.settingsWindow!=null)
            {
                settingsWindow.Close();
            }
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }


    }
}
