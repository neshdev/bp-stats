(function () {
    'use strict';
    angular
        .module("bp.core")
        .factory("GameService", GameService);

    GameService.$inject = ['$http'];

    function GameService($http: ng.IHttpService) {

        function GetStats(username, outcome, matchup) {
            var url = "/odata/GameResults?$filter=userName eq '" + username
                + "' and outcome eq NeshHouse.Stats.Web.Models.GameOutcome'" + outcome
                + "' and game/matchup eq NeshHouse.Stats.Web.Models.Matchup'" + matchup + "'";

            return $http.get(url);
        }

        var _getWinsForOneOnOne = function (username) {
            return GetStats(username, "Win", "OneOnOne");
        };
        var _getLossForOneOnOne = function (username){
            return GetStats(username, "Loss", "OneOnOne");
        };

        var _getWinsForTwoOnTwo = function (username) {
            return GetStats(username, "Win", "TwoOnTwo");
        };
        var _getLossForTwoOnTwo = function (username) {
            return GetStats(username, "Loss", "TwoOnTwo");
        };

        return {
            getWinsForOneOnOne: _getWinsForOneOnOne,
            getLossForOneOnOne: _getLossForOneOnOne,
            getWinsForTwoOnTwo: _getWinsForTwoOnTwo,
            getLossForTwoOnTwo: _getLossForTwoOnTwo
        };
    }

})();