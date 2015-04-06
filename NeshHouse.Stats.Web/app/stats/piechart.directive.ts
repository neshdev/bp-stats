interface ID3Service {
    d3(): JQueryPromise<D3.Base>;
}

interface HTMLElement {
    innerWidth: any;
    innerHeight: any;
}

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
                    
                    var color = d3.scale.ordinal().range(['#dff0d8', '#f2dede', '#B3F2C9', '#528C18', '#C3F25C']);
                    var width = 300;
                    var height = 300;
                    var radius = Math.min(width, height) / 2;

                    var area = d3.select(iElement[0]).append('svg')
                        .attr('width', width)
                        .attr('height', height)
                        .attr('class', 'center-block');

                    scope.$watch('data', function (newVals, oldVals) {
                        return scope.render(newVals);
                    }, true);

                    scope.render = function (data) {
                        area.selectAll("*").remove();
                        
                        var dataset = data;

                        var pieGroup = area.append('g')
                                            .attr('transform', 'translate(' + (width / 2) + ',' + (height / 2) + ')');
                        var arc = d3.svg.arc()
                            .innerRadius(0)
                            .outerRadius(radius);

                        var arc = d3.svg.arc()
                            .outerRadius(radius);

                        var pie = d3.layout.pie()
                            .value(function (d) { return d.count; })
                        var arcs = pieGroup.selectAll('.arc')
                            .data(pie(dataset))
                            .enter()
                            .append('g')
                            .attr('class', 'arc');

                        arcs.append('path')
                            .attr('d', arc)
                            .attr('fill', function (d) {
                                return color(d.data.label);
                            });

                        arcs.append('text')
                            .attr('transform', function (d) {
                                    d.innerRadius = 0;
                                    d.outerRadius = radius;
                                return 'translate(' + arc.centroid(d) + ')';
                            })
                            .attr('text-anchor', 'middle')
                            .attr('font-size', '1em')
                            .text(function (d) { return d.data.label + " - "  + d.data.count; });

                    };
                });
            },
        }
    }
})();