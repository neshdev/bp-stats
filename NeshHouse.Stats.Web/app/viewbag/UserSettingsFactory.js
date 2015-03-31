(function () {
    'use strict';

    angular
        .module('bp.core')
        .factory('UserSettingsFactory', UserSettingsFactory);

    UserSettingsFactory.$inject = ['localStorageService'];

    function UserSettingsFactory(localStorageService) {
        var factory = this;
        
        function getUserName(){
            var token = localStorageService.get('authorizationData');
            return token.userName || 'anon';
        };

        return {
            getUserName: getUserName
        };
    };
})();