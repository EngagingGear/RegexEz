using System.Text.RegularExpressions;

namespace RegexEzLib
{
    /// <summary>
    /// A class to represent the results of a RegexEz mathc
    /// </summary>
    public class MatchEz
    {
        /// <summary>
        /// Constructor taking a standard .NET Match object
        /// </summary>
        /// <param name="match"></param>
        public MatchEz(Match match)
        {
            _match = match;
        }

        /// <summary>
        /// Return the underlying Match object
        /// </summary>
        public Match Match => _match;

        /// <summary>
        /// Gets a field from the match by macro name
        /// </summary>
        /// <param name="macroName">The macro name</param>
        /// <returns>The value from the match</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no such match</exception>
        public string this[string macroName]
        {
            get
            {
                var tag = RegexEz.TagName(macroName);
                if (!_match.Groups.ContainsKey(tag))
                    throw new KeyNotFoundException($"Field {macroName} does not exist");
                return _match.Groups[tag].Value;
            }
        }

        public string Value => _match.Value;
        public bool Success => _match.Success;
        private readonly Match _match;
    }
}
