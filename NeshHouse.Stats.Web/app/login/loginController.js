angular.module('bp.core').controller('loginController', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    $scope.loginData = {
        username: '',
        password : '',
    };
    
    $scope.message = '';

    $scope.login = function () {
        authService.login($scope.loginData).then(function () {
            $location.path('/rankings');
        },
        function (err) {
            $scope.message = err.error_description;
        });
    };
}])