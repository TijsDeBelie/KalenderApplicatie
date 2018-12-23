using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calender.Exceptions
{
    class DBConnectFailed : Exception
    {

        public DBConnectFailed()
        {

        }

        public DBConnectFailed(string name)
            : base(String.Format("Connection failed at: {0}", name))
        {

        }
    }
}
