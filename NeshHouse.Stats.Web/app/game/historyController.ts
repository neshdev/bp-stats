/// <reference path="historyservice.ts" />
/// <reference path="searchcontroller.ts" />
 

module Beerpong {

    interface HistoryScope extends ng.IScope {
        games: Game[];
        reload();
    }

    export class HistoryController{

        static $inject = ['$scope','HistoryService'];

        constructor(private $scope: HistoryScope, private HistoryService: any) {
            $scope.games = [];
            
            function activate() {
                this.$scope.reload();
            };

            $scope.reload = function () {
                var _this = this;
                HistoryService.getGameHistory.then(function (data) {
                    _this.$scope.games = data;
                });
            };

            activate();
        }
    }

}