﻿// Copyright (c) Jan-Niklas Schäfer. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace ThinkSharp.FeatureTouring.Touring
{
    public class CustomizeHeaderViewModel : ViewModelBase
    {
        private string _header = "My Header";

        public string Header
        {
            get { return _header; }
            set { Set("Header", ref _header, value); }
        }
    }
}
