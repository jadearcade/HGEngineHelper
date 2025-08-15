using CsvConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Code.CsvProcessing.Models
{
    public class FormesForSpeciesInfo
    {
        public string type { get; set; }
        public string speciesKey { get; set; }
        public List<SpeciesForm> forms { get; set; }
    }

    public class FormesForSpeciesInfoRow
    {
        public FormesForSpeciesInfoRow()
        {

        }
        [CsvConverter(ColumnIndex = 3, ColumnName = "Type")]
        public string Type { get; set; }
        [CsvConverter(ColumnIndex = 1, ColumnName = "Species")]
        public string SpeciesKey { get; set; }
        [CsvConverter(ColumnIndex = 4, ColumnName = "Needs Reversion")]
        public bool NeedsReversion { get; set; }
        [CsvConverter(ColumnIndex = 2, ColumnName = "Form Species")]
        public string FormSpeciesKey { get; set; }

        public int FormIndex { get; set; }
        public int FormToRevertToIndex { get; set; } = 0;
        public string MegaItem { get; set; } = "";
        public string MegaMove { get; set; } = "";
    }

    public class SpeciesForm
    {
        public bool needsReversion { get; set; }
        public string formSpeciesKey { get; set; }
    }
}
