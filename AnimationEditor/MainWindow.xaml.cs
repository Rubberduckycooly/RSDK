﻿using AnimationEditor.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Xe.Tools.Wpf.Dialogs;

namespace AnimationEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            List.AllowDrop = true;
        }

        private void MenuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.DefaultExt = "*.bin";
            fd.Filter = "RSDKv5 Animation Files|*.bin|RSDKv3/RSDKv4 Animation Files|*.ani|RSDKv2 Animation Files|*.ani|RSDKv1 (2007 ver) Animation Files|*.ani|RSDKv1 (2006 DC ver) Animation Files|*.ani";
            if (fd.ShowDialog() == true)
            {

                //RSDKvRS and RSDKv1 don't have rotation flags
                if (fd.FilterIndex - 1 > 1) { FlagsSelector.IsEnabled = false;}
                if (fd.FilterIndex - 1 < 2) { FlagsSelector.IsEnabled = true; }

                //For RSDKvRS, RSDKv1 and RSDKv2 & RSDKvB there is no ID and the Delay is always 256, so there is no point to let users change their values
                if (fd.FilterIndex - 1 >= 1) { DelayNUD.IsEnabled = false; idNUD.IsEnabled = false; }
                if (fd.FilterIndex - 1 == 3) { idNUD.IsEnabled = true; IDLabel.Text = "Player"; }
                else { IDLabel.Text = "ID"; }
                if (fd.FilterIndex - 1 == 0) { DelayNUD.IsEnabled = true; idNUD.IsEnabled = true; }
                    ViewModel.FileOpen(fd.FileName,fd.FilterIndex -1);
                
            }
        }

        private void MenuFileSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FileSave();
        }

        private void MenuFileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            fd.DefaultExt = "*.bin";
            fd.Filter = "RSDKv5 Animation Files|*.bin|RSDKv3 and RSDKv4 Animation Files|*.ani|RSDKv2 Animation Files|*.ani|RSDKv1 (2007 ver) Animation Files|*.ani|RSDKv1 (2006 DC ver) Animation Files|*.ani";
            if (fd.ShowDialog() == true)
            {
                ViewModel.FileSave(fd.FileName);
            }
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonAnimationAdd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationAdd();
        }

        private void ButtonAnimationUp_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationUp();
        }

        private void ButtonAnimationDown_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationDown();
        }

        private void ButtonFrameLeft_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FrameLeft();
        }

        private void ButtonFrameRight_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FrameRight();
        }

        private void ButtonAnimationDuplicate_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationDuplicate();
        }

        private void ButtonAnimationRemove_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AnimationRemove();
        }

        private void ButtonAnimationImport_Click(object sender, RoutedEventArgs e)
        {
            var version = ViewModel.AnimationData.Version;
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType;
            switch (version)
            {
                case 1:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk1Animation;
                    break;
                case 2:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk2Animation;
                    break;
                case 3:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk3Animation;
                    break;
                case 5:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk5Animation;
                    break;
                default:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Any;
                    break;
            }

            var fd = Xe.Tools.Wpf.Dialogs.FileDialog.Factory(this,
                Xe.Tools.Wpf.Dialogs.FileDialog.Behavior.Open, fileType, false);
            if (fd.ShowDialog() == true)
            {
                ViewModel.AnimationImport(fd.FileName);
            }
        }

        private void ButtonAnimationExport_Click(object sender, RoutedEventArgs e)
        {
            var version = ViewModel.AnimationData.Version;
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType;
            switch (version)
            {
                case 1:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk1Animation;
                    break;
                case 2:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk2Animation;
                    break;
                case 3:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk3Animation;
                    break;
                case 5:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk5Animation;
                    break;
                default:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Any;
                    break;
            }

            var fd = Xe.Tools.Wpf.Dialogs.FileDialog.Factory(this,
                Xe.Tools.Wpf.Dialogs.FileDialog.Behavior.Save, fileType, false);
            if (fd.ShowDialog() == true)
            {
                ViewModel.AnimationExport(fd.FileName);
            }
        }

        private void ButtonFrameAdd_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FrameAdd();
        }

        private void ButtonFrameDupe_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DupeFrame();
        }

        private void ButtonFrameRemove_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FrameRemove();
        }

        private void ButtonFrameImport_Click(object sender, RoutedEventArgs e)
        {
            var version = ViewModel.AnimationData.Version;
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType;
            switch (version)
            {
                case 1:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk1Frame;
                    break;
                case 2:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk2Frame;
                    break;
                case 3:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk3Frame;
                    break;
                case 5:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk5Frame;
                    break;
                default:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Any;
                    break;
            }

            var fd = Xe.Tools.Wpf.Dialogs.FileDialog.Factory(this,
                Xe.Tools.Wpf.Dialogs.FileDialog.Behavior.Open, fileType, false);
            if (fd.ShowDialog() == true)
            {
                ViewModel.FrameImport(fd.FileName);
            }
        }

        private void ButtonFrameExport_Click(object sender, RoutedEventArgs e)
        {
            var version = ViewModel.AnimationData.Version;
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType;
            switch (version)
            {
                case 1:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk1Frame;
                    break;
                case 2:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk2Frame;
                    break;
                case 3:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk3Frame;
                    break;
                case 5:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Rsdk5Frame;
                    break;
                default:
                    fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.Any;
                    break;
            }

            var fd = Xe.Tools.Wpf.Dialogs.FileDialog.Factory(this,
                Xe.Tools.Wpf.Dialogs.FileDialog.Behavior.Save, fileType, false);
            if (fd.ShowDialog() == true)
            {
                ViewModel.FrameExport(fd.FileName);
            }
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Zoom += 0.25;
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Zoom -= 0.25;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.ViewWidth = e.NewSize.Width;
            ViewModel.ViewHeight = e.NewSize.Height;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel.Zoom += (e.Delta / 120) * 0.25;
        }

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.LoadedAnimVer >= 3)
            {
                var dialog = new SingleInputDialog()
                {
                    Text = ViewModel.SelectedAnimation.Name,
                    Description = "Please select the name of the animation"
                };

                if (dialog.ShowDialog() == true)
                {
                    if (string.IsNullOrWhiteSpace(dialog.Text))
                    {
                        MessageBox.Show("You have specified an empty file name.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (!ViewModel.ChangeCurrentAnimationName(dialog.Text))
                    {
                        MessageBox.Show($"An animation with the name {dialog.Name} already exists.\nPlease specify another name.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuViewTexture_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsAnimationDataLoaded)
            {
                var basePath = Path.Combine(Path.GetDirectoryName(ViewModel.FileName), ViewModel.PathMod);
                new TextureWindow(ViewModel, basePath).Show();
            }
        }

        private void MenuViewHitbox_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsHitboxV3)
                new Hitbox3Window(ViewModel).Show();
            else if (ViewModel.IsHitboxV5)
                new Hitbox5Window(ViewModel).Show();
        }

        private void MenuInfoAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        private void FramesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Frame Index Changing Goes Here
        }

        //
        private void SpriteFrameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "*.txt";
            dialog.Filter = "Text Files|*.txt";

            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    MessageBox.Show("You have specified an empty file name.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else 
                {
                    if (File.Exists(dialog.FileName))
                        File.WriteAllText(dialog.FileName, string.Empty);
                    StreamWriter writer = new StreamWriter(File.OpenWrite(dialog.FileName));
                    for (int i = 0; i < ViewModel.AnimationFrames.Count; ++i)
                    {
                        writer.WriteLine($"\t// Frame {i}");
                        writer.WriteLine($"\tSpriteFrame({ViewModel.AnimationFrames[i].Frame.CenterX}, {ViewModel.AnimationFrames[i].Frame.CenterY}, {ViewModel.AnimationFrames[i].Frame.Width}, {ViewModel.AnimationFrames[i].Frame.Height}, {ViewModel.AnimationFrames[i].Frame.X}, {ViewModel.AnimationFrames[i].Frame.Y})");
                    }
                    writer.Flush();

                    MessageBox.Show($"SpriteFrames have been exported to '{dialog.FileName}'.", "Notice", MessageBoxButton.OK);
                }
            }
        }
    }
}
