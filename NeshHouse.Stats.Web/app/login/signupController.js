(function () {
    'use strict';

    angular
        .module('bp.core')
        .controller('signupController', SignupController)

    SignupController.$inject = ['$scope', '$location', '$timeout', 'authService'];

    function SignupController($scope, $location, $timeout, authService) {
        $scope.registration = {
            userName: '',
            password : '',
            confirmPassword: '',
        }

        $scope.message = '';
        $scope.savedSuccessfully = false;

        $scope.signUp = function () {
            var onSuccess = function (response) {
                $scope.savedSuccessfully = true;
                $scope.message = "User has been registered successfully, you will be redicted to login page in 2 seconds.";
                startTimer();
            }

            var onError = function (response) {
                var errors = [];
                for (var key in response.data.modelState) {
                    for (var i = 0; i < response.data.modelState[key].length; i++) {
                        errors.push(response.data.modelState[key][i]);
                    }
                }
                $scope.message = "Failed to register user due to:" + errors.join(' ');
            }

            authService
                .saveRegistration($scope.registration).then(onSuccess, onError);
        };

        var startTimer = function () {
            var timer = $timeout(function () {
                $timeout.cancel(timer);
                $location.path('/login');
            }, 2000);
        };
    };
})();