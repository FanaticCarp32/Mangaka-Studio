﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mangaka_Studio.Interfaces
{
    public interface IColorPickerContext
    {
        bool SwitchColor { get; set; }
        ICommand SwitchColorPipette { get; }
    }
}
