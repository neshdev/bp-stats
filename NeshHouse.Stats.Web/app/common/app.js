(function () {
    'use strict';

    angular.module('bp.core', ['ngRoute', 'ngCookies', 'LocalStorageModule', 'angular-loading-bar', 'ui.bootstrap'])
           .config(['$routeProvider', function ($routeProvider) {
               $routeProvider
                   .when('/search', { templateUrl: 'game/search.html' })
                   .when('/rankings', { templateUrl: 'stats/rankings.html' })
                   .when('/login', { templateUrl: 'login/login.html'})
                   .otherwise({ redirectTo: '/rankings' });
           }]);

    angular.module('bp.core').run(['authService', function (authService) {
            authService.fillAuthData();
        }]);
})();

