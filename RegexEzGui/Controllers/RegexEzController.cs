using Microsoft.AspNetCore.Mvc;
using RegexEzLib;

namespace RegexExGui.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegexEzController : ControllerBase
    {
//        string pattern = @"// This is a comment
//test: ^$(username)@$(domain)\.$(tld)$
//username: $name
//domain: $name
//tld: $name
//name: [a-zA-Z0-9_]+

//$match: fraser@yahoo.com
//$match: fraser_orr@yahoo.com
//$noMatch: fraser@yahoo
//$noMatch: fraser-orr@yahoo.com
//$field.username: fraser@yahoo.com $= fraser
//$field.domain: fraser@yahoo.com $= yahoo
//$field.tld: fraser@yahoo.com $= com
//";

        [HttpPost("CheckForMatch")]
        public IActionResult CheckForMatch([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);

                bool match = regexEz.Match(request.InputString);

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during regex testing: {ex.Message}");
            }
        }

        [HttpPost("GetFieldValue")]
        public IActionResult GetFieldValue([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);


                bool match = regexEz.Match(request.InputString);
                string fieldValue = request.Field != null ? regexEz.GetField(request.InputString, request.Field) : null;

                return Ok(new { FieldValue = fieldValue });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during regex testing: {ex.Message}");
            }
        }
        [HttpPost("RunUnitTest")]
        public IActionResult RunUnitTest([FromBody] StringTestRequest request)
        {
            try
            {

                var regexEz = new RegexEz(request.Pattern, true);


              
                List<string> failures = new();
                bool passed = regexEz.Test(failures);

               
                return Ok(new
                {
                    TestPassed = passed,
                    Failures = failures.Count > 0 ? failures : null,
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error during regex test run: {ex.Message}");
                return BadRequest($"Error during regex test run: {ex.Message}");
            }
        }




    }

    public class StringTestRequest
    {
        public string? Pattern { get; set; }
        public string? InputString { get; set; }
        public string? Field { get; set; }
    }
}

