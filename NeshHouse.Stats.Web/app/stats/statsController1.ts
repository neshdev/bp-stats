interface ID3Service {
    d3(): JQueryPromise<D3.Base>;
}

interface HTMLElement {
    innerWidth: any;
    innerHeight: any;
}

(function () {
    angular
        .module("bp.core")
        .controller("StatsController", StatsController)
        .directive("barChart", barChart);

    StatsController.$inject = ['$scope'];

    function StatsController($scope) {
        $scope.d3Data = [
            { name: "Greg", score: 98 },
            { name: "Ari", score: 96 },
            { name: "Loser", score: 48 }
        ];
    };

    barChart.$inject = ['d3Service'];

    function barChart(d3Service: ID3Service) {
        return {
            restrict: "EA",
            scope: {
                data : "="

            },
            link: function (scope, iElement, iAttrs) {
                d3Service.d3().then(function (d3) {

                    var svg = d3.select(iElement[0])
                        .append("svg")
                        .attr("width", "100%");

                    window.onresize = function () {
                        return scope.$apply();
                    };

                    scope.$watch(
                        function () {
                            return angular.element(window)[0].innerWidth;
                        }
                        , function () {
                            return scope.render(scope.data);
                        });

                    // watch for data changes and re-render
                    scope.$watch('data', function (newVals, oldVals) {
                        return scope.render(newVals);
                    }, true);

                    scope.render = function (data) {
                        // remove all previous items before render
                        svg.selectAll("*").remove();

                        // setup variables
                        var width, height, max;
                        width = d3.select(iElement[0])[0][0].offsetWidth - 20;
                        // 20 is for margins and can be changed
                        height = scope.data.length * 35;
                        // 35 = 30(bar height) + 5(margin between bars)
                        max = 98;
                        // this can also be found dynamically when the data is not static
                        // max = Math.max.apply(Math, _.map(data, ((val)-> val.count)))

                        // set the height based on the calculations above
                        svg.attr('height', height);

                        //create the rectangles for the bar chart
                        svg.selectAll("rect")
                            .data(data)
                            .enter()
                            .append("rect")
                            .on("click", function (d, i) { return scope.onClick({ item: d }); })
                            .attr("height", 30) // height of each bar
                            .attr("width", 0) // initial width of 0 for transition
                            .attr("x", 10) // half of the 20 side margin specified above
                            .attr("y", function (d, i) {
                            return i * 35;
                        }) // height + margin between bars
                            .transition()
                            .duration(1000) // time of duration
                            .attr("width", function (d) {
                            return d.score / (max / width);
                        }); // width based on scale

                        svg.selectAll("text")
                            .data(data)
                            .enter()
                            .append("text")
                            .attr("fill", "#fff")
                            .attr("y", function (d, i) { return i * 35 + 22; })
                            .attr("x", 15)
                            .text(function (d) { return d[scope.label]; });

                    };

                });
            }
        };
    };

})();