(function () {
    'use strict';

    angular
        .module('bp.core')
        .controller('indexController', IndexController);

    IndexController.$inject = ['$scope', '$location', 'authService'];

    function IndexController($scope, $location, authService) {
        $scope.logOut = function () {
            authService.logOut();
            $location.path('/rankings');
        };

        $scope.authentication = authService.authentication;
    };

})();