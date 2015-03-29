angular
    .module('bp.core')
    .controller('statsController', ['$scope', '$http', function ($scope, $http) {
        $scope.message = "this is harija";
        $scope.playerStats = [];

        $scope.getStats = function () {
            $http.get('/api/playerstats')
                .success(function (data, status, headers, config) {
                    $scope.playerStats = data;
                    $scope.message = 'completed';
            });
        };
    }]);