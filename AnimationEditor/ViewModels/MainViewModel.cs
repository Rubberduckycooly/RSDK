using AnimationEditor.Services;
using RSDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using Newtonsoft.Json;

namespace AnimationEditor.ViewModels
{
    public class MainViewModel : Xe.Tools.Wpf.BaseNotifyPropertyChanged
    {
        private class DummyHitbox : IHitbox
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }

            public object Clone()
            {
                return new DummyHitbox()
                {
                    Left = Left,
                    Top = Top,
                    Right = Right,
                    Bottom = Bottom
                };
            }

            public void SaveChanges(BinaryWriter writer)
            { }

            public void Read(BinaryReader reader)
            { }

            public void Write(BinaryWriter writer)
            { }
        }

        public int LoadedAnimVer = 5;

        private string _fileName;
        private IAnimation _animationData;
        private IAnimationEntry _selectedAnimation;
        private SpriteService _spriteService;
        private AnimationService _animService;

        public string PathMod { get; set; }

        #region Animation data

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public readonly string TITLE = $"RSDK Animation Editor v{Assembly.GetExecutingAssembly().GetName().Version} ({((AssemblyCompanyAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company})";

        public string Title => string.IsNullOrEmpty(FileName) ? TITLE : $"{TITLE} - {Path.GetFileName(FileName)}";

        public ObservableCollection<string> Textures { get; private set; }

        public ObservableCollection<IAnimationEntry> Animations { get; private set; }

        public IAnimation AnimationData
        {
            get => _animationData;
            set
            {
                _animationData = value;
                var basePath = _fileName != "" ? Path.GetDirectoryName(_fileName) : "";
                basePath = Path.Combine(basePath, PathMod);

                Textures = new ObservableCollection<string>(_animationData.SpriteSheets);
                Animations = new ObservableCollection<IAnimationEntry>(_animationData.GetAnimations());
                bool IsHitboxV1 = LoadedAnimVer == 1;
                IsHitboxV3 = LoadedAnimVer == 3 || LoadedAnimVer == 2;
                IsHitboxV5 = LoadedAnimVer == 5;
                if (IsHitboxV3)
                {
                    if (LoadedAnimVer == 3 || LoadedAnimVer == 2)
                    {
                        HitboxEntries = new ObservableCollection<IHitboxEntry>(_animationData.GetHitboxes());
                        HitboxItems = HitboxEntries != null ? new ObservableCollection<string>(
                            HitboxEntries.Select(x => GetHitboxEntryString(x)))
                            : new ObservableCollection<string>();
                    }
                }
                else if (IsHitboxV5)
                {
                    HitboxTypes = new ObservableCollection<string>(_animationData.HitboxTypes);
                }
                else if (IsHitboxV1) //hacky
                {
                    IsHitboxV5 = true;
                    HitboxTypes = new ObservableCollection<string>(new List<string>() { "Hitbox" });
                }
                ValidateHitboxVisibility();

                _animService = new AnimationService(_animationData);
                _animService.OnFrameChanged += OnFrameChanged;
                _spriteService = new SpriteService(_animationData, basePath);

                OnPropertyChanged(nameof(IsAnimationDataLoaded));
                OnPropertyChanged(nameof(SelectedAnimationIndex));
                OnPropertyChanged(nameof(AnimationCount));
                OnPropertyChanged(nameof(Textures));
                OnPropertyChanged(nameof(Animations));
                OnPropertyChanged(nameof(HitboxEntries));
                OnPropertyChanged(nameof(HitboxItems));
            }
        }

        public bool IsAnimationDataLoaded => AnimationData != null;

        #endregion

        #region Animation view view

        private double _viewWidth, _viewHeight, _zoom = 1.0;

        public double ViewWidth
        {
            get => _viewWidth;
            set
            {
                _viewWidth = value;
                OnPropertyChanged(nameof(SpriteLeft));
                OnPropertyChanged(nameof(SpriteTop));
                OnPropertyChanged(nameof(SpriteRight));
                OnPropertyChanged(nameof(SpriteBottom));
                OnPropertyChanged(nameof(SpriteCenter));
            }
        }

        public double ViewHeight
        {
            get => _viewHeight;
            set
            {
                _viewHeight = value;
                OnPropertyChanged(nameof(SpriteLeft));
                OnPropertyChanged(nameof(SpriteTop));
                OnPropertyChanged(nameof(SpriteRight));
                OnPropertyChanged(nameof(SpriteBottom));
                OnPropertyChanged(nameof(SpriteCenter));
            }
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(Math.Min(value, 24), 0.125);
                OnPropertyChanged();
                InvalidateCanvas();
            }
        }

        public BitmapSource Sprite
        {
            get
            {
                return _spriteService?[SelectedFrameTexture, _animService.CurrentFrame];
            }
        }

        public double SpriteLeft => ViewWidth / 2.0 + _animService?.CurrentFrame?.CenterX ?? 0;
        public double SpriteTop => ViewHeight / 2.0 + _animService?.CurrentFrame?.CenterY ?? 0;
        public double SpriteRight => SpriteLeft + _animService?.CurrentFrame?.Width ?? 0;
        public double SpriteBottom => SpriteTop + _animService?.CurrentFrame?.Height ?? 0;
        public Point SpriteCenter
        {
            get
            {
                var frame = _animService?.CurrentFrame;
                if (frame != null)
                {
                    return new Point((double)-frame.CenterX / frame.Width, (double)-frame.CenterY / frame.Height);
                }
                return new Point(0.5, 0.5);
            }
        }
        public double SpriteScaleX => Zoom;
        public double SpriteScaleY => Zoom;
        
        public bool IsRunning
        {
            get => _animService?.IsRunning ?? false;
            set
            {
                _animService.IsRunning = value;
                OnPropertyChanged(nameof(IsNotRunning));
            }
        }
        public bool IsNotRunning => !IsRunning;

        #endregion

        #region Current animation properties

        public ObservableCollection<FrameViewModel> AnimationFrames { get; private set; }

        public IAnimationEntry SelectedAnimation
        {
            get => _selectedAnimation;
            set
            {
                if (_animService == null)
                    return;

                _selectedAnimation = value;
                _animService.Animation = value?.Name;

                if (_selectedAnimation != null)
                {
                    ChangeAllFrames();
                }
                else
                {
                    AnimationFrames = null;
                }
                OnPropertyChanged(nameof(AnimationFrames));
                OnPropertyChanged(nameof(IsAnimationSelected));
                OnPropertyChanged(nameof(FramesCount));
                OnPropertyChanged(nameof(Speed));
                OnPropertyChanged(nameof(Loop));
                OnPropertyChanged(nameof(Flags));
            }
        }

        public int AnimationCount => Animations?.Count() ?? 0;

        private int _selectedAnimationIndex { get; set; }
        public int SelectedAnimationIndex
        {
            get => _selectedAnimationIndex;
            set
            {
                if (value >= 0)
                {
                    _selectedAnimationIndex = value;
                    OnPropertyChanged(nameof(SelectedAnimationIndex));
                }
            }
        }

        public bool IsFrameSelected => SelectedFrame != null && SelectedAnimation?.GetFrames().Count() > 0;

        public int FramesCount => SelectedAnimation?.GetFrames().Count() ?? 0;

        public int Speed
        {
            get => SelectedAnimation != null ? SelectedAnimation.Speed : 0;
            set => SelectedAnimation.Speed = value;
        }

        public int Loop
        {
            get => SelectedAnimation?.Loop ?? 0;
            set => SelectedAnimation.Loop = value;
        }

        public int Flags
        {
            get
            {
                if ((AnimationData?.Version ?? 0) == 1)
                    return AnimationData?.PlayerType ?? 0;
                else
                    return SelectedAnimation?.Flags ?? 0;
            }

            set
            {
                if ((AnimationData?.Version ?? 0) == 1)
                    AnimationData.PlayerType = value;
                else if (SelectedAnimation != null)
                    SelectedAnimation.Flags = value;
            }
        }

        #endregion

        #region Selected frame
        
        public bool IsAnimationSelected => SelectedAnimation != null;

        public int SelectedFrameIndex
        {
            get => _animService?.FrameIndex ?? 0;
            set
            {
                if (value >= 0)
                {
                    _animService.FrameIndex = value;
                    OnPropertyChanged(nameof(SelectedFrameIndex));
                }
            }
        }

        public IFrame SelectedFrame => _animService?.CurrentFrame;


        /// <summary>
        /// Get or set the texture for the selected animation
        /// </summary>
        public int SelectedFrameTexture
        {
            get => SelectedFrame?.SpriteSheet ?? 0;
            set
            {
                if (SelectedFrame != null)
                {
                    SelectedFrame.SpriteSheet = value;
                    _spriteService.InvalidateAll();
                    ChangeAllFrames();
                }
            }
        }

        public int SelectedFrameHitbox
        {
            get => SelectedFrame?.CollisionBox ?? 0;
            set
            {
                SelectedFrame.CollisionBox = value;
            }
        }

        public int SelectedFrameLeft
        {
            get => SelectedFrame?.X ?? 0;
            set
            {
                SelectedFrame.X = value;
                CurrentFrameChanged();
                InvalidateCanvas();
            }
        }

        public int SelectedFrameTop
        {
            get => SelectedFrame?.Y ?? 0;
            set
            {
                SelectedFrame.Y = value;
                CurrentFrameChanged();
                InvalidateCanvas();
            }
        }

        public int SelectedFrameWidth
        {
            get => SelectedFrame?.Width ?? 0;
            set
            {
                SelectedFrame.Width = value;
                CurrentFrameChanged();
                InvalidateCanvas();
            }
        }

        public int SelectedFrameHeight
        {
            get => SelectedFrame?.Height ?? 0;
            set
            {
                SelectedFrame.Height = value;
                CurrentFrameChanged();
                InvalidateCanvas();
            }
        }

        public int SelectedFramePivotX
        {
            get => SelectedFrame?.CenterX ?? 0;
            set
            {
                SelectedFrame.CenterX = value;
                InvalidateCanvas();
            }
        }

        public int SelectedFramePivotY
        {
            get => SelectedFrame?.CenterY ?? 0;
            set
            {
                SelectedFrame.CenterY = value;
                InvalidateCanvas();
            }
        }

        public int SelectedFrameId
        {
            get
            {
                if (AnimationData == null) return 0;
                if (AnimationData.Version == 5) return (SelectedFrame as RSDK5.Frame)?.Id ?? 0;
                else if (AnimationData.Version == 1) return AnimationData.PlayerType;
                return 0;
            }
            set
            {
                if (AnimationData == null) return;
                if (AnimationData.Version == 5)
                {
                    if (SelectedFrame is RSDK5.Frame frame)
                    {
                        frame.Id = value;
                        InvalidateCanvas();
                    }
                }
                else if (AnimationData.Version == 1)
                {
                    AnimationData.PlayerType = value;
                    InvalidateCanvas();
                }
            }
        }

        public int SelectedFrameDuration
        {
            get => (SelectedFrame)?.Duration ?? 0;
            set
            {
                if (SelectedFrame is RSDK5.Frame frame)
                {
                    frame.Duration = value;
                    InvalidateCanvas();
                }
            }
        }

        #endregion

        #region Hitbox

        #region Hitbox v3
        private bool _isHitboxV3;
        public bool IsHitboxV3
        {
            get => _isHitboxV3;
            set
            {
                _isHitboxV3 = value;
                ValidateHitboxVisibility();
            }
        }
        public bool IsNotHitboxV3 => !IsHitboxV3;
        public Visibility HitboxV3Visibility => IsHitboxV3 ? Visibility.Visible : Visibility.Collapsed;
        public ObservableCollection<IHitboxEntry> HitboxEntries { get; private set; }
        public ObservableCollection<string> HitboxItems { get; private set; }
        #endregion

        #region Hitbox v5
        private bool _isHitboxV5;
        private int _selectedIndex;
        public bool IsHitboxV5
        {
            get => _isHitboxV5;
            set
            {
                _isHitboxV5 = value;
                ValidateHitboxVisibility();
            }
        }
        public bool IsNotHitboxV5 => !IsHitboxV5;
        public Visibility HitboxV5Visibility => IsHitboxV5 ? Visibility.Visible : Visibility.Collapsed;
        public ObservableCollection<string> HitboxTypes { get; set; }
        public int SelectedHitboxType
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(SelectedHitbox));
            }
        }
        public IHitbox SelectedHitbox => SelectedFrame?.GetHitbox(SelectedHitboxType) ?? new DummyHitbox();
        #endregion

        private void ValidateHitboxVisibility()
        {
            OnPropertyChanged(nameof(IsHitboxV3));
            OnPropertyChanged(nameof(IsNotHitboxV3));
            OnPropertyChanged(nameof(HitboxV3Visibility));
            OnPropertyChanged(nameof(HitboxEntries));
            OnPropertyChanged(nameof(HitboxItems));

            OnPropertyChanged(nameof(IsNotHitboxV5));
            OnPropertyChanged(nameof(HitboxV5Visibility));
            OnPropertyChanged(nameof(HitboxTypes));
        }

        #endregion

        #region Methods

        public void InvalidateCanvas()
        {
            OnPropertyChanged(nameof(Sprite));
            OnPropertyChanged(nameof(SpriteLeft));
            OnPropertyChanged(nameof(SpriteTop));
            OnPropertyChanged(nameof(SpriteRight));
            OnPropertyChanged(nameof(SpriteBottom));
            OnPropertyChanged(nameof(SpriteCenter));
            OnPropertyChanged(nameof(SpriteScaleX));
            OnPropertyChanged(nameof(SpriteScaleY));
        }

        public void InvalidateFrameProperties()
        {
            OnPropertyChanged(nameof(IsFrameSelected));
            OnPropertyChanged(nameof(SelectedFrameIndex));
            OnPropertyChanged(nameof(SelectedFrameTexture));
            OnPropertyChanged(nameof(SelectedFrameHitbox));
            OnPropertyChanged(nameof(SelectedFrameLeft));
            OnPropertyChanged(nameof(SelectedFrameTop));
            OnPropertyChanged(nameof(SelectedFrameWidth));
            OnPropertyChanged(nameof(SelectedFrameHeight));
            OnPropertyChanged(nameof(SelectedFramePivotX));
            OnPropertyChanged(nameof(SelectedFramePivotY));
            OnPropertyChanged(nameof(SelectedFrameId));
            OnPropertyChanged(nameof(SelectedFrameDuration));
            OnPropertyChanged(nameof(SelectedHitbox));
        }

        private void OnFrameChanged(AnimationService service)
        {
            InvalidateCanvas();
            InvalidateFrameProperties();
        }

        public bool FileOpen(string fileName, int fi)
        {
            if (File.Exists(fileName))
            {
                var ext = Path.GetExtension(fileName);
                using (var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var reader = new BinaryReader(fStream))
                    {
                        try
                        {
                            FileName = fileName;

                            switch (fi)
                            {
                                case 0: //Sonic Mania (Plus)
                                    PathMod = "..";
                                    LoadedAnimVer = 5;
                                    AnimationData = new RSDK5.Animation(reader);
                                    break;
                                case 1: //Sonic 1, Sonic 2, Sonic CD
                                    PathMod = "..\\sprites";
                                    LoadedAnimVer = 3;
                                    AnimationData = new RSDK3.Animation(reader);
                                    break;
                                case 2: //Sonic Nexus
                                    PathMod = "..\\sprites";
                                    LoadedAnimVer = 2;
                                    AnimationData = new RSDK2.Animation(reader);
                                    break;
                                case 3: //Retro-Sonic
                                case 4: //Retro-Sonic (Dreamcast)
                                    PathMod = "";
                                    LoadedAnimVer = 1;
                                    AnimationData = new RSDK1.Animation(reader, fi == 4);
                                    break;
                                default:
                                    return false;
                            }
                            return true;
                        }
                        catch (Exception e)
                        {
                            FileName = "";
                            PathMod = "..";
                            LoadedAnimVer = 5;
                            AnimationData = new RSDK5.Animation();
                            MessageBox.Show($"Error Opening Animation File!\nError: {e.Message}", "RSDK Animation Editor");
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        public void FileSave(string fileName = null)
        {
            SaveChanges();

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = FileName;

            using (var fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(fStream))
                {
                    _animationData.SaveChanges(writer);
                }
            }
            FileName = fileName;
        }

        public bool Import(string fileName)
        {
            if (!File.Exists(fileName)) return false;

            FileName = fileName;

            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
                {
                    var parsedJson = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(new JsonTextReader(reader));

                    LoadedAnimVer = (int)parsedJson["Anim Version"];

                    switch (LoadedAnimVer)
                    {
                        case 5:
                            PathMod = "..";
                            AnimationData = new RSDK5.Animation();
                            break;
                        case 3:
                            PathMod = "..\\sprites";
                            AnimationData = new RSDK3.Animation();
                            break;
                        case 2:
                            PathMod = "..\\sprites";
                            AnimationData = new RSDK2.Animation();
                            break;
                        case 1:
                            PathMod = "";
                            AnimationData = new RSDK1.Animation();
                            break;
                        default:
                            break;
                    }

                    //Player Type (v1 only)
                    _animationData.PlayerType = (int)(parsedJson["Player Type"] == null ? 0 : parsedJson["Player Type"]);

                    Newtonsoft.Json.Linq.JArray sheets = (Newtonsoft.Json.Linq.JArray)parsedJson["Sheets"];
                    AnimationData.SpriteSheets.Clear();
                    if (sheets != null)
                    {
                        for (int s = 0; s < sheets.Count; ++s)
                        {
                            AnimationData.SpriteSheets.Add((string)sheets[s]);
                        }
                    }

                    //Hitbox Types (v5)
                    Newtonsoft.Json.Linq.JArray hitboxTypes = (Newtonsoft.Json.Linq.JArray)parsedJson["Hitbox Types"];
                    List<string> hitboxTypeList = AnimationData.HitboxTypes.ToList();
                    hitboxTypeList.Clear();
                    if (hitboxTypes != null)
                    {
                        for (int h = 0; h < hitboxTypes.Count; ++h)
                        {
                            hitboxTypeList.Add((string)hitboxTypes[h]);
                        }
                    }
                    AnimationData.SetHitboxTypes(hitboxTypeList);

                    //Hitboxes (v2, v3 & v4)
                    Newtonsoft.Json.Linq.JArray hitboxes = (Newtonsoft.Json.Linq.JArray)parsedJson["Hitboxes"];
                    List<IHitboxEntry> hitboxList = AnimationData.GetHitboxes().ToList();
                    hitboxList.Clear();
                    if (hitboxes != null)
                    {
                        for (int h = 0; h < hitboxes.Count; ++h)
                        {
                            Newtonsoft.Json.Linq.JArray entries = (Newtonsoft.Json.Linq.JArray)hitboxes[h]["Entries"];

                            AnimationData.Factory(out IHitboxEntry hitbox);
                            List<IHitbox> entryList = hitbox.GetHitboxes().ToList();
                            entryList.Clear();
                            if (entries != null)
                            {
                                for (int e = 0; e < entries.Count; ++e)
                                {
                                    RSDK3.Hitbox entry = new RSDK3.Hitbox();
                                    entry.Left = (int)(entries[e]["Left"] == null ? 0 : entries[e]["Left"]);
                                    entry.Top = (int)(entries[e]["Top"] == null ? 0 : entries[e]["Top"]);
                                    entry.Right = (int)(entries[e]["Right"] == null ? 0 : entries[e]["Right"]);
                                    entry.Bottom = (int)(entries[e]["Bottom"] == null ? 0 : entries[e]["Bottom"]);
                                    entryList.Add((IHitbox)entry);
                                }
                            }
                            hitbox.SetHitboxes(entryList);
                            hitboxList.Add(hitbox);
                        }
                    }
                    AnimationData.SetHitboxes(hitboxList);

                    Newtonsoft.Json.Linq.JArray animations = (Newtonsoft.Json.Linq.JArray)parsedJson["Animations"];
                    List<IAnimationEntry> animList = AnimationData.GetAnimations().ToList();
                    animList.Clear();
                    if (animations != null)
                    {
                        for (int a = 0; a < animations.Count; ++a)
                        {
                            AnimationData.Factory(out IAnimationEntry anim);

                            anim.Name = (string)(animations[a]["Name"] == null ? $"Unnamed Animation {a + 1}" : animations[a]["Name"]);
                            anim.Speed = (int)(animations[a]["Animation Speed"] == null ? 0 : animations[a]["Animation Speed"]);
                            anim.Loop = (int)(animations[a]["Loop Index"] == null ? 0 : animations[a]["Loop Index"]);
                            anim.Flags = (int)(animations[a]["Rotation Flags"] == null ? 0 : animations[a]["Rotation Flags"]);

                            Newtonsoft.Json.Linq.JArray frames = (Newtonsoft.Json.Linq.JArray)animations[a]["Frames"];
                            List<IFrame> frameList = anim.GetFrames().ToList();
                            frameList.Clear();
                            if (frames != null)
                            {
                                for (int f = 0; f < frames.Count; ++f)
                                {
                                    _animationData.Factory(out IFrame o);

                                    o.SpriteSheet = (int)(frames[f]["SheetID"] == null ? 0 : frames[f]["SheetID"]);
                                    o.CollisionBox = (int)(frames[f]["HitboxID"] == null ? 0 : frames[f]["HitboxID"]);
                                    o.Id = (int)(frames[f]["ID"] == null ? 0 : frames[f]["ID"]);
                                    o.Duration = (int)(frames[f]["Duration"] == null ? 256 : frames[f]["Duration"]);

                                    if (frames[f]["Src"] != null)
                                    {
                                        o.X = (int)(frames[f]["Src"]["x"] == null ? 0 : frames[f]["Src"]["x"]);
                                        o.Y = (int)(frames[f]["Src"]["y"] == null ? 0 : frames[f]["Src"]["y"]);
                                    }
                                    if (frames[f]["Size"] != null)
                                    {
                                        o.Width = (int)(frames[f]["Size"]["w"] == null ? 0 : frames[f]["Size"]["w"]);
                                        o.Height = (int)(frames[f]["Size"]["h"] == null ? 0 : frames[f]["Size"]["h"]);
                                    }
                                    if (frames[f]["Pivot"] != null)
                                    {
                                        o.CenterX = (int)(frames[f]["Pivot"]["x"] == null ? 0 : frames[f]["Pivot"]["x"]);
                                        o.CenterY = (int)(frames[f]["Pivot"]["y"] == null ? 0 : frames[f]["Pivot"]["y"]);
                                    }

                                    if (LoadedAnimVer == 5)
                                    {
                                        RSDK5.Frame frame = (RSDK5.Frame)o;
                                        if (frames[f]["Hitboxes"] != null)
                                        {
                                            for (int i = 0; i < AnimationData.HitboxTypes.Count(); ++i)
                                            {
                                                if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)] != null)
                                                {
                                                    if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"] != null)
                                                        frame.Hitboxes[i].Left = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"];
                                                    else
                                                        frame.Hitboxes[i].Left = 0;

                                                    if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"] != null)
                                                        frame.Hitboxes[i].Top = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"];
                                                    else
                                                        frame.Hitboxes[i].Top = 0;

                                                    if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"] != null)
                                                        frame.Hitboxes[i].Right = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"];
                                                    else
                                                        frame.Hitboxes[i].Right = 0;

                                                    if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"] != null)
                                                        frame.Hitboxes[i].Bottom = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"];
                                                    else
                                                        frame.Hitboxes[i].Bottom = 0;
                                                }
                                            }
                                        }
                                    }
                                    else if (LoadedAnimVer == 1)
                                    {
                                        RSDK1.Frame frame = (RSDK1.Frame)o;
                                        if (frames[f]["Hitboxes"] != null)
                                        {
                                            if (frames[f]["Hitboxes"]["Default"] != null)
                                            {
                                                if (frames[f]["Hitboxes"]["Default"]["Left"] != null)
                                                    frame.Hitbox.Left = (int)frames[f]["Hitboxes"]["Default"]["Left"];
                                                else
                                                    frame.Hitbox.Left = 0;

                                                if (frames[f]["Hitboxes"]["Default"]["Top"] != null)
                                                    frame.Hitbox.Top = (int)frames[f]["Hitboxes"]["Default"]["Top"];
                                                else
                                                    frame.Hitbox.Top = 0;

                                                if (frames[f]["Hitboxes"]["Default"]["Right"] != null)
                                                    frame.Hitbox.Right = (int)frames[f]["Hitboxes"]["Default"]["Right"];
                                                else
                                                    frame.Hitbox.Right = 0;

                                                if (frames[f]["Hitboxes"]["Default"]["Bottom"] != null)
                                                    frame.Hitbox.Bottom = (int)frames[f]["Hitboxes"]["Default"]["Bottom"];
                                                else
                                                    frame.Hitbox.Bottom = 0;
                                            }
                                        }
                                    }

                                    frameList.Add(o);
                                }
                            }
                            anim.SetFrames(frameList);

                            animList.Add(anim);
                        }
                    }
                    AnimationData.SetAnimations(animList);
                }

                AnimationData = AnimationData; // world's dumbest hack, gets all "onXChanged" events to call
                return true;
            }
            catch (Exception e)
            {
                FileName = "";
                LoadedAnimVer = 5;
                AnimationData = new RSDK5.Animation();
                MessageBox.Show($"Error Importing Json File!\nError: {e.Message}", "RSDK Animation Editor");
                return false;
            }
        }

        public void Export(string fileName)
        {
            SaveChanges();

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();

                writer.WritePropertyName("Anim Version");
                writer.WriteValue(_animationData.Version);

                if (LoadedAnimVer == 1)
                {
                    writer.WritePropertyName("Player Type");
                    writer.WriteValue(_animationData.PlayerType);
                }

                if (_animationData.SpriteSheets.Count() > 0)
                {
                    writer.WritePropertyName("Sheets");
                    writer.WriteStartArray();
                    for (int h = 0; h < _animationData.SpriteSheets.Count(); ++h)
                    {
                        writer.WriteValue(_animationData.SpriteSheets.ElementAt(h));
                    }
                    writer.WriteEndArray();
                }

                if (LoadedAnimVer == 5)
                {
                    if (_animationData.HitboxTypes.Count() > 0)
                    {
                        writer.WritePropertyName("Hitbox Types");
                        writer.WriteStartArray();
                        for (int h = 0; h < _animationData.HitboxTypes.Count(); ++h)
                        {
                            writer.WriteValue(_animationData.HitboxTypes.ElementAt(h));
                        }
                        writer.WriteEndArray();
                    }
                }
                else if (LoadedAnimVer == 1)
                {
                    writer.WritePropertyName("Hitbox Types");
                    writer.WriteStartArray();
                    writer.WriteValue("Default");
                    writer.WriteEndArray();
                }
                else
                {
                    if (_animationData.GetHitboxes().Count() > 0)
                    {
                        if (Settings.Default.exportFullJson) {
                            writer.WritePropertyName("Hitbox Types");
                            writer.WriteStartArray();
                            writer.WriteValue("Default");
                            writer.WriteEndArray();
                        }

                        writer.WritePropertyName("Hitboxes");
                        writer.WriteStartArray();
                        for (int h = 0; h < _animationData.GetHitboxes().Count(); ++h)
                        {
                            writer.WriteStartObject();

                            IHitboxEntry selectedHitbox = _animationData.GetHitboxes().ElementAt(h);
                            if (selectedHitbox.Count > 0)
                            {
                                writer.WritePropertyName("Entries");
                                writer.WriteStartArray();
                                for (int e = 0; e < selectedHitbox.Count; ++e)
                                {
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("Left");
                                    writer.WriteValue(selectedHitbox.GetHitbox(e).Left);
                                    writer.WritePropertyName("Top");
                                    writer.WriteValue(selectedHitbox.GetHitbox(e).Top);
                                    writer.WritePropertyName("Right");
                                    writer.WriteValue(selectedHitbox.GetHitbox(e).Right);
                                    writer.WritePropertyName("Bottom");
                                    writer.WriteValue(selectedHitbox.GetHitbox(e).Bottom);
                                    writer.WriteEndObject();
                                }
                                writer.WriteEndArray();
                            }
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                    }
                }

                if (_animationData.GetAnimations().Count() > 0)
                {
                    writer.WritePropertyName("Animations");
                    writer.WriteStartArray();
                    for (int a = 0; a < _animationData.GetAnimations().Count(); ++a)
                    {
                        IAnimationEntry selectedAnimation = _animationData.GetAnimations().ElementAt(a);
                        writer.WriteStartObject();
                        writer.WritePropertyName("Name");
                        writer.WriteValue(selectedAnimation.Name);
                        writer.WritePropertyName("Animation Speed");
                        writer.WriteValue(selectedAnimation.Speed);
                        writer.WritePropertyName("Loop Index");
                        writer.WriteValue(selectedAnimation.Loop);
                        if (LoadedAnimVer >= 3)
                        {
                            writer.WritePropertyName("Rotation Flags");
                            writer.WriteValue(selectedAnimation.Flags);
                        }

                        if (selectedAnimation.GetFrames().Count() > 0)
                        {
                            writer.WritePropertyName("Frames");
                            writer.WriteStartArray();
                            for (int f = 0; f < selectedAnimation.GetFrames().Count(); ++f)
                            {
                                IFrame selectedFrame = selectedAnimation.GetFrames().ElementAt(f);
                                writer.WriteStartObject();

                                writer.WritePropertyName("SheetID");
                                writer.WriteValue(selectedFrame.SpriteSheet);
                                if (LoadedAnimVer != 5 && LoadedAnimVer != 1)
                                {
                                    writer.WritePropertyName("HitboxID");
                                    writer.WriteValue(selectedFrame.CollisionBox);
                                }
                                else if (LoadedAnimVer == 5)
                                {
                                    writer.WritePropertyName("ID");
                                    writer.WriteValue(selectedFrame.Id);
                                    writer.WritePropertyName("Duration");
                                    writer.WriteValue(selectedFrame.Duration);
                                }

                                writer.WritePropertyName("Src");
                                writer.WriteStartObject();
                                writer.WritePropertyName("x");
                                writer.WriteValue(selectedFrame.X);
                                writer.WritePropertyName("y");
                                writer.WriteValue(selectedFrame.Y);
                                writer.WriteEndObject();

                                writer.WritePropertyName("Size");
                                writer.WriteStartObject();
                                writer.WritePropertyName("w");
                                writer.WriteValue(selectedFrame.Width);
                                writer.WritePropertyName("h");
                                writer.WriteValue(selectedFrame.Height);
                                writer.WriteEndObject();

                                writer.WritePropertyName("Pivot");
                                writer.WriteStartObject();
                                writer.WritePropertyName("x");
                                writer.WriteValue(selectedFrame.CenterX);
                                writer.WritePropertyName("y");
                                writer.WriteValue(selectedFrame.CenterY);
                                writer.WriteEndObject();

                                if (LoadedAnimVer == 5)
                                {
                                    RSDK5.Frame frame = (RSDK5.Frame)selectedFrame;

                                    if (frame.Hitboxes.Count() > 0)
                                    {
                                        writer.WritePropertyName("Hitboxes");
                                        writer.WriteStartObject();
                                        for (int i = 0; i < frame.Hitboxes.Count(); ++i)
                                        {
                                            writer.WritePropertyName(_animationData.HitboxTypes.ElementAt(i));
                                            writer.WriteStartObject();
                                            writer.WritePropertyName("Left");
                                            writer.WriteValue(frame.Hitboxes[i].Left);
                                            writer.WritePropertyName("Top");
                                            writer.WriteValue(frame.Hitboxes[i].Top);
                                            writer.WritePropertyName("Right");
                                            writer.WriteValue(frame.Hitboxes[i].Right);
                                            writer.WritePropertyName("Bottom");
                                            writer.WriteValue(frame.Hitboxes[i].Bottom);
                                            writer.WriteEndObject();
                                        }
                                        writer.WriteEndObject();
                                    }
                                }
                                else if (LoadedAnimVer == 1)
                                {
                                    RSDK1.Frame frame = (RSDK1.Frame)selectedFrame;

                                    writer.WritePropertyName("Hitboxes");
                                    writer.WriteStartObject();
                                    {
                                        writer.WritePropertyName("Default");
                                        writer.WriteStartObject();
                                        writer.WritePropertyName("Left");
                                        writer.WriteValue(frame.Hitbox.Left);
                                        writer.WritePropertyName("Top");
                                        writer.WriteValue(frame.Hitbox.Top);
                                        writer.WritePropertyName("Right");
                                        writer.WriteValue(frame.Hitbox.Right);
                                        writer.WritePropertyName("Bottom");
                                        writer.WriteValue(frame.Hitbox.Bottom);
                                        writer.WriteEndObject();
                                    }
                                    writer.WriteEndObject();
                                }
                                else if (Settings.Default.exportFullJson)
                                {
                                    if (selectedFrame.CollisionBox < _animationData.GetHitboxes().Count() && selectedFrame.CollisionBox >= 0)
                                    {
                                        writer.WritePropertyName("Hitboxes");
                                        writer.WriteStartObject();

                                        List<IHitboxEntry> hitboxList = _animationData.GetHitboxes().ToList();
                                        {
                                            writer.WritePropertyName("Default");
                                            writer.WriteStartObject();
                                            writer.WritePropertyName("Left");
                                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Left);
                                            writer.WritePropertyName("Top");
                                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Top);
                                            writer.WritePropertyName("Right");
                                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Right);
                                            writer.WritePropertyName("Bottom");
                                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Bottom);
                                            writer.WriteEndObject();
                                        }
                                        writer.WriteEndObject();
                                    }
                                }

                                writer.WriteEndObject();
                            }
                            writer.WriteEndArray();
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            if (!fileName.ToLower().EndsWith(".json"))
                fileName += ".json";

            File.WriteAllText(fileName, string.Empty);
            File.WriteAllText(fileName, sb.ToString());
        }

        public void AnimationAdd()
        {
            _animationData.Factory(out IAnimationEntry o);
            Animations.Add(o);
            OnPropertyChanged(nameof(AnimationCount));
        }
        public void AnimationUp()
        {
            var anim = SelectedAnimation;
            if (anim != null)
            {
                var index = Animations.IndexOf(anim);
                if (index == 0)
                    return; // Don't Continue if the entry is first on the list.
                Animations.Insert(index - 1, anim);
                Animations.RemoveAt(index + 1);
                SelectedAnimationIndex = index - 1;
                OnPropertyChanged(nameof(SelectedAnimationIndex));
                SelectedAnimation = anim;
            }
        }

        public void AnimationDown()
        {
            var anim = SelectedAnimation;
            if (anim != null)
            {
                var index = Animations.IndexOf(anim);
                if (index == Animations.Count - 1)
                    return; // Don't Continue if the entry is last on the list.
                Animations.Insert(index + 2, anim);
                Animations.RemoveAt(index);
                SelectedAnimationIndex = index + 1;
                OnPropertyChanged(nameof(SelectedAnimationIndex));
                SelectedAnimation = anim;
            }
        }

        public void AnimationDuplicate()
        {
            var selectedAnimation = SelectedAnimation;
            if (selectedAnimation != null)
            {
                Animations.Add(selectedAnimation.Clone() as IAnimationEntry);
                OnPropertyChanged(nameof(AnimationCount));
            }
        }

        public void AnimationRemove()
        {
            Animations.Remove(SelectedAnimation);
            OnPropertyChanged(nameof(AnimationCount));
        }

        public void AnimationImport(string fileName)
        {
            if (!File.Exists(fileName)) return;

            using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
            {
                var parsedJson = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(new JsonTextReader(reader));

                _animationData.Factory(out IAnimationEntry anim);

                anim.Name = (string)(parsedJson["Name"] == null ? $"Unnamed Animation" : parsedJson["Name"]);
                anim.Speed = (int)(parsedJson["Animation Speed"] == null ? 0 : parsedJson["Animation Speed"]);
                anim.Loop = (int)(parsedJson["Loop Index"] == null ? 0 : parsedJson["Loop Index"]);
                anim.Flags = (int)(parsedJson["Rotation Flags"] == null ? 0 : parsedJson["Rotation Flags"]);

                Newtonsoft.Json.Linq.JArray frames = (Newtonsoft.Json.Linq.JArray)parsedJson["Frames"];
                List<IFrame> frameList = anim.GetFrames().ToList();
                frameList.Clear();
                if (frames != null)
                {
                    for (int f = 0; f < frames.Count; ++f)
                    {
                        _animationData.Factory(out IFrame o);

                        o.SpriteSheet = (int)(frames[f]["SheetID"] == null ? 0 : frames[f]["SheetID"]);
                        o.CollisionBox = (int)(frames[f]["HitboxID"] == null ? 0 : frames[f]["HitboxID"]);
                        o.Id = (int)(frames[f]["ID"] == null ? 0 : frames[f]["ID"]);
                        o.Duration = (int)(frames[f]["Duration"] == null ? 256 : frames[f]["Duration"]);

                        if (frames[f]["Src"] != null)
                        {
                            o.X = (int)(frames[f]["Src"]["x"] == null ? 0 : frames[f]["Src"]["x"]);
                            o.Y = (int)(frames[f]["Src"]["y"] == null ? 0 : frames[f]["Src"]["y"]);
                        }
                        if (frames[f]["Size"] != null)
                        {
                            o.Width = (int)(frames[f]["Size"]["w"] == null ? 0 : frames[f]["Size"]["w"]);
                            o.Height = (int)(frames[f]["Size"]["h"] == null ? 0 : frames[f]["Size"]["h"]);
                        }
                        if (frames[f]["Pivot"] != null)
                        {
                            o.CenterX = (int)(frames[f]["Pivot"]["x"] == null ? 0 : frames[f]["Pivot"]["x"]);
                            o.CenterY = (int)(frames[f]["Pivot"]["y"] == null ? 0 : frames[f]["Pivot"]["y"]);
                        }

                        if (LoadedAnimVer == 5)
                        {
                            RSDK5.Frame frame = (RSDK5.Frame)o;
                            if (frames[f]["Hitboxes"] != null)
                            {
                                for (int i = 0; i < AnimationData.HitboxTypes.Count(); ++i)
                                {
                                    if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)] != null)
                                    {
                                        if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"] != null)
                                            frame.Hitboxes[i].Left = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"];
                                        else
                                            frame.Hitboxes[i].Left = 0;

                                        if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"] != null)
                                            frame.Hitboxes[i].Top = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"];
                                        else
                                            frame.Hitboxes[i].Top = 0;

                                        if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"] != null)
                                            frame.Hitboxes[i].Right = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"];
                                        else
                                            frame.Hitboxes[i].Right = 0;

                                        if (frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"] != null)
                                            frame.Hitboxes[i].Bottom = (int)frames[f]["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"];
                                        else
                                            frame.Hitboxes[i].Bottom = 0;
                                    }
                                }
                            }
                        }
                        else if (LoadedAnimVer == 1)
                        {
                            RSDK1.Frame frame = (RSDK1.Frame)o;
                            if (frames[f]["Hitboxes"] != null)
                            {
                                if (frames[f]["Hitboxes"]["Default"] != null)
                                {
                                    if (frames[f]["Hitboxes"]["Default"]["Left"] != null)
                                        frame.Hitbox.Left = (int)frames[f]["Hitboxes"]["Default"]["Left"];
                                    else
                                        frame.Hitbox.Left = 0;

                                    if (frames[f]["Hitboxes"]["Default"]["Top"] != null)
                                        frame.Hitbox.Top = (int)frames[f]["Hitboxes"]["Default"]["Top"];
                                    else
                                        frame.Hitbox.Top = 0;

                                    if (frames[f]["Hitboxes"]["Default"]["Right"] != null)
                                        frame.Hitbox.Right = (int)frames[f]["Hitboxes"]["Default"]["Right"];
                                    else
                                        frame.Hitbox.Right = 0;

                                    if (frames[f]["Hitboxes"]["Default"]["Bottom"] != null)
                                        frame.Hitbox.Bottom = (int)frames[f]["Hitboxes"]["Default"]["Bottom"];
                                    else
                                        frame.Hitbox.Bottom = 0;
                                }
                            }
                        }

                        frameList.Add(o);
                    }
                }
                anim.SetFrames(frameList);

                Animations.Add(anim);
            }
        }

        public void AnimationExport(string fileName)
        {
            var selectedAnimation = SelectedAnimation;
            if (selectedAnimation == null) return;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                writer.WritePropertyName("Name");
                writer.WriteValue(selectedAnimation.Name);
                writer.WritePropertyName("Animation Speed");
                writer.WriteValue(selectedAnimation.Speed);
                writer.WritePropertyName("Loop Index");
                writer.WriteValue(selectedAnimation.Loop);
                if (LoadedAnimVer >= 3)
                {
                    writer.WritePropertyName("Rotation Flags");
                    writer.WriteValue(selectedAnimation.Flags);
                }

                if (selectedAnimation.GetFrames().Count() > 0)
                {
                    writer.WritePropertyName("Frames");
                    writer.WriteStartArray();
                    for (int f = 0; f < selectedAnimation.GetFrames().Count(); ++f)
                    {
                        IFrame selectedFrame = selectedAnimation.GetFrames().ElementAt(f);
                        writer.WriteStartObject();

                        writer.WritePropertyName("SheetID");
                        writer.WriteValue(selectedFrame.SpriteSheet);
                        if (LoadedAnimVer != 5 && LoadedAnimVer != 1)
                        {
                            writer.WritePropertyName("HitboxID");
                            writer.WriteValue(selectedFrame.CollisionBox);
                        }
                        else if (LoadedAnimVer == 5)
                        {
                            writer.WritePropertyName("ID");
                            writer.WriteValue(selectedFrame.Id);
                            writer.WritePropertyName("Duration");
                            writer.WriteValue(selectedFrame.Duration);
                        }

                        writer.WritePropertyName("Src");
                        writer.WriteStartObject();
                        writer.WritePropertyName("x");
                        writer.WriteValue(selectedFrame.X);
                        writer.WritePropertyName("y");
                        writer.WriteValue(selectedFrame.Y);
                        writer.WriteEndObject();

                        writer.WritePropertyName("Size");
                        writer.WriteStartObject();
                        writer.WritePropertyName("w");
                        writer.WriteValue(selectedFrame.Width);
                        writer.WritePropertyName("h");
                        writer.WriteValue(selectedFrame.Height);
                        writer.WriteEndObject();

                        writer.WritePropertyName("Pivot");
                        writer.WriteStartObject();
                        writer.WritePropertyName("x");
                        writer.WriteValue(selectedFrame.CenterX);
                        writer.WritePropertyName("y");
                        writer.WriteValue(selectedFrame.CenterY);
                        writer.WriteEndObject();

                        if (LoadedAnimVer == 5)
                        {
                            RSDK5.Frame frame = (RSDK5.Frame)selectedFrame;

                            if (frame.Hitboxes.Count() > 0)
                            {
                                writer.WritePropertyName("Hitboxes");
                                writer.WriteStartObject();
                                for (int i = 0; i < frame.Hitboxes.Count(); ++i)
                                {
                                    writer.WritePropertyName(_animationData.HitboxTypes.ElementAt(i));
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("Left");
                                    writer.WriteValue(frame.Hitboxes[i].Left);
                                    writer.WritePropertyName("Top");
                                    writer.WriteValue(frame.Hitboxes[i].Top);
                                    writer.WritePropertyName("Right");
                                    writer.WriteValue(frame.Hitboxes[i].Right);
                                    writer.WritePropertyName("Bottom");
                                    writer.WriteValue(frame.Hitboxes[i].Bottom);
                                    writer.WriteEndObject();
                                }
                                writer.WriteEndObject();
                            }
                        }
                        else if (LoadedAnimVer == 1)
                        {
                            RSDK1.Frame frame = (RSDK1.Frame)selectedFrame;

                            writer.WritePropertyName("Hitboxes");
                            writer.WriteStartObject();
                            {
                                writer.WritePropertyName("Default");
                                writer.WriteStartObject();
                                writer.WritePropertyName("Left");
                                writer.WriteValue(frame.Hitbox.Left);
                                writer.WritePropertyName("Top");
                                writer.WriteValue(frame.Hitbox.Top);
                                writer.WritePropertyName("Right");
                                writer.WriteValue(frame.Hitbox.Right);
                                writer.WritePropertyName("Bottom");
                                writer.WriteValue(frame.Hitbox.Bottom);
                                writer.WriteEndObject();
                            }
                            writer.WriteEndObject();
                        }
                        else if (Settings.Default.exportFullJson)
                        {
                            if (selectedFrame.CollisionBox < _animationData.GetHitboxes().Count() && selectedFrame.CollisionBox >= 0)
                            {
                                writer.WritePropertyName("Hitboxes");
                                writer.WriteStartObject();

                                List<IHitboxEntry> hitboxList = _animationData.GetHitboxes().ToList();
                                {
                                    writer.WritePropertyName("Default");
                                    writer.WriteStartObject();
                                    writer.WritePropertyName("Left");
                                    writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Left);
                                    writer.WritePropertyName("Top");
                                    writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Top);
                                    writer.WritePropertyName("Right");
                                    writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Right);
                                    writer.WritePropertyName("Bottom");
                                    writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Bottom);
                                    writer.WriteEndObject();
                                }
                                writer.WriteEndObject();
                            }
                        }

                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            if (!fileName.ToLower().EndsWith(".json"))
                fileName += ".json";

            File.WriteAllText(fileName, string.Empty);
            File.WriteAllText(fileName, sb.ToString());
        }

        public void FrameAdd()
        {
            _animationData.Factory(out IFrame frame);
            FrameAdd(frame);
        }

        public void FrameLeft()
        {
            var frame = SelectedFrame;
            var list = SelectedAnimation.GetFrames().ToList();
            if (frame != null)
            {
                var index = list.IndexOf(frame);
                if (index == 0)
                    return; // Don't Continue if the entry is first on the list.
                list.Insert(index - 1, frame);
                list.RemoveAt(index + 1);
                SelectedAnimation.SetFrames(list);
                SelectedFrameIndex = index - 1;
                ChangeAllFrames();
            }
        }

        public void FrameRight()
        {
            var frame = SelectedFrame;
            var list = SelectedAnimation.GetFrames().ToList();
            if (frame != null)
            {
                var index = list.IndexOf(frame);
                if (index == list.Count - 1)
                    return; // Don't Continue if the entry is last on the list.
                list.Insert(index + 2, frame);
                list.RemoveAt(index);
                SelectedAnimation.SetFrames(list);
                SelectedFrameIndex = index + 1;
                ChangeAllFrames();
            }
        }
        public void FrameAdd(IFrame frame, int? insertOnIndex = null)
        {
            var frameVm = new FrameViewModel(_spriteService, frame);
            var list = SelectedAnimation.GetFrames().ToList();
            if (!insertOnIndex.HasValue)
            {
                AnimationFrames.Add(frameVm);
                list.Add(frame);
            }
            else
            {
                AnimationFrames.Insert(insertOnIndex.Value, frameVm);
                list.Insert(insertOnIndex.Value, frame);
            }
            SelectedAnimation.SetFrames(list);
        }

        public void DupeFrame()
        {
            var selectedFrame = SelectedFrame;
            if (selectedFrame != null)
                FrameAdd(selectedFrame.Clone() as IFrame, SelectedFrameIndex);
        }

        public void FrameRemove()
        {
            if (SelectedFrameIndex >= 0)
            {
                AnimationFrames.RemoveAt(SelectedFrameIndex);
                var frames = SelectedAnimation.GetFrames().ToList();
                if (frames.Count > 0)
                {
                    frames.RemoveAt(SelectedFrameIndex);
                    SelectedAnimation.SetFrames(frames);
                    SelectedFrameIndex = frames.Count - 1;
                    OnPropertyChanged(nameof(IsFrameSelected));
                }
            }
        }

        public void FrameImport(string fileName)
        {
            if (!File.Exists(fileName)) return;
            var selectedAnimation = SelectedAnimation;
            if (selectedAnimation == null) return;

            using (StreamReader reader = new StreamReader(File.OpenRead(fileName)))
            {
                _animationData.Factory(out IFrame o);

                var parsedJson = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.Linq.JToken.ReadFrom(new JsonTextReader(reader));

                o.SpriteSheet = (int)(parsedJson["SheetID"] == null ? 0 : parsedJson["SheetID"]);
                o.CollisionBox = (int)(parsedJson["HitboxID"] == null ? 0 : parsedJson["HitboxID"]);
                o.Id = (int)(parsedJson["ID"] == null ? 0 : parsedJson["ID"]);
                o.Duration = (int)(parsedJson["Duration"] == null ? 256 : parsedJson["Duration"]);

                if (parsedJson["Src"] != null)
                {
                    o.X = (int)(parsedJson["Src"]["x"] == null ? 0 : parsedJson["Src"]["x"]);
                    o.Y = (int)(parsedJson["Src"]["y"] == null ? 0 : parsedJson["Src"]["y"]);
                }
                if (parsedJson["Size"] != null)
                {
                    o.Width = (int)(parsedJson["Size"]["w"] == null ? 0 : parsedJson["Size"]["w"]);
                    o.Height = (int)(parsedJson["Size"]["h"] == null ? 0 : parsedJson["Size"]["h"]);
                }
                if (parsedJson["Pivot"] != null)
                {
                    o.CenterX = (int)(parsedJson["Pivot"]["x"] == null ? 0 : parsedJson["Pivot"]["x"]);
                    o.CenterY = (int)(parsedJson["Pivot"]["y"] == null ? 0 : parsedJson["Pivot"]["y"]);
                }

                if (LoadedAnimVer == 5)
                {
                    RSDK5.Frame frame = (RSDK5.Frame)o;
                    if (parsedJson["Hitboxes"] != null)
                    {
                        for (int i = 0; i < AnimationData.HitboxTypes.Count(); ++i)
                        {
                            if (parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)] != null)
                            {
                                if (parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"] != null)
                                    frame.Hitboxes[i].Left = (int)parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Left"];
                                else
                                    frame.Hitboxes[i].Left = 0;

                                if (parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"] != null)
                                    frame.Hitboxes[i].Top = (int)parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Top"];
                                else
                                    frame.Hitboxes[i].Top = 0;

                                if (parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"] != null)
                                    frame.Hitboxes[i].Right = (int)parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Right"];
                                else
                                    frame.Hitboxes[i].Right = 0;

                                if (parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"] != null)
                                    frame.Hitboxes[i].Bottom = (int)parsedJson["Hitboxes"][AnimationData.HitboxTypes.ElementAt(i)]["Bottom"];
                                else
                                    frame.Hitboxes[i].Bottom = 0;
                            }
                        }
                    }
                }
                else if (LoadedAnimVer == 1)
                {
                    RSDK1.Frame frame = (RSDK1.Frame)o;
                    if (parsedJson["Hitboxes"] != null)
                    {
                        if (parsedJson["Hitboxes"]["Default"] != null)
                        {
                            if (parsedJson["Hitboxes"]["Default"]["Left"] != null)
                                frame.Hitbox.Left = (int)parsedJson["Hitboxes"]["Default"]["Left"];
                            else
                                frame.Hitbox.Left = 0;

                            if (parsedJson["Hitboxes"]["Default"]["Top"] != null)
                                frame.Hitbox.Top = (int)parsedJson["Hitboxes"]["Default"]["Top"];
                            else
                                frame.Hitbox.Top = 0;

                            if (parsedJson["Hitboxes"]["Default"]["Right"] != null)
                                frame.Hitbox.Right = (int)parsedJson["Hitboxes"]["Default"]["Right"];
                            else
                                frame.Hitbox.Right = 0;

                            if (parsedJson["Hitboxes"]["Default"]["Bottom"] != null)
                                frame.Hitbox.Bottom = (int)parsedJson["Hitboxes"]["Default"]["Bottom"];
                            else
                                frame.Hitbox.Bottom = 0;
                        }
                    }
                }

                FrameAdd(o, SelectedFrameIndex);
            }
        }

        public void FrameExport(string fileName)
        {
            var selectedFrame = SelectedFrame;
            if (selectedFrame == null) return;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                //writer.WritePropertyName("Frame");
                writer.WriteStartObject();

                writer.WritePropertyName("SheetID");
                writer.WriteValue(selectedFrame.SpriteSheet);
                if (LoadedAnimVer != 5 && LoadedAnimVer != 1)
                {
                    writer.WritePropertyName("HitboxID");
                    writer.WriteValue(selectedFrame.CollisionBox);
                }
                else if (LoadedAnimVer == 5)
                {
                    writer.WritePropertyName("ID");
                    writer.WriteValue(selectedFrame.Id);
                    writer.WritePropertyName("Duration");
                    writer.WriteValue(selectedFrame.Duration);
                }

                writer.WritePropertyName("Src");
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(selectedFrame.X);
                writer.WritePropertyName("y");
                writer.WriteValue(selectedFrame.Y);
                writer.WriteEndObject();

                writer.WritePropertyName("Size");
                writer.WriteStartObject();
                writer.WritePropertyName("w");
                writer.WriteValue(selectedFrame.Width);
                writer.WritePropertyName("h");
                writer.WriteValue(selectedFrame.Height);
                writer.WriteEndObject();

                writer.WritePropertyName("Pivot");
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(selectedFrame.CenterX);
                writer.WritePropertyName("y");
                writer.WriteValue(selectedFrame.CenterY);
                writer.WriteEndObject();

                if (LoadedAnimVer == 5)
                {
                    RSDK5.Frame frame = (RSDK5.Frame)selectedFrame;

                    if (frame.Hitboxes.Count() > 0)
                    {
                        writer.WritePropertyName("Hitboxes");
                        writer.WriteStartObject();
                        for (int i = 0; i < frame.Hitboxes.Count(); ++i)
                        {
                            writer.WritePropertyName(_animationData.HitboxTypes.ElementAt(i));
                            writer.WriteStartObject();
                            writer.WritePropertyName("Left");
                            writer.WriteValue(frame.Hitboxes[i].Left);
                            writer.WritePropertyName("Top");
                            writer.WriteValue(frame.Hitboxes[i].Top);
                            writer.WritePropertyName("Right");
                            writer.WriteValue(frame.Hitboxes[i].Right);
                            writer.WritePropertyName("Bottom");
                            writer.WriteValue(frame.Hitboxes[i].Bottom);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndObject();
                    }
                }
                else if (LoadedAnimVer == 1)
                {
                    RSDK1.Frame frame = (RSDK1.Frame)selectedFrame;

                    writer.WritePropertyName("Hitboxes");
                    writer.WriteStartObject();
                    {
                        writer.WritePropertyName("Default");
                        writer.WriteStartObject();
                        writer.WritePropertyName("Left");
                        writer.WriteValue(frame.Hitbox.Left);
                        writer.WritePropertyName("Top");
                        writer.WriteValue(frame.Hitbox.Top);
                        writer.WritePropertyName("Right");
                        writer.WriteValue(frame.Hitbox.Right);
                        writer.WritePropertyName("Bottom");
                        writer.WriteValue(frame.Hitbox.Bottom);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                }
                else if (Settings.Default.exportFullJson)
                {
                    if (selectedFrame.CollisionBox < _animationData.GetHitboxes().Count() && selectedFrame.CollisionBox >= 0)
                    {
                        writer.WritePropertyName("Hitboxes");
                        writer.WriteStartObject();

                        List<IHitboxEntry> hitboxList = _animationData.GetHitboxes().ToList();
                        {
                            writer.WritePropertyName("Default");
                            writer.WriteStartObject();
                            writer.WritePropertyName("Left");
                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Left);
                            writer.WritePropertyName("Top");
                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Top);
                            writer.WritePropertyName("Right");
                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Right);
                            writer.WritePropertyName("Bottom");
                            writer.WriteValue(hitboxList[selectedFrame.CollisionBox].GetHitbox(0).Bottom);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndObject();
                    }
                }

                writer.WriteEndObject();
            }

            if (!fileName.ToLower().EndsWith(".json"))
                fileName += ".json";

            File.WriteAllText(fileName, string.Empty);
            File.WriteAllText(fileName, sb.ToString());
        }

        public void CurrentFrameChanged()
        {
            var curAnim = SelectedAnimation;
            var curFrameIndex = SelectedFrameIndex;
            if (curAnim != null && curFrameIndex >= 0 &&
                curFrameIndex < curAnim.GetFrames().Count())
            {
                var animationFrames = AnimationFrames;
                var item = animationFrames[curFrameIndex];
                animationFrames.RemoveAt(curFrameIndex);
                animationFrames.Insert(curFrameIndex, item);
                SelectedFrameIndex = curFrameIndex;
                _spriteService.Invalidate(SelectedFrameTexture, item.Frame);
                OnPropertyChanged(nameof(SelectedFrameIndex));
                
            }
        }

        private void ChangeAllFrames()
        {
            AnimationFrames = new ObservableCollection<FrameViewModel>(
                _selectedAnimation.GetFrames()
                    .Select(x => new FrameViewModel(_spriteService, x)));
            OnPropertyChanged(nameof(AnimationFrames));
        }

        internal static string GetHitboxEntryString(IHitboxEntry entry)
        {
            return entry.Count >= 0 ? GetHitboxString(entry.GetHitbox(0)) : "???";
        }
        internal static string GetHitboxString(IHitbox hb)
        {
            return $"({hb.Left}, {hb.Top}, {hb.Right}, {hb.Bottom})";
        }

        public bool ChangeCurrentAnimationName(string name)
        {
            if (Animations.Any(x => x.Name == name))
                return false;

            SelectedAnimation.Name = name;
            var index = SelectedAnimationIndex;
            var item = Animations[index];
            Animations.RemoveAt(index);
            Animations.Insert(index, item);
            SelectedAnimationIndex = index;
            OnPropertyChanged(nameof(SelectedAnimationIndex));
            return true;
        }

        public void SaveChanges()
        {
            _animationData.SpriteSheets.Clear();
            _animationData.SpriteSheets.AddRange(Textures);
            _animationData.SetAnimations(Animations);
            if (IsHitboxV3)
            {
                _animationData.SetHitboxes(HitboxEntries);
            }
            else if (IsHitboxV5)
            {
                _animationData.SetHitboxTypes(HitboxTypes);
            }
        }
        #endregion
    }
}
