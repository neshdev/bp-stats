(function () {
    'use strict';

    angular.module('bp.core')
        .controller("PieStatsController", PieStatsController);

    PieStatsController.$inject = ['$scope'];

    function PieStatsController($scope) {
        $scope.data = [
             { label: 'Wins', count: 25 }
            ,{ label: 'Losses', count: 20 }
        ];
    };

})(); 