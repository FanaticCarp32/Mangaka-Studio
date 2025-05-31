using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class FrameConfig
    {
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int Spacing { get; set; } = 0;
        public int BorderWidth { get; set; } = 2;
        public List<FrameConfig> Children { get; set; } = null;
    }

}
