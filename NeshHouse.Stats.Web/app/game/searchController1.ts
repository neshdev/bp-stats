interface BeerpongClient {
    disconnect();
    joinedLobby(user: User);
    unjoinedLobby(user: User);
}

interface BeerpongServer {
    joinLobby(roomName: string) : () => JQueryPromise<void>;
    unjoinLobby(roomName: string) : () => JQueryPromise<void>;
}

interface BeerpongHubProxy {
    client: BeerpongClient;
    server: BeerpongServer;
}

interface SignalR {
    beerpongHub: BeerpongHubProxy;
}

class User {
    public Name: string;
}




    angular.module('bp.core')
        .factory('SignalRFactory', SignalRFactory);

    interface ISignalRFactory {
        disconnect();
        joinedLobby(user: User);
        unjoinedLobby(user: User);
        joinLobby(name: string);
        unjoinLobby(name : string);
        init();
    }

    class SignalRFactory implements ISignalRFactory {
        private _beerpongHub: BeerpongHubProxy;

        static $inject = ['$window', '$rootScope', '$location', '$log', 'localStorageService'];

        constructor(
             private $window : ng.IWindowService
            ,private $rootScope: ng.IRootScopeService
            ,private $location: ng.ILocationService
            ,private $log : ng.ILogService
            ,private localStorageService: any) {
            var that = this;
            this._beerpongHub = $.connection.beerpongHub;
            $.connection.hub.logging = true;
            var authData = localStorageService.get('authorizationData');
            $.connection.hub.qs = { 'Bearer': authData.token };
            
            this._beerpongHub.client.disconnect = () => this.disconnect();
            this._beerpongHub.client.joinedLobby = (user: User) => this.joinedLobby(user);
            this._beerpongHub.client.unjoinedLobby = (user: User) => this.unjoinedLobby(user);
        }

        public init() {
            $.connection.hub.start().done(this.signalrRStarted).fail(this.signlarRFailed);            
        }

        private signalrRStarted() {
            this.$log.log("Now connected, connection ID=" + $.connection.id);
        }

        private signlarRFailed() {
            this.$log.log("Could not connect!");
        }

        public disconnect() {
            $.connection.hub.stop();
        }

        public joinedLobby(user: User) {
            this.$rootScope.$emit("joinedLobby", user);
        }

        public unjoinedLobby(user: User) {
            this.$rootScope.$emit("unjoinedLobby", user);
        }

        public joinLobby(roomName: string) {
            this._beerpongHub.server.joinLobby(roomName);
        }

        public unjoinLobby(roomName: string) {
            this._beerpongHub.server.unjoinLobby(roomName);
        }
    }

    interface ISearchControllerScope extends ng.IScope {
        roomName: string;
        joinLobby();
    }

    class SearchController {

        static $inject = ['$scope', '$location', 'SignalRFactory'];

        constructor(
             private $scope: ISearchControllerScope
            ,private $location: ng.ILocationService
            ,private SignalRFactory: ISignalRFactory) {
            
            var that = this;
            this.activate();
        }

        public joinLobby() {
            this.SignalRFactory.joinLobby(this.$scope.roomName);
            this.$location.path("/ActiveGame");
        }

        private activate() {
            this.SignalRFactory.init();
        }
        
    }

    interface ActiveGameScope extends ng.IScope {
        users: User[];
    }

    class ActiveGameController {
        static $inject = ['$scope', 'SignalRFactory'];

        constructor(
             private $scope: ActiveGameScope
            ,private SignalRFactory : ISignalRFactory
            ) {
            var that = this;

            $scope.$parent.$on("joinedLobby", function (user : any) {
                that.$scope.users.push(user);
            });

            $scope.$parent.$on("unjoinLobby", function (user: any) {
                that.$scope.users.splice(that.$scope.users.indexOf(user), 1);
            });
        }

        private leaveRoom() {
            this.SignalRFactory.unjoinLobby;
        }
    }
