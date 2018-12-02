using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calender.Exceptions
{
    class NameIsEmpty : Exception
    {

        public NameIsEmpty()
        {

        }
        public NameIsEmpty(string name)
            : base(String.Format("name was empty for: {0}", name))
        {

        }

    }
}
