(function () {
    'use strict';

    angular
        .module('bp.core')
        .controller('indexController', IndexController);

    IndexController.$inject = ['$scope', 'authService'];

    function IndexController($scope, authService) {
        $scope.logOut = function () {
            authService.logOut();
            $location.path('/rankings');
        };

        $scope.authentication = authService.authentication;
    };

})();