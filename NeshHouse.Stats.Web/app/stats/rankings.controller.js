(function () {
    'use strict';

    angular.module('bp.core')
        .controller('RankingsController', RankingsController);

    RankingsController.$inject = ['$scope', 'RankingsService']

    function RankingsController($scope, RankingsService) {
        $scope.rankings = [];
        $scope.message = '';

        function activate() {

            var success = function (response) {
                for (var i = 0; i < response.data.length; i++) {
                    var ranking = response.data[i];
                    $scope.rankings.push(ranking);
                }
            };

            var error = function (err) {
                $scope.message = JSON.stringify(err);
            }

            RankingsService.getRankings().then(success, error);
        }
        activate();
    }
})();