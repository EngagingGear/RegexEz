// regex.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

interface StringTestRequest {
  pattern: string;
  inputString: string;
  field?: string;
}

@Injectable({
  providedIn: 'root',
})
export class RegexService {
  private baseUrl = 'https://localhost:44484/'; // Ensure a trailing slash here

  constructor(private http: HttpClient) { }

  // Call to test a string against a regex pattern
  CheckForMatch(request: StringTestRequest): Observable<any> {
    return this.http.post(`https://localhost:44484/RegexEz/CheckForMatch`, request); // Ensure proper URL
  }

  // Call to test a string against a regex pattern
  GetFieldValue(request: any, options?: any) {
    return this.http.post('https://localhost:44484/RegexEz/GetFieldValue', request);
  }


  // Call to run unit tests
  RunUnitTest(request: StringTestRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}RegexEz/RunUnitTest`, request); // Ensure proper URL
  }
}
