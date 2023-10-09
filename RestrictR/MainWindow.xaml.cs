using Helper;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
       ImagesRepository ImagesRepository { get; } = new();

        public MainWindow()
        {
            this.InitializeComponent();

            string folderPath = "C:\\Users\\hazya\\Pictures\\Screenshots";

            LoadImages(folderPath);
        }

        private void LoadImages(string folderPath)
        {
            ImagesRepository.GetImages(folderPath);
            var numImages = ImagesRepository.Images.Count;
            ImageInfoBar.Message = $"{numImages} loaded.";
            ImageInfoBar.IsOpen = true;
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();

            if(folder != null)
            {
                LoadImages(folder.Path);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var imageInfo = (sender as Button)?.DataContext as ImageInfo;

            if(imageInfo != null)
            {
                Image image = new();

                image.Source = new BitmapImage(new Uri(imageInfo.Path, UriKind.Absolute));

                Window window = new()
                {
                    Title = imageInfo.Name,
                    Content = image
                };

                SetWindowSize(window, 640, 400);
                window.Activate();
            }
        }

        private static void SetWindowSize(Window window, int height, int width)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowsId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowsId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(height, width));
        }

        SpringVector3NaturalMotionAnimation _springAnimation;

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = this.Compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(1.15f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(1.0f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

    }

    public class ImageInfo
    {
        public ImageInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class ImagesRepository
    {
        public ObservableCollection<ImageInfo> Images = new();

        public void GetImages(string folderPath)
        {
            Images.Clear();

            var dir = new DirectoryInfo(folderPath);

            var files = dir.GetFiles("*.png");

            foreach (var file in files)
            {
                Images.Add(new ImageInfo(file.Name, file.FullName));
            }
        }
    }
}
