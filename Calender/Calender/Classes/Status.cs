using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calender
{
    public class Status
    {
        public Status()
        {
            ListOfItems = new List<string>();
            ListOfItems.Add("Vrij");
            ListOfItems.Add("Bezet");
            ListOfItems.Add("Werk");
        }

        public IList<string> ListOfItems { get; set; }
    }
}
