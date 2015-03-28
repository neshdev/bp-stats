angular.module('bp.core').service('rankingsService', ['$http', '$location', function ($http, $location) {
    
    var serviceBase = 'http://localhost:54648/';
    var url = serviceBase + 'api/Stats/Rankings';

    var getRankings = function () {
        return $http.get(url).then(function (results) {
            return results;
        });
    };

    return {
        getRankings: getRankings
    };
}]);