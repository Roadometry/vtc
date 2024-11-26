using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Messages
{
    class FileCreationTimeMessage
    {

        public FileCreationTimeMessage(DateTime dt)
        {
            FileCreationTime = dt;
        }

        public DateTime FileCreationTime { get; private set; }
    
    }
}
