(function () {
    'use strict';

    angular.module('bp.core')
           .service('RankingsService', RankingsService);

    RankingsService.$inject = ['$http'];

    function RankingsService($http) {
        this.getRankings = function (matchup) {
            var url = '/api/rankings?matchup=' + matchup;
            return $http.get(url);
        };
    }
})();

