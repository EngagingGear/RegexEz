using System.Text;
using System.Text.RegularExpressions;

namespace RegexEzLib;

public class RegexEz
{
    private record FieldTest(string FieldName, string ExpectedValue);
    private record ListNode(string? Macro = null, string? Text = null);

    private const string TestMatch = "$match";
    private const string TestMatchMulti = "$multimatch";
    private const string TestNoMatch = "$nomatch";
    private const string TestEnd = "$end";
    private const string SimpleFieldTestPrefix = "$field.";
    private const string MultiFieldTestPrefix = "$field[";
    private const string TagPrefix = "__tag_";

    private readonly Dictionary<string, List<ListNode>> _macros = new();
    private readonly List<string> _passTests = new();
    private readonly List<string> _failTests = new();
    private readonly Dictionary<string, List<string>> _multiMatches = new();
    private readonly Dictionary<string, Dictionary<int, List<FieldTest>>> _fieldTests = new();
    private readonly bool _withGroups;
    private Regex? _regex = null;

    public RegexEz(string pattern, bool withGroups = false, string breakString = "\r\n")
    {
        _withGroups = withGroups;
        Initialize(pattern.Split(breakString));
    }

    public RegexEz(string[] pattern, bool withGroups = false)
    {
        _withGroups = withGroups;
        Initialize(pattern);
    }

    public Regex Regex()
    {
        _regex ??= new Regex(RegexStr());
        return _regex;
    }

    public Regex Regex(RegexOptions options)
    {
        return new Regex(RegexStr(), options);
    }

    public Regex Regex(RegexOptions options, TimeSpan timeout)
    {
        return new Regex(RegexStr(), options, timeout);
    }

    public string RegexStr()
    {
        return Build(_macros.First().Key);
    }

    public bool IsMatch(string test)
    {
        return Regex().IsMatch(test);
    }

    public MatchEz Match(string test)
    {
        return new MatchEz(Regex().Match(test));
    }

    public IList<MatchEz> Matches(string test)
    {
        return Regex().Matches(test).Select(m => new MatchEz(m)).ToList();
    }

    public bool Test(List<string>? optionalFailures = null)
    {
        return PerformTest(Regex(), optionalFailures);
    }

    public bool Test(RegexOptions options, List<string>? optionalFailures = null)
    {
        return PerformTest(Regex(options), optionalFailures);
    }

    public bool Test(RegexOptions options, TimeSpan timeout, List<string>? optionalFailures = null)
    {
        return PerformTest(Regex(options, timeout), optionalFailures);
    }

    public static string TagName(string fieldName)
    {
        return $"{TagPrefix}{fieldName}";
    }

    private bool PerformTest(Regex regex, List<string>? optionalFailures = null)
    {
        var allPass = true;
        foreach (var test in _passTests)
        {
            if (!regex.IsMatch(test))
            {
                allPass = false;
                optionalFailures?.Add($"Expected pass: {test}");
            }
        }

        foreach (var test in _failTests)
        {
            if (regex.IsMatch(test))
            {
                allPass = false;
                optionalFailures?.Add($"Expected fail: {test}");
            }
        }

        if (_fieldTests.Any() && _withGroups == false)
        {
            allPass = false;
            optionalFailures?.Add("Field tests require groups to be enabled. This is a RegexEz constructor parameter");
        }
        else
        {
            foreach (var (testStr, list) in _fieldTests)
            {
                var allMatches = regex.Matches(testStr);
                foreach (var (matchNum, matchList) in list)
                {
                    if (allMatches.Count <= matchNum)
                    {
                        allPass = false;
                        optionalFailures?.Add($"For {testStr} expected {matchNum} matches but only got {allMatches.Count}");
                        continue;
                    }
                    var groups = allMatches[matchNum].Groups;
                    foreach (var expectedMatch in matchList)
                    {
                        var fieldName = expectedMatch.FieldName;
                        var expected = expectedMatch.ExpectedValue;
                        var groupName = TagName(fieldName);
                        if (groups.ContainsKey(groupName))
                        {
                            if (groups[groupName].Value != expected)
                            {
                                allPass = false;
                                optionalFailures?.Add($"For {testStr} expected {expected} for field {fieldName} in match num {matchNum} but got {groups[groupName].Value}");
                            }
                        }
                        else
                        {
                            allPass = false;
                            optionalFailures?.Add($"For {testStr} unknown macro {fieldName}");
                        }
                    }
                }
            }
        }

        foreach (var (testStr, expectedMatches) in _multiMatches)
        {
            var match = regex.Matches(testStr);
            if (match.Count != expectedMatches.Count)
            {
                allPass = false;
                optionalFailures?.Add($"For {testStr} expected {expectedMatches.Count} matches but got {match.Count}. " +
                                      $"Actual matches {string.Join(", ", match.Select(m => m.Value))}");
            }
            else
            {
                for (var i = 0; i < match.Count; i++)
                {
                    if (match[i].Value != expectedMatches[i])
                    {
                        allPass = false;
                        optionalFailures?.Add($"For {testStr} expected {expectedMatches[i]} but got {match[i].Value}");
                    }
                }
            }
        }

        return allPass;
    }

    private void AddTest(string testString, bool expectedPass)
    {
        if (expectedPass)
            _passTests.Add(testString);
        else
            _failTests.Add(testString);
    }

    private void AddMultiMatch(string testString, List<string> matches)
    {
        if (!_multiMatches.ContainsKey(testString))
            _multiMatches.Add(testString, matches);
        else
            _multiMatches[testString].AddRange(matches);
    }

    private void AddFieldTest(string testString, string fieldName, string expectedValue, int matchNum)
    {
        if (!_fieldTests.ContainsKey(testString))
        {
            _fieldTests.Add(testString, new Dictionary<int, List<FieldTest>>());
            _fieldTests[testString].Add(0, new List<FieldTest> { new(fieldName, expectedValue) });
        }
        else if (!_fieldTests[testString].ContainsKey(matchNum))
        {
             _fieldTests[testString].Add(matchNum, new List<FieldTest> { new(fieldName, expectedValue) });
        }
        else
        {
            _fieldTests[testString][matchNum].Add(new FieldTest(fieldName, expectedValue));
        }
    }

    private void Initialize(string[] pattern)
    {
        _macros.Clear();
        for (var lineNum = 0; lineNum < pattern.Length; lineNum++)
        {
            var line = pattern[lineNum];
            if (line.Trim().StartsWith("//") || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var colonIdx = line.IndexOf(':');

            if (colonIdx == -1)
            {
                throw new ArgumentException($"Invalid pattern line {lineNum}, expected :");
            }

            var name = line.Substring(0, colonIdx);
            if (_macros.ContainsKey(name))
            {
                throw new ArgumentException($"Invalid pattern line {lineNum}, macro {name} defined twice");
            }

            while (char.IsWhiteSpace(line[colonIdx + 1]))
            {
                colonIdx++;
            }
            var regex = line.Substring(colonIdx + 1);
            if (name.ToLower() == TestMatch)
                AddTest(regex, true);
            else if (name.ToLower() == TestNoMatch)
                AddTest(regex, false);
            else if (name.ToLower() == TestMatchMulti)
            {
                var list = new List<string>();
                var endFound = false;
                for (lineNum++; lineNum < pattern.Length; lineNum++)
                {
                    var multiMatchLine = pattern[lineNum];
                    if (multiMatchLine.ToLower() == TestEnd)
                    {
                        endFound = true;
                        break;
                    }

                    list.Add(multiMatchLine);
                }
                if (!endFound)
                    throw new ArgumentException($"Invalid test line {lineNum}, missing {TestEnd}");
                AddMultiMatch(regex, list);
            }
            else if (name.StartsWith(SimpleFieldTestPrefix))
            {
                var fieldName = name.Substring(SimpleFieldTestPrefix.Length);
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected field name after .");
                }
                var split = regex.Split("$=");

                if (split.Length != 2 || string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected $=");
                }

                AddFieldTest(split[0].Trim(), fieldName, split[1].Trim(), 0);
            }
            else if (name.StartsWith(MultiFieldTestPrefix))
            {
                int closeBracketIdx = name.IndexOf(']');
                if (closeBracketIdx+1 >= name.Length - 1 || name[closeBracketIdx+1] != '.')
                {
                    closeBracketIdx = -1;
                }
                if (closeBracketIdx < 0 || !int.TryParse(name.Substring(MultiFieldTestPrefix.Length), out var matchNumber))
                {
                    throw new ArgumentException($"Invalid field test {line}, invalid index or field name");
                }

                var fieldName = name.Substring(closeBracketIdx+2);
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected field name after .");
                }
                var split = regex.Split("$=");

                if (split.Length != 2 || string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected $=");
                }

                AddFieldTest(split[0].Trim(), fieldName, split[1].Trim(), matchNumber);
            }
            else if (name.StartsWith("$"))
            {
                throw new ArgumentException($"Invalid definition line {lineNum}");
            }
            else
            {
                var nodes = Parse(regex);
                _macros.Add(name, nodes);
            }
        }

        Build(_macros.First().Key);
    }

    private string Build(string name, HashSet<string>? alreadyUsed = null)
    {
        if (! _macros.ContainsKey(name))
        {
            throw new ArgumentException($"Macro {name} not defined");
        }

        if (alreadyUsed == null)
        {
            alreadyUsed = new HashSet<string> { name };
        }
        else
        {
            alreadyUsed.Add(name);
        }

        var sb = new StringBuilder();
        foreach (var node in _macros[name])
        {
            if (node.Macro != null)
            {
                if (alreadyUsed.Contains(node.Macro))
                {
                    throw new ArgumentException($"Circular reference detected for \"{node.Macro}\"");
                }

                var str = Build(node.Macro, alreadyUsed);
                if (_withGroups)
                {
                    str = $"(?<{TagName(node.Macro)}>{str})";
                }
                sb.Append(str);
            }
            else
            {
                sb.Append(node.Text);
            }
        }
        alreadyUsed.Remove(name);
        return sb.ToString();
    }

    private enum Mode
    {
        Text,
        MacroStart,
        ScanningMacroName,
        ScanningMacroNameExpectingBracketAtEnd
    };
    private List<ListNode> Parse(string regex)
    {
        var state = Mode.Text;
        var nodes = new List<ListNode>();
        var builder = new StringBuilder();
        var soFar = new StringBuilder();
        foreach (var c in regex)
        {
            soFar.Append(c);
            switch (state)
            {
                case Mode.Text:
                    if (c == '$')
                    {
                        state = Mode.MacroStart;
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
                case Mode.MacroStart:
                    if (builder.Length > 0)
                    {
                        nodes.Add(new ListNode { Text = builder.ToString() });
                    }

                    builder.Clear();
                    if (c == '(')
                    {
                        state = Mode.ScanningMacroNameExpectingBracketAtEnd;
                    }
                    else if (char.IsLetter(c))
                    {
                        builder.Append(c);
                        state = Mode.ScanningMacroName;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid pattern after {soFar}");
                    }

                    break;
                case Mode.ScanningMacroName:
                    if (char.IsLetter(c))
                    {
                        builder.Append(c);
                    }
                    else if (c == '$')
                    {
                        nodes.Add(new ListNode { Macro = builder.ToString() });
                        builder.Clear();
                        state = Mode.MacroStart;
                    }
                    else
                    {
                        if (builder.Length == 0)
                        {
                            throw new ArgumentException($"Empty macro name after {soFar}");
                        }
                        nodes.Add(new ListNode { Macro = builder.ToString() });
                        builder.Clear();
                        builder.Append(c);
                        state = Mode.Text;
                    }

                    break;
                case Mode.ScanningMacroNameExpectingBracketAtEnd:
                    if (c == ')')
                    {
                        if (builder.Length == 0)
                        {
                            throw new ArgumentException($"Empty macro name after {soFar}");
                        }
                        nodes.Add(new ListNode { Macro = builder.ToString() });
                        builder.Clear();
                        state = Mode.Text;
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
            }
        }

        if (state is Mode.ScanningMacroName or Mode.ScanningMacroNameExpectingBracketAtEnd)
        {
            if (builder.Length == 0)
            {
                throw new ArgumentException($"Empty macro name after {soFar}");
            }
            nodes.Add(new ListNode { Macro = builder.ToString() });
        }
        else if (state == Mode.MacroStart)
        {
            // Here we saw a $ thinking it is a macro start but really it is a $ at the end of the string
            builder.Append('$');
            nodes.Add(new ListNode { Text = builder.ToString() });
        }
        else
        {
            nodes.Add(new ListNode { Text = builder.ToString() });
        }

        return nodes;
    }
}