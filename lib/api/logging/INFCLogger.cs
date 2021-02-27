using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.NFC
{
    public interface INFCLogger
    {
        void Log(string message);
        void ManageException(Exception ex);
    }
}
