(function () {
    'use strict';
    angular.module('bp.core').controller("PieStatsController", PieStatsController);
    PieStatsController.$inject = ['$scope'];
    function PieStatsController($scope) {
        $scope.data = {
            title: "1v1",
            values: [
                { label: 'Wins', count: 25 },
                { label: 'Losses', count: 20 }
            ]
        };
    }
    ;
})();
//# sourceMappingURL=pieStats.controller.js.map