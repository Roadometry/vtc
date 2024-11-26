using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTC.Messages
{
    class StringMessage
    {
        public StringMessage(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }
    }
}
