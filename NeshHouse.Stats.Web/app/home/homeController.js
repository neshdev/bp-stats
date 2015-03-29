(function () {
    'use strict';
    angular
        .module('bp.core')
        .controller('homeController', HomeController);

    HomeController.$inject = ['$scope','userService'];

    function HomeController($scope, userService) {
        $scope.userName = userService.userName;
    };
})();