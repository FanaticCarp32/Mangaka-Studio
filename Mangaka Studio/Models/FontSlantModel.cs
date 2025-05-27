using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class FontSlantModel
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }
}
