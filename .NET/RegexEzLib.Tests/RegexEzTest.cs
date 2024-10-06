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
    }
}