using System.Text.RegularExpressions;

namespace RegexEzLib
{
    public class MatchEz
    {
        public MatchEz(Match match)
        {
            _match = match;
        }
        public Match Match => _match;
        public string this[string fieldName]
        {
            get
            {
                var tag = RegexEz.TagName(fieldName);
                if (!_match.Groups.ContainsKey(tag))
                    throw new KeyNotFoundException($"Field {fieldName} does not exist");
                return _match.Groups[tag].Value;
            }
        }

        public string Value => _match.Value;
        public bool Success => _match.Success;
        private readonly Match _match;
    }
}
