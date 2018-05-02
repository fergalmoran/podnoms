import { environment } from 'environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { ProfileModel } from 'app/models/profile.model';
import 'rxjs/add/operator/map';
<<<<<<< HEAD
=======
import { Profile } from 'selenium-webdriver/firefox';
import { HttpClient } from '@angular/common/http';
>>>>>>> develop

@Injectable()
export class ProfileService {
    profile: ProfileModel;
    constructor(private _http: HttpClient) {}

    constructor(private _http: AuthHttp) {}

    getProfile(): Observable<ProfileModel> {
        if (!this.profile) {
            return this._http
<<<<<<< HEAD
                .get(environment.API_HOST + '/profile')
                .map((res) => {
                    this.profile = res.json();
=======
                .get<ProfileModel>(environment.API_HOST + '/profile')
                .map((res) => {
                    this.profile = res;
>>>>>>> develop
                    return this.profile;
                });
        } else {
            return Observable.of(this.profile);
        }
    }

    updateProfile(profile): Observable<ProfileModel> {
        console.log('ProfileService', 'updateProfile', profile);
<<<<<<< HEAD
        return this._http
            .post(environment.API_HOST + '/profile', profile)
            .map((res) => res.json());
=======
        return this._http.post<ProfileModel>(
            environment.API_HOST + '/profile',
            profile
        );
>>>>>>> develop
    }

    checkSlug(slug): Observable<Response> {
        console.log('profile.service.ts', 'checkSlug', slug);
<<<<<<< HEAD
        return this._http
            .get(environment.API_HOST + '/profile/checkslug/' + slug)
            .map((res) => res.text());
    }
    regenerateApiKey(): Observable<string> {
        return this._http
            .post(environment.API_HOST + '/profile/updateapikey', null)
            .map((res) => res.text());
=======
        return this._http.get<Response>(
            environment.API_HOST + '/profile/checkslug/' + slug
        );
    }
    regenerateApiKey(): Observable<string> {
        return this._http.post<string>(
            environment.API_HOST + '/profile/updateapikey',
            null
        );
>>>>>>> develop
    }
}
