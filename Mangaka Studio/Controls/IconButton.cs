using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mangaka_Studio.Controls
{
    public class IconButton : Button
    {
        public static readonly DependencyProperty dependencyProperty =
            DependencyProperty.Register(
                "IconSource",
                typeof(string),
                typeof(IconButton),
                new PropertyMetadata(null)
            );

        public string IconSource
        {
            get { return (string)GetValue(dependencyProperty); }
            set { SetValue(dependencyProperty, value); }
        }
    }
}
