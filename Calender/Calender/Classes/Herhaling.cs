using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calender
{
    public class Herhaling
    {
        public Herhaling()
        {
            Herhalingen = new List<string>();
            Herhalingen.Add("Geen");
            Herhalingen.Add("Dagelijks");
            Herhalingen.Add("Wekelijks");
        }

        public IList<string> Herhalingen { get; set; }
    }
}