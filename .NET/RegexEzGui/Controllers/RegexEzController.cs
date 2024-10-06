using Microsoft.AspNetCore.Mvc;
using RegexEzLib;

namespace RegexExGui.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegexEzController : ControllerBase
    {
        [HttpPost("CheckForMatch")]
        public IActionResult CheckForMatch([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);

                var match = regexEz.Match(request.InputString);

                return Ok(new { value = match.Value });
            }
            catch (Exception ex)
            {
                return Ok(new { errorMessage = ex.Message });
            }
        }

        [HttpPost("checkForMultiMatch")]
        public IActionResult checkForMultiMatch([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);

                var match = regexEz.Matches(request.InputString);

                return Ok(new { multiValues = match.Select(m => m.Value) });
            }
            catch (Exception ex)
            {
                return Ok(new { errorMessage = ex.Message });
            }
        }

        [HttpPost("GetFieldValue")]
        public IActionResult GetFieldValue([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);
                string fieldValue = regexEz.Match(request.InputString)[request.Field];
                return Ok(new { FieldValue = fieldValue });
            }
            catch (Exception ex)
            {
                return Ok(new { errorMessage = ex.Message });
            }
        }

        [HttpPost("GetFieldMultiMatch")]
        public IActionResult GetFieldMultiMatch([FromBody] StringTestRequest request)
        {
            try
            {
                var regexEz = new RegexEz(request.Pattern, true);
                var matches = regexEz.Matches(request.InputString);
                if (request.MatchNum != null && request.MatchNum.Value < matches.Count)
                {
                    var fieldValue = matches[request.MatchNum.Value][request.Field];
                    return Ok(new { FieldValue = fieldValue });
                }
                return Ok(new { errorMessage = "MatchNumber out of range" });
            }
            catch (Exception ex)
            {
                return Ok(new { errorMessage = ex.Message });
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
                return Ok(new { errorMessage = ex.Message });
            }
        }




    }

    public class StringTestRequest
    {
        public string Pattern { get; set; } = string.Empty;
        public string InputString { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public int? MatchNum { get; set; }
    }
}

