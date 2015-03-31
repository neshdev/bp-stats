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
var Game;
(function (Game) {
    var SearchController = (function () {
        function SearchController($scope, localStorageService) {
            this.$scope = $scope;
            this.localStorageService = localStorageService;
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
                console.log('Error joining lobby :' + err);
            };
            $scope.joinLobby = function (team) {
                vm._beerpongHub.server.joinLobby(vm.$scope.roomName, team).then(joinLobbySuccess, joinLobbyError);
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
                    that.$scope.userGroups.pushIfNotExist(ug, function (e) {
                        return ug.groupName === e.groupName && ug.team === e.team && ug.userName === e.userName;
                    });
                });
            };
            $.connection.hub.start().done(this.signalrRStarted).fail(this.signlarRFailed);
        };
        SearchController.$inject = ['$scope', 'localStorageService'];
        return SearchController;
    })();
    Game.SearchController = SearchController;
    angular.module('bp.core').controller('SearchController', Game.SearchController);
    var User = (function () {
        function User() {
        }
        return User;
    })();
    Game.User = User;
    var Group = (function () {
        function Group() {
        }
        return Group;
    })();
    Game.Group = Group;
    var UserGroup = (function () {
        function UserGroup() {
        }
        return UserGroup;
    })();
    Game.UserGroup = UserGroup;
})(Game || (Game = {}));
//# sourceMappingURL=searchController.js.map