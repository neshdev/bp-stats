
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

        //document.cookie = "BearerToken=" + authData.token + "; path=/";
        //$cookies.put('BearerToken', 'Bearer ' + authData.token)

        //working
        //$window.jQuery.ajaxSetup({
        //    beforeSend: function (xhr) {
        //        xhr.setRequestHeader('Authorization', 'Bearer ' + authData.token);
        //    }
        //});

        this.proxy = connection.createHubProxy('beerpongHub');

        //server pushes messages to client
        this.proxy.on('userChanged', function (data) {
            $rootScope.$emit("userChanged", data);
        });

        //working
        //connection.start({ transport: 'longPolling' })
        connection.start()
            .done(function () {
                console.log('Now connected, connection ID=' + connection.id);
            })
            .fail(function () {
                console.log('Could not Connect!');
            });
    };

    //client pushes message to server
    var addToRoom = function (roomName) {
        this.proxy.invoke('addToRoom', roomName);
    };

    //client pushes message to server
    var removeFromRoom = function (roomName) {
        this.proxy.invoke('RemoveFromRoom', roomName);
    };



    return {
        init: init,
        addToRoom: addToRoom,
        removeFromRoom: removeFromRoom,
    };


};