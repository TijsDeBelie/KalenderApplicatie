﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Calender.Exceptions
{
    class NameIsEmpty : Exception
    {

        public NameIsEmpty()
        {

        }
        public NameIsEmpty(string name)
            : base(String.Format("Naam was leeg voor: {0}", name))
        {
           
        }

    }
}
