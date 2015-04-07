(function () {
    'use strict';

    angular.module('bp.core')
           .service('RankingsService', RankingsService);

    RankingsService.$inject = ['$http'];

    function RankingsService($http) {
        this.getRankings = function () {
            var url = '/api/rankings';
            return $http.get(url);
        };
    }
})();

