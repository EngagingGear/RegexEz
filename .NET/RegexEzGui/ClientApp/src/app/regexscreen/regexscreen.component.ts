import { Component } from '@angular/core';
import { RegexService } from './regex.service';  // Adjust the path as necessary

@Component({
  selector: 'app-regexscreen',
  templateUrl: './regexscreen.component.html',
  styleUrls: ['./regexscreen.component.css']
})
export class RegexScreenComponent {
  regexPattern = `// This is a comment
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
  testString = "fraserorr@yahoo.com";
  testResults = "";
  fieldName = "";
  matchNum = "";
  errorMessage = "";
  matchStyle = "";

  constructor(private regexService: RegexService) { }

  checkMatch() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
    };
    this.clear();
    this.regexService.checkForMatch(request).subscribe(
      (response) => {
        if (!this.isError(response)) {
          if (response.value) {
            this.testResults = "Match found:\r\n" + response.value;
          } else {
            this.testResults = "No match found";
          }
        }
      },
      (error) => {
        this.handleError(error);
      }
    );
  }

  checkMultiMatches() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
    };
    this.clear();
    this.regexService.checkForMultiMatch(request).subscribe(
      (response) => {
        if (!this.isError(response)) {
          if (response.multiValues) {
            debugger;
            this.testResults = "Match found:\r\n";
            for (let m of response.multiValues)
              this.testResults += m + "\r\n";
          } else {
            this.testResults = "No matches found";
          }
        }
      },
      (error) => {
        this.handleError(error);
      }
    );
  }
    getFieldValue() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
      field: this.fieldName
    };

    this.clear();
    this.regexService.getFieldValue(request).subscribe(
      (response: any) => {
          if (!this.isError(response)) {
            this.testResults = `Field: ${this.fieldName}\r\nValue: ${response.fieldValue}`;
          }
      },
      (error) => {
        this.handleError(error);
      }
    );
  }

  getFieldValueMultiMatch() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
      field: this.fieldName,
      matchNum: Number(this.matchNum)
  };

    this.clear();
    this.regexService.getFieldMultiMatch(request).subscribe(
      (response: any) => {
        if (!this.isError(response)) {
          this.testResults = `Field: ${this.fieldName}\r\nMatchNum: ${this.matchNum}\r\nValue: ${response.fieldValue}`;
        }
      },
      (error) => {
        this.handleError(error);
      }
    );
  }

  // Method to run unit tests via backend
  runTests() {
    const request = {
      pattern: this.regexPattern.replace(/\n/g, '\r\n'),
      inputString: this.testString,
      field: this.fieldName,
    };

    this.clear();
    this.regexService.runUnitTest(request).subscribe(
      (response) => {
        if (!this.isError(response)) {
          this.testResults = response.testPassed
            ? 'All unit tests passed successfully!'
            : `Test failures: ${response.failures.join(', ')}`;
        }
      },
      (error) => {
        this.handleError(error);
      }
    );
  }

  clear() {
    this.testResults = "";
    this.errorMessage = "";
  }

  private isError(response: any): boolean {
    if (response.errorMessage) {
      this.handleError(response);
      return true;
    }
    return false;
  }

  private handleError(error: any) {
    this.clear();
    this.errorMessage = error.message ?? error.errorMessage ?? "Error";
  }
}
