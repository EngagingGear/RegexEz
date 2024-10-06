# RegexEz - Modernizing Regular Expressions
Modern software practices have embraced certain key ideas such as the primacy of clarity and simplicity in code, the idea of breaking down complexity into its component parts, the recognition that code is read more than it is written and the key importance of unit testing.
However, one common aspect of programming that pervades all languages and frameworks is the regular expression -- a notorious exception to this modern practice.
Regular expressions are extremely difficult to read and understand and are in a sense "write only". Even the authors of the code can have a very under time understanding them.
They are writien in an archaic language designed for conciseness rather than understandability so that really understanding what they do is very challenging.
This is further compounded by the extreme difficulty of using capture groups to extract parts of the matched text.

RegexEz is a library that is designed to introduce the ideas of modern software design into this very useful technique, even though its syntax and usage is a relic of the very early days of computer programming.
We do this by making four specific changes:
* We allow them to be broken down into component parts with high quality names
* We allow the use of comments to explain their meaning
* We introduce a built in unit testing framework that both tests the regular expression and also documents the expectations of the programmer
* We provide a mechanism to access the parts of the regular expression easiliy.

## A Sample RegexEz Specification
As an example we will look at a regular expression designed to match an email address.
Matching actual email addresses allowing for all the possible complexity and expecially non latin characters is very complex and so we are going to use a fairly simple form, since our purpose is to illustrate the ideas of RegexEz.
The type of email address we will match is a name consisting of alphanumerics, and @ sign, a domain (again of alpha numerics) a . and a TLD.
In a regular expression we would have something like:
```
[a-zA-Z][a-zA-Z0-9]*@[a-zA-Z][a-zA-Z0-9]*\.[a-zA-Z][a-zA-Z0-9]*
```
This is a pretty hard to follow stream of characters. In RegexEz we can specify this much more clearly:
```
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*
```
We could also do:
```
email: $(name)@$name)\.$(name)
name: [a-zA-Z][a-zA-Z0-9]*
```
However, this former is better since it gives a lot more clarity as to what the pieces mean, and we will see later there is another significant advantage to this.

It is important to note the this is complied down to the exact same regular expression and underlying finite state automaton.
You are not sacrificing any performance aside from the compliation time, which is very short.

This is designed to look very much like a BNF grammar, the main difference is that recurrsion is not allowed since it is not supported by the underlying regular expression.
RegexEz detects recursion and throws an error if found.

How this is used in code is as follows:
* First of all install the package RegexEz
* Secondly in code add a using for RegexEzLib
* Now in your code you can do:
```
var template = @"
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*
";
var regexEz = new RegexEz(template); // This compiles the template
Console.WriteLine($"Regular expression is ${regexEz.RegexStr()}");
var theActualRegex = regexEz.Regex(); // Return a traditional regular expression
var match = regexEz.Match("sample@example.com"); // Use RegexEz to do a match
```

`RegexEz` class also has a number of other methods that can be useful for matching, matching multiple items and some other things we will discuss later.

## Comments and Details About Syntax
Since regular expressions are complex and difficult we can also include comments in the regular expression with a // line comment.

```
\\ This matches a simple email address
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*
```
Notice that the grammar specifies a name, then a colon and then to the left is the regular expression.
* Note any spaces between the colon and the start are ignored (you can add them explicitly if needed by matching them).
* The expression is simply a standard regular expression except that it allows substitution of macros with a $.
* Note we use $ since $ is used except in rare circumstances at the end of a regular expression so in the middle it is usually unambiguous.
* Macro names to be substituted in can use used plainly or with () around them if such is necessary to resolve ambituity.
* All other types of regular expression grammar may be used (in the sample we use ^$ and we must escape the . since we want it explicitly.
* Note it is generally discouraged to use capture group markers in RegexEz since they are hard to understand, different in different systems and RegexEz provides a much better way to access this information.

## Unit Testing
Unit testing is at the heart of modern day software development and so RegexEz has it built into the core.
```
email: ^$(username)@$domain)\.$(tld)$
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*

$match: sample@sample.com
$noMatch: sample@eample
```

Here you see that we added a $match line which is a unit test indicating that the test on the right should match the expression and #nomatch indicating that the string on the right should not match.

Regular expressions can also match multiple items in a string. 
In our example we have anchored the expression to the beginning and the end, but that is not necessary and we can unit test for multiple matches:
```
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*

$multimatch: sample@sample.com junk to ignore sample2@example.come
sample@sample.com
sample2@example.com
$end
$noMatch: sample@eample
```

Here we put each of multiple match (in order) on several lines and end with the special $end command.

To execute the tests we call
```
var regexEz = new RegexEz(template);
Assert.IsTrue(regexEz.runTests());
```

We can also get details of the failing tests:
```
var regexEz = new RegexEz(template);
var errors = new List<string>();
regexEz.runTests(errors);
foreach(var error in errors)
  Console.WriteLine(error);
```

# Extracting field values
Often we want to extract certain parts of the regular expression, for example, you might want to extract the user name or the domain.
This is possible with regular expressions but it is quite hard and makes the syntax or regular expressions even more obtuse. 
So RegexEz has built in mechanisms to make this sort of extration easier:
```
email: ^$(username)@$domain)\.$(tld)$
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*
```

What we want to get are the macros we have defined, so to do this:
```
var regexEz = new RegexEz(pattern, true);
var test = "sample@example.com";
Console.WriteLine($"User name = {regexEz.GetField(test, "username")}, domain = {regexEz.GetField(test, "domain")}.{regexEz.GetField(test, "tld")}");
```

A couple of things to note:
* When you want to use this field extraction you must include true as the second parameter to the regular expression constructor.
  This flag tells RegexEz to include appropriate tags in the regular expression to make extraction possible.
* To extract a file use the GetField method passing in the test string and the name of the macro to extract.

You can also unit test this:
```
email: ^$(username)@$domain)\.$(tld)$
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*

$field.username: sample@example.com $= sample
$field.domain: sample@example.com $= example
$field.tld: sample@example.com $= com
```

Finally you can extract these fields when there are multiple matches, With this template:
```
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*
```

We can then get the usernames from a multi match as follows:
```
var regexEz = new RegexEz(pattern, true);
var test = "sample@sample.com sample2@example.com";
Console.WriteLine($"{regexEz.GetMultiField(test, "username", 0)},{regexEz.GetMultiField(test, "username", 1)}");
```

We can also unit test this with:
```
email: $(username)@$domain)\.$(tld)
username: $name
domain: $name
tld: $name
name: [a-zA-Z][a-zA-Z0-9]*

$field[0].username: sample@example.com sample1@example.com $= sample
$field[1].username: sample@example.com sample1@example.com $= sample1
```
