using AnimationEditor.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Xe.Tools.Wpf.Dialogs;

namespace AnimationEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        private IList<MenuItem> RecentItems = new List<MenuItem>(); //menu items for recent files

        public MainWindow()
        {
            Settings.Init();
            InitializeComponent();
            DataContext = new MainViewModel();
            MenuFileClose_Click(null, null); // makes it look neater (v5 UI instead of v3 one)
            List.AllowDrop = true;
            Closing += OnWindowClosing;
            MenuExportFullJson.IsChecked = Settings.Default.exportFullJson;
            UpdateRecentsDropDown();
        }

        public void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Save();
        }

        //Sets up the UI based on the anim ver loaded
        public void SetupUI()
        {
            MenuFileSave.IsEnabled = true;
            MenuFileSaveAs.IsEnabled = true;
            MenuFileExportJson.IsEnabled = true;
            MenuView.IsEnabled = true;
            MenuExportSpriteFrames.IsEnabled = true;

            //RSDKv2 doesn't have rotation flags
            FlagsSelector.IsEnabled = true;
            if (ViewModel.LoadedAnimVer == 2) { FlagsSelector.IsEnabled = false; }

            if (ViewModel.LoadedAnimVer == 1)
            {
                RotFlag_Label.Text = "Player ID";

                string[] playerIDs = new string[] {
                    "Sonic",
                    "Tails",
                    "Knuckles"
                };

                int flag = ViewModel.AnimationData.PlayerType;
                FlagsSelector.Items.Clear();
                for (int i = 0; i < playerIDs.Length; ++i)
                    FlagsSelector.Items.Add(playerIDs[i]);
                ViewModel.AnimationData.PlayerType = flag;
                FlagsSelector.SelectedIndex = flag;

                MenuViewHitbox.IsEnabled = false; //v1 doesn't have hitboxes like that
            }
            else
            {
                RotFlag_Label.Text = "Rotation Flags";

                string[] rotationFlags = new string[] {
                    "No Rotation",
                    "Full Rotation",
                    "Snap to multiples of 45 Degrees",
                    "Static rotation using extra frames",
                };
                if (ViewModel.LoadedAnimVer == 5)
                {
                    rotationFlags = new string[] {
                        "No Rotation",
                        "Full Rotation",
                        "Snap to multiples of 45 Degrees",
                        "Snap to multiples of 90 degrees",
                        "Snap to multiples of 180 degrees",
                        "Static rotation using extra frames",
                    };
                }

                int flag = 0;
                if (ViewModel.AnimationData.GetAnimations().ToList().Count > 0)
                    flag = ViewModel.AnimationData.GetAnimations().ToList()[0].Flags;

                FlagsSelector.Items.Clear();
                for (int i = 0; i < rotationFlags.Length; ++i)
                    FlagsSelector.Items.Add(rotationFlags[i]);

                if (ViewModel.AnimationData.GetAnimations().ToList().Count > 0)
                {
                    ViewModel.AnimationData.GetAnimations().ToList()[0].Flags = flag;
                    FlagsSelector.SelectedIndex = flag;
                }

                MenuViewHitbox.IsEnabled = true; 
            }

            //For RSDKv1, RSDKv2 and RSDKv3 & RSDKv4 there is no ID and the Delay is always 256, so there is no point to let users change their values
            DelayNUD.IsEnabled = false;
            idNUD.IsEnabled = false;
            if (ViewModel.LoadedAnimVer == 5) 
            { 
                DelayNUD.IsEnabled = true; 
                idNUD.IsEnabled = true; 
            }
        }

        private void MenuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.DefaultExt = "*.bin";
            fd.Filter = "RSDKv5 Animation Files|*.bin|RSDKv3/RSDKv4 Animation Files|*.ani|RSDKv2 Animation Files|*.ani|RSDKv1 (2007 ver) Animation Files|*.ani|RSDKv1 (2006 DC ver) Animation Files|*.ani";
            if (fd.ShowDialog() == true)
            {
                MenuFileSave.IsEnabled = false;
                MenuFileSaveAs.IsEnabled = false;
                MenuFileExportJson.IsEnabled = false;
                MenuView.IsEnabled = false;
                MenuExportSpriteFrames.IsEnabled = false;
                bool success = ViewModel.FileOpen(fd.FileName, fd.FilterIndex - 1);
                if (success)
                {
                    AddRecentFile(fd.FileName, ViewModel.LoadedAnimVer);
                    SetupUI();
                }
                else
                {
                    MenuFileClose_Click(sender, e);
                }
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
                AddRecentFile(fd.FileName, ViewModel.LoadedAnimVer);
                SetupUI();
            }
        }

        private void MenuFileImportJson_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.DefaultExt = "*.json";
            fd.Filter = "Json Files|*.json"; 
            if (fd.ShowDialog() == true)
            {
                MenuFileSave.IsEnabled = false;
                MenuFileSaveAs.IsEnabled = false;
                MenuFileExportJson.IsEnabled = false;
                MenuView.IsEnabled = false;
                MenuExportSpriteFrames.IsEnabled = false;
                bool success = ViewModel.Import(fd.FileName);
                if (success)
                {
                    AddRecentFile(fd.FileName, 0);
                    SetupUI();
                }
                else
                {
                    MenuFileClose_Click(sender, e);
                }
            }
        }

        private void MenuFileExportJson_Click(object sender, RoutedEventArgs e)
        {
            var fd = new SaveFileDialog();
            fd.DefaultExt = "*.json";
            fd.Filter = "Json Files|*.json";
            if (fd.ShowDialog() == true)
            {
                ViewModel.Export(fd.FileName);
                AddRecentFile(fd.FileName, 0);
                SetupUI();
            }
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MenuFileClose_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FileName = "";
            ViewModel.PathMod = "";
            ViewModel.LoadedAnimVer = 5;
            ViewModel.AnimationData = new RSDK5.Animation();
            ViewModel.InvalidateCanvas();
            ViewModel.InvalidateFrameProperties();
            MenuFileSave.IsEnabled = false;
            MenuFileSaveAs.IsEnabled = false;
            MenuFileExportJson.IsEnabled = false;
            MenuView.IsEnabled = false;
            MenuExportSpriteFrames.IsEnabled = false;
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

        private void ButtonRenameAnim_Click(object sender, RoutedEventArgs e)
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
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.jsonFile;

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
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.jsonFile;

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
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.jsonFile;

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
            Xe.Tools.Wpf.Dialogs.FileDialog.Type fileType = Xe.Tools.Wpf.Dialogs.FileDialog.Type.jsonFile;

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
            if (ViewModel.AnimationData.Version == 3 || ViewModel.AnimationData.Version == 2)
                new Hitbox3Window(ViewModel).Show();
            else if (ViewModel.AnimationData.Version == 5)
                new Hitbox5Window(ViewModel).Show();
        }

        private void MenuInfoAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        
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


        private void MenuExportFullJson_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.exportFullJson = !Settings.Default.exportFullJson;
            MenuExportFullJson.IsChecked = Settings.Default.exportFullJson;

            Settings.Save();
        }

        // TODO: this maybe?
        // Add this under "<MenuItem x:Name="MenuExportSpriteFrames"" for it to all work
        // No clue how to add these features with this wonky rendering system
        // <Separator HorizontalAlignment="Left" Height="2" Width="220"/>
        // <MenuItem x:Name="MenuShowTransColour" Header="Show Transparent Colour" Click="MenuShowTransColour_Click"/>
        // <MenuItem x:Name="MenuShowHitbox" Header="Show Hitbox" Click="MenuShowHitbox_Click"/>

        private void MenuShowTransColour_Click(object sender, RoutedEventArgs e)
        {
            //MenuShowTransColour.IsChecked = !MenuShowTransColour.IsChecked;
        }
        private void MenuShowHitbox_Click(object sender, RoutedEventArgs e)
        {
            //MenuShowHitbox.IsChecked = !MenuShowHitbox.IsChecked;
        }


        #region Recent Files
        //adds a new recent file
        public void AddRecentFile(string filename, int format)
        {
            if (Settings.Default.RecentFiles == null)
                Settings.Default.RecentFiles = new List<Settings.Instance.RecentFile>();

            if (Settings.Default.RecentFiles.Exists(x => x.FilePath == filename))
                Settings.Default.RecentFiles.RemoveAll(x => x.FilePath == filename);

            if (Settings.Default.RecentFiles.Count >= 10)
            {
                for (int i = 9; i < Settings.Default.RecentFiles.Count; i++)
                    Settings.Default.RecentFiles.RemoveAt(i);
            }

            Settings.Default.RecentFiles.Insert(0, new Settings.Instance.RecentFile(filename, format));

            Settings.Save();
            Settings.Reload();

            UpdateRecentsDropDown();
        }
        //creates a new recent file menu item and returns it
        private MenuItem CreateRecentFileMenuLink(Settings.Instance.RecentFile file)
        {
            MenuItem newItem = new MenuItem();
            newItem.Header = file.FilePath;
            newItem.Tag = file;
            newItem.Click += RecentFileClicked;
            return newItem;
        }
        //clean up the recent files list
        private void CleanUpRecentList()
        {
            foreach (var menuItem in RecentItems)
            {
                menuItem.Click -= RecentFileClicked;
                MenuFileRecent.Items.Remove(menuItem);
            }

            List<string> ItemsForRemoval = new List<string>();

            for (int i = 0; i < Settings.Default.RecentFiles.Count; i++)
            {
                if (File.Exists(Settings.Default.RecentFiles[i].FilePath)) continue;
                else ItemsForRemoval.Add(Settings.Default.RecentFiles[i].FilePath);
            }
            foreach (string item in ItemsForRemoval)
            {
                Settings.Default.RecentFiles.RemoveAll(x => x.FilePath == item);
            }

            RecentItems.Clear();
        }
        //refresh recent files drop down menu
        public void UpdateRecentsDropDown()
        {
            if (Settings.Default.RecentFiles?.Count > 0)
            {
                NoRecentFiles.Visibility = Visibility.Collapsed;
                CleanUpRecentList();

                var startRecentItems = MenuFileRecent.Items.IndexOf(NoRecentFiles);

                foreach (var recent in Settings.Default.RecentFiles)
                {
                    RecentItems.Add(CreateRecentFileMenuLink(recent));
                }

                foreach (MenuItem menuItem in RecentItems.Reverse())
                {
                    MenuFileRecent.Items.Insert(startRecentItems, menuItem);
                }
            }
            else
            {
                NoRecentFiles.Visibility = Visibility.Visible;
            }
        }

        //Event called when a recent file item is clicked
        public void RecentFileClicked(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            int index = MenuFileRecent.Items.IndexOf(menuItem);
            var recent = Settings.Default.RecentFiles[index];

            if (File.Exists(recent.FilePath))
            {
                bool success = false;
                switch (recent.Format)
                {
                    default: break;
                    case 5: success = ViewModel.FileOpen(recent.FilePath, 0); break;
                    case 3: success = ViewModel.FileOpen(recent.FilePath, 1); break;
                    case 2: success = ViewModel.FileOpen(recent.FilePath, 2); break;
                    case 1: success = ViewModel.FileOpen(recent.FilePath, 3); break;
                    case 0: success = ViewModel.Import(recent.FilePath); break;
                }

                if (success)
                {
                    AddRecentFile(recent.FilePath, recent.Format);
                    SetupUI();
                }
                else
                {
                    MenuFileClose_Click(sender, e);
                }
            }
            else
            {
                MessageBox.Show($"The specified File \"{recent.FilePath}\" is not valid.",
                                "Invalid Annimation File!",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Settings.Default.RecentFiles.RemoveAt(index);
                UpdateRecentsDropDown();
            }
            Settings.Save();
        }
        #endregion
    }
}
