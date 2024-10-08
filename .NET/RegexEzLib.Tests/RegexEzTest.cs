using System.Reflection;
using System.Text.RegularExpressions;
using static RegexEzLib.RegexEz;
using static System.Net.Mime.MediaTypeNames;

namespace RegexEzLib.Tests
{
    [TestClass]
    public class RegexEzTest
    {
        [TestMethod]
        public void RegexShouldPassThrough()
        {
            var template = new[]
            {
                "test: ^[A-Za-z]*$",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[A-Za-z]*$", regexStr);
        }

        [TestMethod]
        public void ThrowExceptionIfNoMacro()
        {
            var template = new[]
            {
                "^[A-Za-z]*$",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfDuplicateMacro()
        {
            var template = new[]
            {
                "test: ^[A-Za-z]*$",
                "test: ^[A-Za-z]*$",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void RegexShouldPassThroughOptions()
        {
            var template = new[]
            {
                "test: ^[A-Za-z]*$",
            };
            var regexEz = new RegexEz(template);
            var regex = regexEz.Regex(RegexOptions.Compiled);
            Assert.AreEqual(RegexOptions.Compiled, regex.Options);
        }

        [TestMethod]
        public void CommentsShouldBeIgnored()
        {
            var template = new[]
            {
                "// This is a comment",
                "test: ^[A-Za-z]*$",
                "// This is a comment",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[A-Za-z]*$", regexStr);
        }

        [TestMethod]
        public void BlankLinesShouldBeIgnored()
        {
            var template = new[]
            {
                "// This is a comment",
                "test: ^[A-Za-z]*$",
                "",
                "// This is a comment",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[A-Za-z]*$", regexStr);
        }

        [TestMethod]
        public void MacrosCanComeFromAFullString()
        {
            var template = "test: ^$(username)@$(domain)\\.$(tld)$\r\nusername: $name\r\n\r\ndomain: $name\r\ntld: $name\r\nname: [a-zA-Z0-9_]+";
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[a-zA-Z0-9_]+@[a-zA-Z0-9_]+\\.[a-zA-Z0-9_]+$", regexStr);
        }

        [TestMethod]
        public void MacrosShouldBeSubstitutedWithBrackets()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[a-zA-Z0-9_]+@[a-zA-Z0-9_]+\\.[a-zA-Z0-9_]+$", regexStr);
        }

        [TestMethod]
        public void MacrosShouldBeSubstitutedWithoutBrackets()
        {
            var template = new[]
            {
                "test: ^$username@$domain\\.$tld$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("^[a-zA-Z0-9_]+@[a-zA-Z0-9_]+\\.[a-zA-Z0-9_]+$", regexStr);
        }

        [TestMethod]
        public void MacroShouldBeDefined()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
            };
           Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void MacroShouldNotBeRecursive()
        {
            var template = new[]
            {
                "test: $domain",
                "domain: $name",
                "name: $domain"
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void MacroShouldHaveAValidName()
        {
            var template = new[]
            {
                "test: $domain$###",
                "domain: [A-Z]+",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void MacroCanImmediatelyFollowAnotherMacro()
        {
            var template = new[]
            {
                "test: $first$second",
                "first: first",
                "second: second",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("firstsecond", regexStr);
        }

        [TestMethod]
        public void MacroCanImmediatelyFollowAnotherMacroWtihInterveningText()
        {
            var template = new[]
            {
                "test: $first###$second",
                "first: first",
                "second: second",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("first###second", regexStr);
        }

        [TestMethod]
        public void MacroShouldNonEmptyName()
        {
            var template = new[]
            {
                "test: $()",
                "domain: [A-Z]+",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void MacroShouldShouldDistinguishBetweenDollarAtEndAndMacroStartAtEnd()
        {
            var template = new[]
            {
                "test: Match$",
            };
            var regexEz = new RegexEz(template);
            var regexStr = regexEz.RegexStr();
            Assert.AreEqual("Match$", regexStr);
        }

        [TestMethod]
        public void MacrosHaveCaptureGroupsInserted()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var regexStr = regexEz.RegexStr();
            var expected =
                "^(?<__tag_username>(?<__tag_name>[a-zA-Z0-9_]+))@(?<__tag_domain>(?<__tag_name>[a-zA-Z0-9_]+))\\.(?<__tag_tld>(?<__tag_name>[a-zA-Z0-9_]+))$";
            Assert.AreEqual(expected, regexStr);
        }

        [TestMethod]
        public void ThrowsExceptionIfUnknownCommand()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$invalid: sample@test.com test@example.com",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void IsMatchReturnsTrueOnMatch()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsTrue(regexEz.IsMatch("test@sample.com"));
        }

        [TestMethod]
        public void IsMatchReturnsFalseOnNonMatch()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsFalse(regexEz.IsMatch("test@sample"));
        }

        [TestMethod]
        public void MatchReturnsMatchObjectWithSuccessTrueOnMatch()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var match = regexEz.Match("test@sample.com");
            Assert.IsTrue(match.Success);
        }

        [TestMethod]
        public void MatchReturnsMatchObjectWithSuccessFalseOnNonMatch()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var match = regexEz.Match("before test@sample after");
            Assert.IsFalse(match.Success);
        }

        [TestMethod]
        public void MatchReturnsMatchWithCorrectValueObjectOnMatch()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var match = regexEz.Match("before test@sample.com after");
            Assert.AreEqual("test@sample.com", match.Value);
        }

        [TestMethod]
        public void MatchesReturnsMultipleMatchWithSuccessTrueObjectOnMatch()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var matches = regexEz.Matches("before test@sample.com after another@domain.com and also final@example.com");
            var matchInfo = matches.Select(m => new {m.Success, m.Value}).ToList();
            var expected = new[]
            {
                new { Success = true, Value = "test@sample.com" },
                new { Success = true, Value = "another@domain.com" },
                new { Success = true, Value = "final@example.com" },
            };
            Assert.IsTrue(SequenceEqual(matchInfo, expected));
        }

        [TestMethod]
        public void MatchesReturnsEmptyListOnNoMatchMatch()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var matches = regexEz.Matches("before test@sample after another@domain and also final@example");
            Assert.IsFalse(matches.Any());
        }

        [TestMethod]
        public void MatchObjectReturnsCorrectField()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var match = regexEz.Match("test@sample.com");
            Assert.AreEqual("test", match["username"]);
        }

        [TestMethod]
        public void MatchObjectListReturnsCorrectFieldValues()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var matches = regexEz.Matches("before test@sample.com after another@domain.com and also final@example.com");
            var matchInfo = matches.Select(m => m["username"]).ToList();
            var expected = new[] {"test", "another", "final"};
            Assert.IsTrue(SequenceEqual(matchInfo, expected));
        }


        [TestMethod]
        public void MatchObjectThrowsExceptionForInvalidFieldName()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+"
            };
            var regexEz = new RegexEz(template, true);
            var match = regexEz.Match("test@sample.com");
            Assert.ThrowsException<KeyNotFoundException>(() => match["unknown"]);
        }

        [TestMethod]
        public void PassTestAdded()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$match: test@test.com"
            };
            var regexEz = new RegexEz(template);
            var passTests = GetPassTests(regexEz);
            Assert.AreEqual(passTests.Count, 1);
        }

        [TestMethod]
        public void PassTestCorrectValueStored()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$match: test@test.com"
            };
            var regexEz = new RegexEz(template);
            var passTests = GetPassTests(regexEz);
            Assert.AreEqual(passTests[0], "test@test.com");
        }

        [TestMethod]
        public void ValidPassTestPasses()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$match: test@test.com"
            };
            var regexEz = new RegexEz(template);
            Assert.IsTrue(regexEz.Test());
        }

        [TestMethod]
        public void InvalidPassTestFails()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$match: test@test"
            };
            var regexEz = new RegexEz(template);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void FailTestAdded()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$nomatch: test@test"
            };
            var regexEz = new RegexEz(template);
            var failTest = GetFailTests(regexEz);
            Assert.AreEqual(failTest.Count, 1);
        }

        [TestMethod]
        public void FailTestCorrectValueStored()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$nomatch: test@test"
            };
            var regexEz = new RegexEz(template);
            var failTest = GetFailTests(regexEz);
            Assert.AreEqual(failTest[0], "test@test");
        }

        [TestMethod]
        public void ValidFailTestPasses()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$nomatch: test@test"
            };
            var regexEz = new RegexEz(template);
            Assert.IsTrue(regexEz.Test());
        }

        [TestMethod]
        public void InvalidFailTestFails()
        {
            var template = new[]
            {
                "test: ^$(username)@$(domain)\\.$(tld)$",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$nomatch: test@test.com"
            };
            var regexEz = new RegexEz(template);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void MultiTestAdded()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test test2@domain.com ignore this junk test4@example.com",
                "test@test",
                "test2@domain.com",
                "test4@example.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.AreEqual(multiTests.Count, 1);
        }

        [TestMethod]
        public void MultiTestCorrectValueStored()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.IsTrue(SequenceEqual(multiTests.First().Value, new string[] {"test@test.com", "test2@domain.com", "test4@example.com"}));
        }

        [TestMethod]
        public void MultiTestPasses()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.IsTrue(regexEz.Test());
        }

        [TestMethod]
        public void InvalidMultiTestFailsTooFewMatches()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test4@example.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void InvalidMultiTestFailsInvalidMatch()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test2@nomatch.com",
                "test4@example.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void InvalidMultiTestFailsTooManyMatches()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "thisonenothere@domain.com",
                "$end"
            };
            var regexEz = new RegexEz(template);
            var multiTests = GetMultiMatchTests(regexEz);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void MultitestThrowsExceptionIfNoEnd()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com ignore this junk test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "thisonenothere@domain.com",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void MultitestThrowsExceptionTwoMultimatchForSameString()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "$multimatch: test@test.com test2@domain.com test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "thisonenothere@domain.com",
                $"$end",
                "$multimatch: test@test.com test2@domain.com test4@example.com",
                "test@test.com",
                "test2@domain.com",
                "test4@example.com",
                "thisonenothere@domain.com",
                $"$end",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void FieldTestsFailIfFlagNotSet()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: test@test.com $= username",
                "$field.domain: sample@test.com $= test",
            };
            var regexEz = new RegexEz(template);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void FieldTestsAreFound()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: test@test.com $= username",
                "$field.domain: sample@test.com $= test",
            };
            var regexEz = new RegexEz(template, true);
            var tests = GetFieldTests(regexEz);
            Assert.AreEqual(2, tests.Count);
        }

        [TestMethod]
        public void ThrowsExceptionIfFieldIsMissingFromFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.: test@test.com $= username",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfResultMissingFromFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: test@test.com",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void FieldTestsAreCorrectlyParsed()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: sample@test.com $= sample",
                "$field.domain: sample@test.com $= test",
            };
            var regexEz = new RegexEz(template, true);
            var tests = GetFieldTests(regexEz);
            var expected = new Dictionary<string, Dictionary<int, List<FieldTest>>>
            {
                {
                    "sample@test.com", new Dictionary<int, List<FieldTest>>
                    {
                        { 0, new List<FieldTest> { new ("username", "sample"), new("domain", "test") } },
                    }
                }
            };
            Assert.IsTrue(FieldTestsMatch(expected, tests));
        }

        [TestMethod]
        public void FieldTestsAreCorrectlyParsedWithMultiplePatterns()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: sample@test.com $= sample",
                "$field.domain: sample@test.com $= test",
                "$field.domain: test@example.com $= example",
                "$field.tld: test@example.com $= com",
            };
            var regexEz = new RegexEz(template, true);
            var tests = GetFieldTests(regexEz);
            var expected = new Dictionary<string, Dictionary<int, List<FieldTest>>>
            {
                {
                    "sample@test.com", new Dictionary<int, List<FieldTest>>
                    {
                        { 0, new List<FieldTest> { new ("username", "sample"), new("domain", "test") } },
                    }
                },
                {
                    "test@example.com", new Dictionary<int, List<FieldTest>>
                    {
                        { 0, new List<FieldTest> { new ("domain", "example"), new ("tld", "com") } },
                    }
                }
            };
            Assert.IsTrue(FieldTestsMatch(expected, tests));
        }

        [TestMethod]
        public void ComplexFieldTestsRunSuccessfully()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: sample@test.com $= sample",
                "$field.domain: sample@test.com $= test",
                "$field.domain: test@example.com $= example",
                "$field.tld: test@example.com $= com",
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsTrue(regexEz.Test());
        }

        [TestMethod]
        public void ComplexFieldTestsRunFailIfAnError()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field.username: sample@test.com $= sample",
                "$field.domain: sample@test.com $= test",
                "$field.domain: test@example.com $= example2",
                "$field.tld: test@example.com $= com",
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void FieldTestForMultiMatchAreCorrectlyParsed()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0].username: sample@test.com test@example.com $= sample",
                "$field[1].username: sample@test.com test@example.com $= test",
                "$field[0].domain: sample@test.com test@example.com  $= test",
                "$field[1].domain: sample@test.com test@example.com  $= example",
                "$field[0].username: sample2@test2.com test2@example2.com $= sample2",
                "$field[1].username: sample2@test2.com test2@example2.com $= test2",
                "$field[0].domain: sample2@test2.com test2@example2.com  $= test2",
                "$field[1].domain: sample2@test2.com test2@example2.com  $= example2",
            };
            var regexEz = new RegexEz(template, true);
            var tests = GetFieldTests(regexEz);
            var expected = new Dictionary<string, Dictionary<int, List<FieldTest>>>
            {
                {
                    "sample@test.com test@example.com", new Dictionary<int, List<FieldTest>>
                    {
                        { 0, new List<FieldTest> { new ("username", "sample"), new("domain", "test") } },
                        { 1, new List<FieldTest> { new ("username", "test"), new("domain", "example") } },
                    }
                },
                {
                    "sample2@test2.com test2@example2.com", new Dictionary<int, List<FieldTest>>
                    {
                        { 0, new List<FieldTest> { new ("username", "sample2"), new("domain", "test2") } },
                        { 1, new List<FieldTest> { new ("username", "test2"), new("domain", "example2") } },
                    }
                }
            };
            Assert.IsTrue(FieldTestsMatch(expected, tests));
        }

        [TestMethod]
        public void ThrowsExceptionIfCloseSquareBracketMissingInMultiFieldFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0.username: sample@test.com test@example.com $= sample",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfIndexMissingInMultiFieldFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[].username: sample@test.com test@example.com $= sample",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfIndexInvalidInMultiFieldFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[not-number].username: sample@test.com test@example.com $= sample",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfFieldNameMissingMultiFieldFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0].: sample@test.com test@example.com $= sample",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void ThrowsExceptionIfFieldNameDotMissingMultiFieldFieldTest()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0]username: sample@test.com test@example.com $= sample",
            };
            Assert.ThrowsException<ArgumentException>(() => new RegexEz(template));
        }

        [TestMethod]
        public void FieldTestForMultiMatchPassIfCorrect()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0].username: sample@test.com test@example.com $= sample",
                "$field[1].username: sample@test.com test@example.com $= test",
                "$field[0].domain: sample@test.com test@example.com  $= test",
                "$field[1].domain: sample@test.com test@example.com  $= example",
                "$field[0].username: sample2@test2.com test2@example2.com $= sample2",
                "$field[1].username: sample2@test2.com test2@example2.com $= test2",
                "$field[0].domain: sample2@test2.com test2@example2.com  $= test2",
                "$field[1].domain: sample2@test2.com test2@example2.com  $= example2",
            };
            var regexEz = new RegexEz(template, true);
            var errors = new List<string>();
            Assert.IsTrue(regexEz.Test(errors));
        }

        [TestMethod]
        public void FieldTestForMultiMatchFailIfNotCorrect()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0].username: sample@test.com test@example.com $= sample",
                "$field[1].username: sample@test.com test@example.com $= test",
                "$field[0].domain: sample@test.com test@example.com  $= test",
                "$field[1].domain: sample@test.com test@example.com  $= example",
                "$field[0].username: sample2@test2.com test2@example2.com $= sample",
                "$field[1].username: sample2@test2.com test2@example2.com $= test2",
                "$field[0].domain: sample2@test2.com test2@example2.com  $= test2",
                "$field[1].domain: sample2@test2.com test2@example2.com  $= example2",
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void FieldTestForMultiMatchFailIOutOfRange()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[5].username: sample@test.com test@example.com $= sample",
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsFalse(regexEz.Test());
        }

        [TestMethod]
        public void FieldTestForMultiMatchFailNonExistingField()
        {
            var template = new[]
            {
                "test: $(username)@$(domain)\\.$(tld)",
                "username: $name",
                "domain: $name",
                "tld: $name",
                "name: [a-zA-Z0-9_]+",
                "",
                "$field[0].unknown: sample@test.com test@example.com $= sample",
            };
            var regexEz = new RegexEz(template, true);
            Assert.IsFalse(regexEz.Test());
        }





        private List<string> GetPassTests(RegexEz regexEz)
        {
            var type = regexEz.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var tests = fields.FirstOrDefault(f => f.Name == "_passTests" )?.GetValue(regexEz) as List<string>;
            Assert.IsNotNull(tests);
            return tests;
        }

        private List<string> GetFailTests(RegexEz regexEz)
        {
            var type = regexEz.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var tests = fields.FirstOrDefault(f => f.Name == "_failTests")?.GetValue(regexEz) as List<string>;
            Assert.IsNotNull(tests);
            return tests;
        }

        private Dictionary<string, List<string>> GetMultiMatchTests(RegexEz regexEz)
        {
            var type = regexEz.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var tests = fields.FirstOrDefault(f => f.Name == "_multiMatches")?.GetValue(regexEz) as Dictionary<string, List<string>>;
            Assert.IsNotNull(tests);
            return tests;
        }
        private Dictionary<string, Dictionary<int, List<FieldTest>>> GetFieldTests(RegexEz regexEz)
        {
            var type = regexEz.GetType();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var tests = fields.FirstOrDefault(f => f.Name == "_fieldTests")?.GetValue(regexEz) as Dictionary<string, Dictionary<int, List<FieldTest>>>;
            Assert.IsNotNull(tests);
            return tests;
        }

        private bool FieldTestsMatch(Dictionary<string, Dictionary<int, List<FieldTest>>> expected, Dictionary<string, Dictionary<int, List<FieldTest>>> tests)
        {
            var expectedKeys = expected.Keys;
            var testKeys = tests.Keys;
            if (!expectedKeys.SequenceEqual(testKeys))
                return false;
            foreach (var key in expectedKeys)
            {
                var expectedValues = expected[key];
                var testValues = tests[key];
                if (!expectedValues.Keys.SequenceEqual(testValues.Keys))
                    return false;
                foreach (var valueKey in expectedValues.Keys)
                {
                    var expectedTests = expectedValues[valueKey];
                    var testTests = testValues[valueKey];
                    if (!expectedTests.SequenceEqual(testTests))
                        return false;
                }
            }

            return true;
        }

        private bool SequenceEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            return first.SequenceEqual(second);
        }
    }
}