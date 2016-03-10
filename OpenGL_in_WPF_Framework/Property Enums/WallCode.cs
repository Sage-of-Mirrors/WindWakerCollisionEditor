using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionEditor
{
    public enum WallCode
    {
        Normal = 0x0,
	    Climbable_Generic = 0x1,
	    Wall = 0x2,
	    Grabable = 0x3,
	    Climbable_Ladder = 0x4,
	    Code6 = 0x5,
	    Code7 = 0x6,
	    Code8 = 0x7,
	    Code9 = 0x8,
	    Code10 = 0x9,
	    Code11 = 0xA,
	    Code12 = 0xB,
	    Code13 = 0xC,
	    Code14 = 0xD,
	    Code15 = 0xE,
	    Code16 = 0xF,
        Multi,
        None
    }
}
