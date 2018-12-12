using System.Collections.Generic;
using System.Data;

namespace Calender
{
    public interface IKalender
    {
        IList<IAfspraak> AfsprakenLijst { get; set; }
        string Beschrijving { get; set; }
        int Id { get; set; }
        string Naam { get; set; }
        DataRow row { get; }

        void Delete();
        string ToString();
        void VoegAfspraakToe(IAfspraak afspraak);
    }
}