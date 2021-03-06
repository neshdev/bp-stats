(function () {
    angular.module('bp.core').service('HistoryService', HistoryService);
    HistoryService.$inject = ['$http', 'UserSettingsFactory'];
    function HistoryService($http, UserSettingsFactory) {
        this.getGameHistory = function () {
            //var url = "/odata/Games?$expand=gameResults&$filter=gameResults/any(o: o/userName eq '" + UserSettingsFactory.getUserName() + "' ) and status eq NeshHouse.Stats.Web.Models.GameStatus'PendingConfirmation'";
            var url = "/odata/Games?$orderby=id desc&$expand=gameResults&$filter=gameResults/any(o: o/userName eq '" + UserSettingsFactory.getUserName() + "' )";
            return $http.get(url);
        };
        this.updateGame = function (gameResult) {
            var url = "/odata/GameResults(" + gameResult.id + ")";
            return $http.put(url, gameResult);
        };
    }
})();
//# sourceMappingURL=historyservice.js.map