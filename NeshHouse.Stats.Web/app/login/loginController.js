(function () {
    'use strict';

    angular
        .module('bp.core')
        .controller('loginController', LoginController);

    LoginController.$inject = ['$scope', '$location', 'authService', 'userService'];

    function LoginController($scope, $location, authService, userService) {
        $scope.loginData = {
            userName: '',
            password: '',
        };

        $scope.message = '';

        $scope.login = function () {
            authService.login($scope.loginData).then(function () {
                userService.userName = $scope.loginData.userName;
                $location.path('/home');
            },
            function (err) {
                $scope.message = err.error_description;
            });
        };
    };
})();