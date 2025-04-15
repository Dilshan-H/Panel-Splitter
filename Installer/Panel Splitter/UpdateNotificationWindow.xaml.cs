using System.Windows;

namespace Panel_Splitter
{
    /// <summary>
    /// Interaction logic for UpdateNotificationWindow.xaml
    /// </summary>
    public partial class UpdateNotificationWindow : Window
    {
        public UpdateNotificationWindow()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            Left = SystemParameters.PrimaryScreenWidth - Width - 10;
            Top = -Height;

            Loaded += UpdateNotificationWindow_Loaded;
        }

        /// <summary>
        /// Add effects on window loading  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateNotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Slide-in effect
            var slideAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = -Height,
                To = 30,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new System.Windows.Media.Animation.CubicEase
                {
                    EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                }
            };
            BeginAnimation(Window.TopProperty, slideAnimation);

            // Fade-in effect
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(400)
            };
            BeginAnimation(Window.OpacityProperty, fadeIn);
        }

        /// <summary>
        /// Handles Install button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the main window and close this
            MainWindow mainWindow = new();
            System.Windows.Application.Current.MainWindow = mainWindow;
            System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();

            this.Close();
        }

        /// <summary>
        /// Handles Dismiss button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            // Shut down the whole app
            System.Windows.Application.Current.Shutdown();
        }
    }
}
