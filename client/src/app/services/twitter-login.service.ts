import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class TwitterLoginService {
    constructor(private _http: HttpClient) {}

    getAuthToken(): Observable<string> {
        return this._http.get<string>('ads');
    }
}
