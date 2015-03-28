(function ($, window, undefined) {
    'use strict';

    angular.module('bp.core').service('bpService', bpService);

    function bpService() {

        var bpProxy = $.connection.beerPong;


        $.connection.hub.start()
            .done(function () {
                console.log('Connected');
            })
            .fail(function () {
                console.log('Failed to connect');
            });


        return {
            bpProxy: bpProxy,
        };
    };


}(window.jQuery, window));


