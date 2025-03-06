using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mangaka_Studio.Models
{
    public abstract class DrawingTools
    {
        public abstract void OnMouseDown(Canvas canvas, Point pos);
        public abstract void OnMouseUp();
        public abstract void OnMouseMove(Canvas canvas, Point pos);
        public abstract void OnMouseLeave(Canvas canvas, Point pos);
        public abstract void OnMouseEnter(Canvas canvas, Point pos);
    }
}
