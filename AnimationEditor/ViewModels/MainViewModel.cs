﻿using AnimationEditor.Services;
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
                var basePath = Path.GetDirectoryName(_fileName);
                basePath = Path.Combine(basePath, PathMod);

                Textures = new ObservableCollection<string>(_animationData.SpriteSheets);
                Animations = new ObservableCollection<IAnimationEntry>(_animationData.GetAnimations());
                IsHitboxV3 = _animationData.HitboxTypes == null;
                IsHitboxV5 = !IsHitboxV3;
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
                ValidateHitboxVisibility();

                _animService = new AnimationService(_animationData);
                _animService.OnFrameChanged += OnFrameChanged;
                _spriteService = new SpriteService(_animationData, basePath);

                OnPropertyChanged(nameof(IsAnimationDataLoaded));
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
                _zoom = Math.Max(Math.Min(value, 16), 0.25);
                OnPropertyChanged();
                InvalidateCanvas();
            }
        }

        public BitmapSource Sprite => _spriteService?[SelectedFrameTexture, _animService.CurrentFrame];

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

        public int SelectedAnimationIndex { get; set; }

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
            get => SelectedAnimation?.Flags ?? 0;
            set => SelectedAnimation.Flags = value;
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

        #region Hitbox v1
        private bool _isHitboxV1;
        public bool IsHitboxV1
        {
            get => _isHitboxV1;
            set
            {
                _isHitboxV1 = value;
                ValidateHitboxVisibility();
            }
        }
        public bool IsNotHitboxV1 => !IsHitboxV1;
        public Visibility HitboxV1Visibility => IsHitboxV1 ? Visibility.Visible : Visibility.Collapsed;
        public ObservableCollection<IHitboxEntry> HitboxEntriesV1 { get; private set; }
        public ObservableCollection<string> HitboxItemsV1 { get; private set; }
        #endregion

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
            OnPropertyChanged(nameof(IsHitboxV1));
            OnPropertyChanged(nameof(IsNotHitboxV1));
            OnPropertyChanged(nameof(HitboxV1Visibility));
            OnPropertyChanged(nameof(HitboxEntriesV1));
            OnPropertyChanged(nameof(HitboxItemsV1));

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

        private void InvalidateCanvas()
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
                        FileName = fileName;

                        switch (fi)
                        {
                            case 0:
                                PathMod = "..";
                                LoadedAnimVer = 5;
                                AnimationData = new RSDK5.Animation(reader);
                                return false;
                            case 1:
                                PathMod = "..\\sprites";
                                LoadedAnimVer = 3;
                                AnimationData = new RSDK3.Animation(reader);
                                break;
                            case 2:
                                {
                                    byte typeCheck = 0, typeCheck2 = 0;
                                    PathMod = "..\\sprites";
                                    LoadedAnimVer = 2;
                                    bool bf = false;
                                    if (typeCheck == 255 && typeCheck2 == 255)
                                        bf = true;
                                    AnimationData = new RSDK2.Animation(reader, bf);
                                    break;
                                }
                            case 3:
                                PathMod = "";
                                LoadedAnimVer = 1;
                                AnimationData = new RSDK1.Animation(reader, false);
                                break;
                            case 4:
                                PathMod = "";
                                LoadedAnimVer = 1;
                                AnimationData = new RSDK1.Animation(reader, true);
                                break;
                            default:
                                return false;
                        }
                    }
                }
                return true;
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

        public void AnimationAdd()
        {
            _animationData.Factory(out IAnimationEntry o);
            Animations.Add(o);
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
                SelectedAnimation = anim;
            }
        }

        public void AnimationDuplicate()
        {
            var selectedAnimation = SelectedAnimation;
            if (selectedAnimation != null)
                Animations.Add(selectedAnimation.Clone() as IAnimationEntry);
        }

        public void AnimationRemove()
        {
            Animations.Remove(SelectedAnimation);
        }

        public void AnimationImport(string fileName)
        {
            if (!File.Exists(fileName)) return;

            using (var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using (var reader = new BinaryReader(fStream))
                {
                    _animationData.Factory(out IAnimationEntry o);
                    o.Read(reader);
                    Animations.Add(o);
                }
            }
        }

        public void AnimationExport(string fileName)
        {
            var selectedAnimation = SelectedAnimation;
            if (selectedAnimation == null) return;

            using (var fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new BinaryWriter(fStream))
                {
                    selectedAnimation.Write(writer);
                }
            }
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

            using (var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                using (var reader = new BinaryReader(fStream))
                {
                    _animationData.Factory(out IFrame o);
                    o.Read(reader);
                    FrameAdd(o, SelectedFrameIndex);
                }
            }
        }

        public void FrameExport(string fileName)
        {
            var selectedFrame = SelectedFrame;
            if (selectedFrame == null) return;

            using (var fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new BinaryWriter(fStream))
                {
                    selectedFrame.Write(writer);
                }
            }
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
