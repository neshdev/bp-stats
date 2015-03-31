/// <reference path="../../scripts/typings/underscore/underscore.d.ts" />

interface Array<T> {
    inArray(comparer: any): boolean;
    pushIfNotExist(element: any, comparer: any);
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


interface BeerpongClient {
    disconnect();
    joinedLobby(user: Game.UserGroup);
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
        joinLobby(roomName: string);
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
                    that.$scope.userGroups.pushIfNotExist(ug, function (e: UserGroup) {
                        return ug.groupName === e.groupName
                            && ug.team === e.team
                            && ug.userName === e.userName;
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
                vm._beerpongHub.server.joinLobby(vm.$scope.roomName, team).then(joinLobbySuccess, joinLobbyError);
            };

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