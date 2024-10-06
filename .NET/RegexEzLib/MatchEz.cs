using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegexEzLib
{
    public class MatchEz
    {
        public MatchEz(Match match)
        {
            _match = match;
        }
        public Match Match => _match;
        public string this[string fieldName] => _match.Groups[RegexEz.TagName(fieldName)].Value;
        public string Value => _match.Value;
        private readonly Match _match;
    }
}
