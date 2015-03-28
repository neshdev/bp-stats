(function() {
    'use strict';
    angular
        .module('bp.core')
        .controller('searchController', SearchController);

    SearchController.$inject = ['$scope', 'beerPongFactory'];

    function SearchController($scope, beerPongFactory) {
        $scope.lobbyName = "";

        $scope.joinLobby = function () {
            beerPongFactory.addToRoom($scope.lobbyName);
        };

        beerPongFactory.init();
    };

})();