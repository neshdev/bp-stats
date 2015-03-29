(function () {
    'use strict';

    angular
        .module('bp.core')
        .service('userService', UserService);

    UserService.$inject = ['localStorageService'];

    function UserService(localStorageService) {
        var service = this;
        var auth = localStorageService.get('authorizationData');

        service.userName = auth.userName;
    };
})();