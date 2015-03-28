(function () {
    'use strict';

    angular.module('bp.core').controller('statsController', function () {
        vm = this;
        vm.currentStats =
            {
                "position": 1,
                "alias": "neshiro",
                "wins": 10,
                "losses": 10,
            };
        vm.statsVsOthers =
            [
                {
                    "hero": "nesh",
                    "heroWins": 1,
                    "villan": "victor",
                    "villanWins": 1
                },
                {
                    "hero": "nesh",
                    "heroWins": 1,
                    "villan": "victor",
                    "villanWins": 1
                },
                {
                    "hero": "nesh",
                    "heroWins": 1,
                    "villan": "victor",
                    "villanWins": 1
                },
            ];
    });
})();