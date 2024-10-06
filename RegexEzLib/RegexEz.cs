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
    private const string TestFieldPrefix = "$field.";
    private const string TagPrefix = "__tag_";

    private readonly Dictionary<string, List<ListNode>> _macros = new();
    private readonly List<string> _passTests = new();
    private readonly List<string> _failTests = new();
    private readonly Dictionary<string, List<string>> _multiMatches = new();
    private readonly Dictionary<string, List<FieldTest>> _fieldTests = new();
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

    public Match Match(string test)
    {
        return Regex().Match(test);
    }

    public MatchCollection Matches(string test)
    {
        return Regex().Matches(test);
    }

    public string GetField(string test, string fieldName)
    {
        var groups = Regex().Match(test).Groups;
        var groupName = $"{TagPrefix}{fieldName}";
        return groups.ContainsKey(groupName) ? groups[groupName].Value : string.Empty;
    }

    public string? GetFieldMultiMatch(string test, string fieldName, int matchNum)
    {
        var matches = Regex().Matches(test);
        if (matches.Count <= matchNum)
        {
            return null;
        }
        var groups = matches[matchNum].Groups;
        var groupName = $"{TagPrefix}{fieldName}";
        return groups.ContainsKey(groupName) ? groups[groupName].Value : string.Empty;
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
            optionalFailures?.Add("Field tests require groups to be enabled. This is a RegexExGui constructor parameter");
        }
        else
        {
            foreach (var (testStr, list) in _fieldTests)
            {
                var groups = regex.Match(testStr).Groups;
                foreach (var (fieldName, expected) in list)
                {
                    var groupName = $"{TagPrefix}{fieldName}";
                    if (groups.ContainsKey(groupName))
                    {
                        if (groups[groupName].Value != expected)
                        {
                            allPass = false;
                            optionalFailures?.Add(
                                $"Expected {expected} for field {fieldName} but got {groups[groupName].Value}");
                        }
                    }
                    else
                    {
                        allPass = false;
                        optionalFailures?.Add($"Unknown field {fieldName}");
                    }
                }
            }
        }

        foreach (var (testStr, matches) in _multiMatches)
        {
            var match = regex.Matches(testStr);
            if (match.Count != matches.Count)
            {
                allPass = false;
                optionalFailures?.Add($"Expected {matches.Count} matches but got {match.Count}");
            }
            else
            {
                for (var i = 0; i < match.Count; i++)
                {
                    if (match[i].Value != matches[i])
                    {
                        allPass = false;
                        optionalFailures?.Add($"Expected {matches[i]} but got {match[i].Value}");
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

    private void AddFieldTest(string testString, string fieldName, string expectedValue)
    {
        if (!_fieldTests.ContainsKey(testString))
            _fieldTests.Add(testString, new List<FieldTest> { new(fieldName, expectedValue) });
        else
            _fieldTests[testString].Add(new FieldTest(fieldName, expectedValue));
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
            else if (name.StartsWith(TestFieldPrefix))
            {
                var fieldName = name.Substring(TestFieldPrefix.Length);
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected field name after .");
                }
                var split = regex.Split("$=");

                if (split.Length != 2 || string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new ArgumentException($"Invalid field test {line}, expected $=");
                }

                AddFieldTest(split[0].Trim(), fieldName, split[1].Trim());
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
                    throw new ArgumentException($"Circular reference detected for {node.Macro}");
                }

                var str = Build(node.Macro, alreadyUsed);
                if (_withGroups)
                {
                    str = $"(?<{TagPrefix}{node.Macro}>{str})";
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
        MacroWalk,
        MacroBracketWalk
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
                        state = Mode.MacroBracketWalk;
                    }
                    else if (char.IsLetter(c))
                    {
                        builder.Append(c);
                        state = Mode.MacroWalk;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid pattern after {soFar}");
                    }

                    break;
                case Mode.MacroWalk:
                    if (char.IsLetter(c))
                    {
                        builder.Append(c);
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
                case Mode.MacroBracketWalk:
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

        if (state is Mode.MacroWalk or Mode.MacroBracketWalk)
        {
            if (builder.Length == 0)
            {
                throw new ArgumentException($"Empty macro name after {soFar}");
            }
            nodes.Add(new ListNode { Macro = builder.ToString() });
        }
        else
        {
            nodes.Add(new ListNode { Text = builder.ToString() });
        }

        return nodes;
    }
}