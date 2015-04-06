(function () {
    'use strict';
    angular.module('bp.core').controller("StatsController", StatsController);
    StatsController.$inject = ['$scope', '$q', 'GameService', 'UserSettingsFactory'];
    function StatsController($scope, $q, GameService, UserSettingsFactory) {
        $scope.username = UserSettingsFactory.getUserName();
        $scope.data = {
            oneOnOne: [],
            twoOnTwo: [],
        };
        $scope.showOneOnOne = false;
        $scope.showTwoOnTwo = false;
        $scope.message = "";
        var activate = function () {
            var ovoWSuccess = function (response) {
                $scope.data.oneOnOne.push({
                    label: "Wins",
                    count: response.data.value.length
                });
            };
            var ovoLSuccess = function (response) {
                $scope.data.oneOnOne.push({
                    label: "Losses",
                    count: response.data.value.length
                });
            };
            var tvtWSuccess = function (response) {
                $scope.data.twoOnTwo.push({
                    label: "Wins",
                    count: response.data.value.length
                });
            };
            var tvtLSuccess = function (response) {
                $scope.data.twoOnTwo.push({
                    label: "Losses",
                    count: response.data.value.length
                });
            };
            var error = function (err) {
                $scope.message = JSON.stringify(err);
            };
            $q.all([
                GameService.getWinsForOneOnOne($scope.username).then(ovoWSuccess, error),
                GameService.getLossForOneOnOne($scope.username).then(ovoLSuccess, error),
                GameService.getWinsForTwoOnTwo($scope.username).then(tvtWSuccess, error),
                GameService.getLossForTwoOnTwo($scope.username).then(tvtLSuccess, error),
            ]).then(function () {
                $scope.showOneOnOne = $scope.data.oneOnOne[0].count > 0 || $scope.data.oneOnOne[1].count > 0;
                $scope.showTwoOnTwo = $scope.data.twoOnTwo[0].count > 0 || $scope.data.twoOnTwo[1].count > 0;
            });
        };
        activate();
    }
    ;
})();
//# sourceMappingURL=stats.controller.js.map