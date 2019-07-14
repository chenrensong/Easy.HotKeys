using Easy.HotKeys;
using System;
using System.Windows;
using System.Windows.Input;
namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly EasyHotKey _easyHotKey = new EasyHotKey();

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            _easyHotKey.KeyPressed += _easyHotKey_KeyPressed;
        }

        private void _easyHotKey_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            MessageBox.Show($"{e.HotKey} Hot Key Pressed!");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _easyHotKey?.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _easyHotKey.Register(Key.F3, ModifierKeys.Control | ModifierKeys.Alt);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _easyHotKey.Unregister(Key.F3, ModifierKeys.Control | ModifierKeys.Alt);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _easyHotKey.Register(Key.A, ModifierKeys.Control);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _easyHotKey.Unregister(Key.A, ModifierKeys.Control);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _easyHotKey.UnregisterAll();
        }
    }
}
