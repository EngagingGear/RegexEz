import { Component } from '@angular/core';
import { RegexService } from './regex.service';  // Adjust the path as necessary

@Component({
  selector: 'app-regexscreen',
  templateUrl: './regexscreen.component.html',
  styleUrls: ['./regexscreen.component.css']
})
export class RegexscreenComponent {
  regexPattern: string =`// This is a comment
test: ^$(username)@$(domain)\\.$(tld)$
username: $name
domain: $name
tld: $name
name: [a-zA-Z0-9_]+

$match: fraser@yahoo.com
$match: fraser_orr@yahoo.com
$noMatch: fraser@yahoo
$noMatch: fraser-orr@yahoo.com
$field.username: fraser@yahoo.com $= fraser
$field.domain: fraser@yahoo.com $= yahoo
$field.tld: fraser@yahoo.com $= com`;
  testString: string = '';
  matchResult: any = '';
  domainString: any = '';
  testResults: any = '';
  domainResult: any = '';
  stringTestPattern =
    {
      Pattern: this.regexPattern,
      InputString: this.testString,
      Field: this.domainString,

    }

  constructor(private regexService: RegexService) { }

  // Method to generate regex pattern via backend
  //generateRegexPattern() {
  //  this.regexService.generateRegexPattern(this.stringTestPattern).subscribe(
  //    (response) => {
  //      this.regexPattern = response.regexPattern || 'Pattern not generated.';
  //    },
  //    (error) => {
  //      console.error('Error generating regex pattern:', error);
  //    }
  //  );
  //}

  // Method to check for match via backend
  checkMatch() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
    };

    this.regexService.CheckForMatch(request).subscribe(
      (response) => {
        this.matchResult = response
          ? `This Pattern Matches`
          : 'This Pattern Not Match.';
      },
      (error) => {
        console.error('Error testing string:', error);
      }
    );
  }

  // Method to extract domain value via backend
  getFieldValue() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
      field: this.domainString, // Hardcoded domain field extraction
    };

    this.regexService.GetFieldValue(request).subscribe(
      (response: any) => {
        try {
          // Try to parse the response as JSON


          // If successfully parsed, store the parsed JSON object
          this.domainResult = response.fieldValue;
        } catch (error) {
          // If parsing fails, store the response as a string
          this.domainResult = response || 'No domain found.';
        }

        console.log(this.domainResult); // This will print the final result
      },
      (error) => {
        console.error('Error occurred:', error);
      }
    );
  }

  // Method to run unit tests via backend
  runTests() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
      field: this.domainString,
    };

    this.regexService.RunUnitTest(request).subscribe(
      (response) => {
        this.testResults = response.testPassed
          ? 'All unit tests passed successfully!'
          : `Test failures: ${response.failures.join(', ')}`;
      },
      (error) => {
        console.error('Error running unit tests:', error);
      }
    );
  }
}
