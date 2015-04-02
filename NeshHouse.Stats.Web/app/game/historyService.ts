/// <reference path="searchcontroller.ts" />


module Beerpong {
    
    export class HistoryService {

        static $inject = ['$http', 'UserSettingsFactory']

        constructor($http: ng.IHttpService, UserSettingsFactory: any) {
            function getGameHistory() {
                var that = this;
                var success = function (data) {
                    return data;
                };

                var error = function (err) {
                    console.log(err);
                };

                return that.$http.get('/api/GameHistory?userName=' + UserSettingsFactory.userName ).then(success, error);
            };
        }
    }

    angular.module('bp.core').service('HistoryService', HistoryService);
}
