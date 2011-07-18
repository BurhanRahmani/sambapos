﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class CommandButtonViewModel
    {
        public ICaptionCommand Command { get; set; }
        public string Caption { get; set; }
        public PrintJob Parameter { get; set; }
    }
}
