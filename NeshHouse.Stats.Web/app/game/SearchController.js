/// <reference path="../../scripts/typings/underscore/underscore.d.ts" />
Array.prototype.inArray = function (comparer) {
    for (var i = 0; i < this.length; i++) {
        if (comparer(this[i]))
            return true;
    }
    return false;
};
// adds an element to the array if it does not already exist using a comparer 
// function
Array.prototype.pushIfNotExist = function (element, comparer) {
    if (!this.inArray(comparer)) {
        this.push(element);
    }
};
Array.prototype.remove = function (element, comparer) {
    for (var i = 0; i < this.length; i++) {
        if (comparer(this[i])) {
            this.splice(i, 1);
        }
    }
};
var Beerpong;
(function (Beerpong) {
    var SearchController = (function () {
        function SearchController($scope, localStorageService, $location) {
            this.$scope = $scope;
            this.localStorageService = localStorageService;
            this.$location = $location;
            var vm = this;
            $scope.userGroups = [];
            $scope.gameFound = false;
            $scope.roomName = '';
            $scope.message = '';
            var joinLobbySuccess = function (data) {
                vm.$scope.$apply(function () {
                    for (var i = 0; i < data.length; i++) {
                        vm.$scope.userGroups.pushIfNotExist(data[i], function (e) {
                            return data[i].groupName === e.groupName && data[i].team === e.team && data[i].userName === e.userName;
                        });
                    }
                    ;
                    vm.$scope.gameFound = true;
                });
            };
            var joinLobbyError = function (err) {
                vm.$scope.$apply(function () {
                    $scope.message = err.description;
                });
            };
            $scope.joinLobby = function (team) {
                if (vm.$scope.roomName) {
                    vm._beerpongHub.server.joinLobby(vm.$scope.roomName, team).then(joinLobbySuccess, joinLobbyError);
                }
                else {
                    vm.$scope.message = 'Error: Please enter room name';
                }
            };
            $scope.leaveLobby = function () {
                if (vm.$scope.roomName) {
                    vm._beerpongHub.server.unjoinLobby(vm.$scope.roomName);
                    vm.$scope.userGroups = [];
                    vm.$scope.gameFound = false;
                    vm.$scope.roomName = '';
                    vm.$scope.message = '';
                }
            };
            var onReportWinSuccess = function (data) {
                vm.$scope.$apply(function () {
                    vm.$location.path("/stats");
                });
            };
            var onReportWinError = function (err) {
                vm.$scope.$apply(function () {
                    $scope.message = err.description;
                });
            };
            $scope.reportWin = function (winningTeam) {
                vm._beerpongHub.server.reportWin(vm.$scope.roomName, winningTeam).then(onReportWinSuccess, onReportWinError);
            };
            $scope.$on('$destroy', function () {
                $.connection.hub.stop();
            });
            vm.activate();
        }
        SearchController.prototype.signalrRStarted = function () {
            console.log("Now connected, connection ID=" + $.connection.hub.id);
        };
        SearchController.prototype.signlarRFailed = function () {
            console.log("Could not connect!");
        };
        SearchController.prototype.activate = function () {
            var _this = this;
            this._beerpongHub = $.connection.beerpongHub;
            $.connection.hub.logging = true;
            var authData = this.localStorageService.get('authorizationData');
            $.connection.hub.qs = { 'Bearer': authData.token };
            this._beerpongHub.client.disconnect = function () {
                $.connection.hub.stop();
            };
            this._beerpongHub.client.joinedLobby = function (ug) {
                var that = _this;
                that.$scope.$apply(function () {
                    //remove user without looking at team
                    that.$scope.userGroups.remove(ug, function (e) {
                        return ug.groupName == e.groupName && ug.userName == e.userName;
                    });
                    //add user to team
                    that.$scope.userGroups.pushIfNotExist(ug, function (e) {
                        return ug.groupName === e.groupName && ug.team === e.team && ug.userName === e.userName;
                    });
                });
            };
            this._beerpongHub.client.unjoinedLobby = function (ug) {
                var that = _this;
                that.$scope.$apply(function () {
                    //remove user without looking at team
                    that.$scope.userGroups.remove(ug, function (e) {
                        return ug.groupName == e.groupName && ug.userName == e.userName;
                    });
                });
            };
            this._beerpongHub.client.confirmResults = function (gameResult) {
                var that = _this;
                that.$scope.$apply(function () {
                    that.$location.path('/stats');
                    //that.$scope.message = angular.toJson(gameResult);
                    //show modal with countdown and navigate to stats
                });
            };
            $.connection.hub.start().done(this.signalrRStarted).fail(this.signlarRFailed);
        };
        SearchController.$inject = ['$scope', 'localStorageService', '$location'];
        return SearchController;
    })();
    Beerpong.SearchController = SearchController;
    angular.module('bp.core').controller('SearchController', Beerpong.SearchController);
    var User = (function () {
        function User() {
        }
        return User;
    })();
    Beerpong.User = User;
    var Group = (function () {
        function Group() {
        }
        return Group;
    })();
    Beerpong.Group = Group;
    var UserGroup = (function () {
        function UserGroup() {
        }
        return UserGroup;
    })();
    Beerpong.UserGroup = UserGroup;
    var GameResult = (function () {
        function GameResult() {
        }
        return GameResult;
    })();
    Beerpong.GameResult = GameResult;
    var Game = (function () {
        function Game() {
        }
        return Game;
    })();
    Beerpong.Game = Game;
})(Beerpong || (Beerpong = {}));
//# sourceMappingURL=searchcontroller.js.map