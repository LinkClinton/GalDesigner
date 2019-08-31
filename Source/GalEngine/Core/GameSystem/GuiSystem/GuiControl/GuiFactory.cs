using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static class GuiFactory
    {
        public static FontClass FontClass => FontClass.UbuntuMono;

        static GuiFactory()
        {
        }
    }
}
