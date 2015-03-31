(function () {
    'use strict';
    angular
        .module('bp.core')
        .controller('homeController', HomeController);

    HomeController.$inject = ['UserSettingsFactory'];

    function HomeController(UserSettingsFactory) {
        var vm = this;
        vm.userName = '';

        function activate() {
            vm.userName = UserSettingsFactory.getUserName();
        };

        activate();
    };
})();