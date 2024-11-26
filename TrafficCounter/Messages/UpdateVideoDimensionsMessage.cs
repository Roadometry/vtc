using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTC.Common.RegionConfig;

namespace VTC.Messages
{
    class UpdateVideoDimensionsMessage
    {
        public UpdateVideoDimensionsMessage(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
    }
}
