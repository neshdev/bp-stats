(function () {
    'use strict';

    angular.module('bp.core')
    .controller('roomController', function () {
        var vm = this;
        vm.roomName = 'test';
        vm.players =
        [
            'Victor',
            'Nesh'
        ];
    });
})();
