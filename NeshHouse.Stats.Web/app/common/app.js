(function () {
    'use strict';

    angular.module('bp.core', ['ngRoute', 'ngCookies', 'LocalStorageModule', 'angular-loading-bar', 'ui.bootstrap', 'd3'])
            .config(function ($httpProvider) {
                $httpProvider.interceptors.push('authInterceptorService');
            })
           .config(['$routeProvider', function ($routeProvider) {
               $routeProvider
                   .when('/home', { templateUrl: 'home/home.html' })
                   .when('/search', { templateUrl: 'game/search.html' })
                   .when('/history', { templateUrl: 'game/history.html' })
                   .when('/rankings', { templateUrl: 'stats/rankings.html' })
                   .when('/stats', { templateUrl: 'stats/stats.html' })
                   .when('/login', { templateUrl: 'login/login.html' })
                   .when('/signup', { templateUrl: 'login/signup.html' })
                   .otherwise({ redirectTo: '/rankings' });
           }]);

    angular.module('bp.core').run(['authService', function (authService) {
            authService.fillAuthData();
        }]);
})();

