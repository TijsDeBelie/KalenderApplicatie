using System.Collections.Generic;

namespace Calender
{
    public interface IKalender
    {
        IList<Afspraak> AfsprakenLijst { get; set; }
        string Beschrijving { get; set; }
        string Naam { get; set; }
        void Delete();
        string ToString();
        void VoegAfspraakToe(Afspraak afspraak);
    }
}