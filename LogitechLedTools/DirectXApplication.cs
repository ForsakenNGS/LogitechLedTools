using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogitechLedTools
{
    class DirectXApplication : MarshalByRefObject
    {
        public void IsInstalled(Int32 clientPid)
        {
            return;
        }

        public void WriteConsole(String text)
        {
            Console.WriteLine(text);
        }
    }
}
