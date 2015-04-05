/// <reference path="historyservice.ts" />
/// <reference path="searchcontroller.ts" />
(function () {
    angular.module("bp.core").controller("HistoryController", HistoryController);

    HistoryController.$inject = ["$scope", "HistoryService", "UserSettingsFactory"];

    function HistoryController($scope: any, HistoryService: any, UserSettingsFactory: any) {
        $scope.games = [];
        $scope.message = "";
        $scope.currentUserName = "";
        $scope.reload = reload;
        $scope.sendConfirmation = sendConfirmation;

        function reload() {

            var success = function (response) {
                var currentUser = $scope.currentUserName;
                var games = response.data.value;

                for (var i = 0; i < games.length; i++) {
                    var game = games[i];
                    var header = "";
                    var currentUsersGameResults;
                    var isWinningUser = false;
                    var winners = [];
                    var losers = [];
                    for (var j = 0; j < game.gameResults.length; j++) {
                        var gameResult = game.gameResults[j];
                        if (gameResult.userName === currentUser) {
                            currentUsersGameResults = gameResult;
                        }
                        if (gameResult.outcome === "Win") {
                            winners.push(gameResult.userName);
                        } else {
                            losers.push(gameResult.userName);
                        }
                    }

                    if (currentUsersGameResults.outcome === "Win") {
                        header = winners.join(" and ") + " vs " + losers.join(" and ");
                        isWinningUser = true;
                    } else {
                        header = losers.join(" and ") + " vs " + winners.join(" and ");
                    }

                    var extendObj = {
                        currentUsersGameResult: currentUsersGameResults
                        , isWinningUser: isWinningUser
                        , header: header
                    };
                    game.extendObj = extendObj;
                }
                $scope.games = games;
            };





            var error = function (err) {
                $scope.message = JSON.stringify(err.data.error.message);
            };

            HistoryService.getGameHistory().then(success, error);

        }

        function activate() {
            $scope.currentUserName = UserSettingsFactory.getUserName();
            reload();
        }

        function sendConfirmation(game, isConfirmed) {
            var gameResult = game.extendObj.currentUsersGameResult;
            gameResult.isConfirmed = isConfirmed;

            var success = function (data) {
                // todo: what should we do here
            };

            var error = function (err) {
                // todo: remove whole error
                $scope.message = JSON.stringify(err);
            };

            HistoryService.updateGame(gameResult).then(success, error);
        }

        activate();
    }

})();