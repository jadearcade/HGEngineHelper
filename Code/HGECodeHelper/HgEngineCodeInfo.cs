using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGEngineHelper.Code.HGECodeHelper
{
    public class HgEngineCodeInfo
    {
        public Dictionary<CodeSectionType, List<CodeSection>> codeSections = new Dictionary<CodeSectionType, List<CodeSection>>();
    }

    public enum CodeSectionType
    {
        BEGINNING = 1,
        END = 2,
        MIDDLE = 3,
    }

    public class CodeSection
    {
        public List<string> lines = new List<string>();
    }

    public static class HgEngineCodeHelperFunctions
    {
        //public static HgEngineCodeIndex ConvertToIndexDescription
        //{

        //}
    }

    public class HgEngineConstantHeaderIndex
    {
        public HgEngineConstantHeaderIndex()
        {

        }
        public AddSubtractPair key;
        public AddSubtractPair value;
    }

    public class AddSubtractPair
    {
        public string part1;
        public string part2;
        public string pairOperator;
        public string GetString()
        {
            if (part2 != "")
            {
                return "(" + (part1.Replace("(", "").Replace(")", "").Trim() + " " 
                    + pairOperator.Trim() + " " + part2.Replace("(", "").Replace(")", "").Trim()) + ")";
            }
            if (part1.Contains("_"))
            {
                return "(" + part1.Replace("(", "").Replace(")", "").Trim() + ")";
            }
            return part1;
        }
    }
}
