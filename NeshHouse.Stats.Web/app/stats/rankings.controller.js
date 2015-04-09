(function () {
    'use strict';

    angular.module('bp.core')
        .controller('RankingsController', RankingsController);

    RankingsController.$inject = ['$scope', 'RankingsService']

    function RankingsController($scope, RankingsService) {
        $scope.rankings = [];
        $scope.message = '';
        $scope.matchup = '1';
        $scope.getRankings = getRankings;

        function getRankings() {
            var success = function (response) {
                var rankings = [];
                for (var i = 0; i < response.data.length; i++) {
                    var ranking = response.data[i];
                    rankings.push(ranking);
                }
                $scope.rankings = rankings;
            };

            var error = function (err) {
                $scope.message = JSON.stringify(err);
            }

            RankingsService.getRankings($scope.matchup).then(success, error);
        }
        

        function activate() {
            getRankings();
        }

        activate();
    }
})();