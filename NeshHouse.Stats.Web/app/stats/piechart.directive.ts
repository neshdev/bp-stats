interface IPieChartScope extends ng.IScope {
    data: any;
    render(data);
};

(function () {
    'use strict';

    angular.module('bp.core')
        .directive("pieChart", PieChart);

    PieChart.$inject = ['d3Service'];

    function PieChart(d3Service) {
        return {
            restrict: "EA",
            scope: {
                data: "="
            },
            link: function (scope: IPieChartScope, iElement: ng.IAugmentedJQuery, iAttrs: ng.IAttributes) {
                d3Service.d3().then(function (d3: D3.Base) {
                    
                    var width = 360;
                    var height = 360;
                    var radius = Math.min(width, height) / 2;

                    var color = d3.scale.category20b();

                    var svg = d3.select(iElement[0])
                        .append("svg")
                        .attr("class", "center-block")
                        .attr("width", width)
                        .attr("height", height)
                        .append('g')
                        .attr('transform', 'translate(' + (width / 2) + "," + (height / 2) + ')');
                        
                    scope.$watch(
                        function () {
                            return angular.element(window)[0].innerWidth;
                        }
                        , function () {
                            return scope.render(scope.data);
                        });

                    scope.$watch('data', function (newVals, oldVals) {
                        return scope.render(newVals);
                    }, true);

                    scope.render = function (data) {
                        //svg.selectAll("*").remove();
                        
                        var arc = d3.svg.arc()
                            .outerRadius(radius);

                        var pie = d3.layout.pie()
                            .value(function (d) {
                                return d.count;
                            })
                            .sort(null);

                        var path = svg.selectAll('path')
                            .data(pie(data))
                            .enter()
                            .append('path')
                            .attr('d', arc)
                            .attr('fill', function (d, i) {
                                return color(d.data.label);
                        });

                    };
                });
            },
        }
    }
})();