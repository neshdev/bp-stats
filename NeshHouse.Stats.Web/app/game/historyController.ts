/// <reference path="historyservice.ts" />
/// <reference path="searchcontroller.ts" />
 

(function () {
    angular.module('bp.core').controller('HistoryController', HistoryController);

    HistoryController.$inject = ['$scope', 'HistoryService' ];

    function HistoryController($scope: any, HistoryService: any) {
        $scope.games = [];
        $scope.message = '';

        function activate() {
            $scope.reload();
        }

        $scope.reload = function () {

            var success = function (response) {
                $scope.games = response.data.value;
            };

            var error = function (err) {
                $scope.message = JSON.stringify(err.data.error.message);
            }

            HistoryService.getGameHistory().then(success, error);
        }

        activate();


    }

})();