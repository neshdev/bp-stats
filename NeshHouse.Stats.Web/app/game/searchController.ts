/// <reference path="../../scripts/typings/underscore/underscore.d.ts" />

interface Array<T> {
    inArray(comparer: any): boolean;
    pushIfNotExist(element: any, comparer: any);
    remove(element: any, comparer: any);
}

Array.prototype.inArray = function (comparer) {
    for (var i = 0; i < this.length; i++) {
        if (comparer(this[i])) return true;
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

Array.prototype.remove = function(element , comparer) {
    for (var i = 0; i < this.length; i++) {
        if (comparer(this[i])) {
            this.splice(i, 1);
        }
    }
}


interface BeerpongClient {
    disconnect();
    joinedLobby(user: Game.UserGroup);
    unjoinedLobby(user: Game.UserGroup);
}

interface BeerpongServer {
    joinLobby(group: string, team: string): JQueryPromise<Game.UserGroup[]>;
    unjoinLobby(roomName: string): JQueryPromise<void>;
}

interface BeerpongHubProxy {
    client: BeerpongClient;
    server: BeerpongServer;
}

interface SignalR {
    beerpongHub: BeerpongHubProxy;
}

module Game {

    interface ISearchControllerScope extends ng.IScope {
        roomName: string;
        joinLobby(team: string);
        leaveLobby();
        userGroups: UserGroup[];
        gameFound: boolean;
        message: string;
    }

    export class SearchController {
        private _beerpongHub: BeerpongHubProxy;
        static $inject = ['$scope', 'localStorageService'];

        private signalrRStarted() {
            console.log("Now connected, connection ID=" + $.connection.hub.id);
        }

        private signlarRFailed() {
            console.log("Could not connect!");
        }

        private activate() {
            this._beerpongHub = $.connection.beerpongHub;
            $.connection.hub.logging = true;
            var authData = this.localStorageService.get('authorizationData');
            $.connection.hub.qs = { 'Bearer': authData.token };

            this._beerpongHub.client.disconnect = () => {
                $.connection.hub.stop();
            };

            this._beerpongHub.client.joinedLobby = (ug: UserGroup) => {
                var that = this;

                that.$scope.$apply(function () {

                    //remove user without looking at team
                    that.$scope.userGroups.remove(ug, function (e: UserGroup) {
                        return ug.groupName == e.groupName && ug.userName == e.userName;
                    });

                    //add user to team
                    that.$scope.userGroups.pushIfNotExist(ug, function (e: UserGroup) {
                        return ug.groupName === e.groupName
                            && ug.team === e.team
                            && ug.userName === e.userName;
                    });
                });
            };

            this._beerpongHub.client.unjoinedLobby = (ug: UserGroup) => {
                var that = this;

                that.$scope.$apply(function () {
                    //remove user without looking at team
                    that.$scope.userGroups.remove(ug, function (e: UserGroup) {
                        return ug.groupName == e.groupName && ug.userName == e.userName;
                    });
                });
            };

            $.connection.hub.start().done(this.signalrRStarted).fail(this.signlarRFailed);
        }

        constructor(private $scope: ISearchControllerScope, private localStorageService: any) {

            var vm = this;
            $scope.userGroups = [];
            $scope.gameFound = false;
            $scope.roomName = '';
            $scope.message = '';

            var joinLobbySuccess = function (data: UserGroup[]) {

                vm.$scope.$apply(function () {
                    for (var i = 0; i < data.length; i++) {
                        vm.$scope.userGroups.pushIfNotExist(data[i], function (e: UserGroup) {
                            return data[i].groupName === e.groupName
                                && data[i].team === e.team
                                && data[i].userName === e.userName;
                        });
                    };

                    vm.$scope.gameFound = true;
                });
            };

            var joinLobbyError = function (err) {
                console.log('Error joining lobby :' + err);
            };

            $scope.joinLobby = (team: string) => {
                if (vm.$scope.roomName) {
                    vm._beerpongHub.server.joinLobby(vm.$scope.roomName, team).then(joinLobbySuccess, joinLobbyError);
                }
                else {
                    vm.$scope.message = 'Error: Please enter room name';
                }
            };

            $scope.leaveLobby = () => {
                if (vm.$scope.roomName) {
                    vm._beerpongHub.server.unjoinLobby(vm.$scope.roomName);
                    vm.$scope.userGroups = [];
                    vm.$scope.gameFound = false;
                    vm.$scope.roomName = '';
                    vm.$scope.message = '';
                }
            }

            $scope.$on('$destroy', function () {
                $.connection.hub.stop();
            });

            vm.activate();
        }
    }

    angular.module('bp.core')
        .controller('SearchController', Game.SearchController);

    export class User {
        public name: string;
        public userGroups: UserGroup[];
    }

    export class Group {
        public name: string;
        public userGroups: UserGroup[];
    }

    export class UserGroup {
        public groupName: string;
        public userName: string;
        public team: string;
    }
}