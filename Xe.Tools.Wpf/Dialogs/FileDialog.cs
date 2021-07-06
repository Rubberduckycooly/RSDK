﻿// MIT License
// 
// Copyright(c) 2017 Luciano (Xeeynamo) Ciccariello
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Part of this software belongs to XeEngine toolset and United Lines Studio
// and it is currently used to create commercial games by Luciano Ciccariello.
// Please do not redistribuite this code under your own name, stole it or use
// it artfully, but instead support it and its author. Thank you.

using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Xe.Tools.Wpf.Dialogs
{
    public class FileDialog
    {
        public enum Behavior
        {
            Open, Save, Folder
        }
        public enum Type
        {
            Any,
            Binary,
            Executable,
            XeGameProject,
            XeAnimation,
            ImagePng,
            ImageGif,
            ImageBmp,

            Rsdk1Animation,
            Rsdk1Frame,
            Rsdk2Animation,
            Rsdk2Frame,
            Rsdk3Animation,
            Rsdk3Frame,
            Rsdk5Animation,
            Rsdk5Frame,
            jsonFile
        }

        private CommonFileDialog _fd;

        private Window WindowParent { get; }

        public Behavior CurrentBehavior { get; }

        public Type CurrentType { get; }

        public string FileName => _fd.FileName;

        public IEnumerable<string> FileNames => (_fd as CommonOpenFileDialog)?.FileNames ?? new string[] { FileName };

        private FileDialog(CommonFileDialog commonFileDialog, Window wndParent, Behavior behavior, Type type)
        {
            _fd = commonFileDialog;
            WindowParent = wndParent;
            CurrentBehavior = behavior;
            CurrentType = type;
        }

        public bool? ShowDialog()
        {
            switch (_fd.ShowDialog(WindowParent))
            {
                case CommonFileDialogResult.Ok: return true;
                case CommonFileDialogResult.None: return false;
                case CommonFileDialogResult.Cancel: return null;
                default: return null;
            }
        }

        public static FileDialog Factory(Window wndParent, Behavior behavior, Type type = Type.Any, bool multipleSelection = false)
        {
            CommonFileDialog fd;
            switch (behavior)
            {
                case Behavior.Open:
                    fd = new CommonOpenFileDialog()
                    {
                        EnsureFileExists = true,
                        Multiselect = multipleSelection
                    };
                    break;
                case Behavior.Save:
                    fd = new CommonSaveFileDialog()
                    {
                        AlwaysAppendDefaultExtension = true,
                        
                    };
                    break;
                case Behavior.Folder:
                    fd = new CommonOpenFileDialog()
                    {
                        IsFolderPicker = true,
                        Multiselect = multipleSelection
                    };
                    break;
                default:
                    throw new ArgumentException("Invalid parameter", nameof(behavior));
            }
            fd.AddToMostRecentlyUsedList = true;
            fd.EnsurePathExists = true;

            if (behavior != Behavior.Folder)
            {
                switch (type)
                {
                    case Type.Any:
                        fd.Filters.Add(CreateFilter("All files",
                            new string[] { "*" }));
                        break;
                    case Type.Binary:
                        fd.Filters.Add(CreateFilter("Binary files",
                            new string[] { "bin" }));
                        break;
                    case Type.Executable:
                        fd.Filters.Add(CreateFilter("Applications",
                            new string[] { "exe" }));
                        break;
                    case Type.XeGameProject:
                        fd.Filters.Add(CreateFilter("XeEngine projects",
                            new string[] { "proj.json" }));
                        break;
                    case Type.XeAnimation:
                        fd.Filters.Add(CreateFilter("XeEngine animation files",
                            new string[] { "anim.json" }));
                        break;
                    case Type.ImagePng:
                        fd.Filters.Add(CreateFilter("PNG image files",
                            new string[] { "png" }));
                        break;
                    case Type.ImageGif:
                        fd.Filters.Add(CreateFilter("GIF image files",
                            new string[] { "gif" }));
                        break;
                    case Type.ImageBmp:
                        fd.Filters.Add(CreateFilter("Bitmap image files",
                            new string[] { "bmp" }));
                        break;
                    case Type.Rsdk1Animation:
                        fd.Filters.Add(CreateFilter("RSDK1 animation files",
                            new string[] { "anim.rsdk1" }));
                        break;
                    case Type.Rsdk1Frame:
                        fd.Filters.Add(CreateFilter("RSDK1 frame files",
                            new string[] { "frame.rsdk1" }));
                        break;
                    case Type.Rsdk2Animation:
                        fd.Filters.Add(CreateFilter("RSDK2 animation files",
                            new string[] { "anim.rsdk2" }));
                        break;
                    case Type.Rsdk2Frame:
                        fd.Filters.Add(CreateFilter("RSDK2 frame files",
                            new string[] { "frame.rsdk2" }));
                        break;
                    case Type.Rsdk3Animation:
                        fd.Filters.Add(CreateFilter("RSDK3 animation files",
                            new string[] { "anim.rsdk3" }));
                        break;
                    case Type.Rsdk3Frame:
                        fd.Filters.Add(CreateFilter("RSDK3 frame files",
                            new string[] { "frame.rsdk3" }));
                        break;
                    case Type.Rsdk5Animation:
                        fd.Filters.Add(CreateFilter("RSDK5 animation files",
                            new string[] { "anim.rsdk5" }));
                        break;
                    case Type.Rsdk5Frame:
                        fd.Filters.Add(CreateFilter("RSDK5 frame files",
                            new string[] { "frame.rsdk5" }));
                        break;
                    case Type.jsonFile:
                        fd.Filters.Add(CreateFilter("JSON files",
                            new string[] { "json" }));
                        break;
                    default:
                        break;
                }
            }

            return new FileDialog(fd, wndParent, behavior, type);
        }

        private static CommonFileDialogFilter CreateFilter(string name, string[] filters)
        {
            var filter = new CommonFileDialogFilter()
            {
                DisplayName = name
            };
            foreach (var item in filters)
                filter.Extensions.Add(item);
            return filter;
        }
    }
}
