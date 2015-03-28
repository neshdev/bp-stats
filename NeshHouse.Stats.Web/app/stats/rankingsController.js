angular.module('bp.core')
    .controller('rankingsController', ['$scope', 'rankingsService', function ($scope, rankingsService) {

        $scope.rankings = [];
        $scope.message = '';

        rankingsService.getRankings().then(function (results) {

            $scope.rankings = results.data;

        }, function (error) {
            //alert(error.data.message);
            message = error.error_description;
        });
    }]);