
angular
    .module('bp.core')
    .service('beerPongFactory', BeerPongFactory);

BeerPongFactory.inject = ['$window', '$rootScope', '$cookies', 'localStorageService'];

function BeerPongFactory($window, $rootScope, $cookies, localStorageService) {
    var proxy = null;

    var init = function () {
        var connection = $window.jQuery.hubConnection();
        connection.logging = true;
        var authData = localStorageService.get('authorizationData');
        connection.qs = { 'Bearer': authData.token };
        this.proxy = connection.createHubProxy('beerpongHub');

        //server pushes messages to client
        this.proxy.on('disconnect', function () {
            connection.stop();
        });

        this.proxy.on('joinedLobby', function (user) {
            $rootScope.$emit("joinedLobby", user);
        });

        this.proxy.on('unjoinedLobby', function (user) {
            $rootScope.$emit("unjoinedLobby", user);
        });

        connection.start()
            .done(function () {
                console.log('Now connected, connection ID=' + connection.id);
            })
            .fail(function () {
                console.log('Could not Connect!');
            });
    };

    //client pushes message to server
    var joinLobby = function (roomName) {
        this.proxy.invoke('joinLobby', roomName);
    };

    //client pushes message to server
    var unjoinLobby = function (roomName) {
        this.proxy.invoke('unjoinLobby', roomName);
    };

    return {
        init: init,
        joinLobby: joinLobby,
        unjoinLobby: unjoinLobby,
    };
};