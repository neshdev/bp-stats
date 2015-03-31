(function () {
    'use strict';

    angular
        .module('bp.core')
        .controller('loginController', LoginController);

    LoginController.$inject = ['$scope', '$location', 'authService'];

    function LoginController($scope, $location, authService) {
        $scope.loginData = {
            userName: '',
            password: '',
        };

        $scope.message = '';

        $scope.login = function () {
            authService.login($scope.loginData).then(function () {
                $location.path('/home');
            },
            function (err) {
                $scope.message = err.error_description;
            });
        };
    };
})();