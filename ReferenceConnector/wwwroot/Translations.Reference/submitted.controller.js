(function () {
    'use strict';

    function submittedController($scope) {

        var pvm = this;
        pvm.job = $scope.vm.job;
        pvm.properties = angular.fromJson(pvm.job.providerProperties);

        // submitted is shown at the top of a job in the submitted (and received)
        // views. it shows the user any additional information you may want to show
        // about a job submitted to your connector. 

    }

    angular.module('umbraco')
        .controller('referenceSubmittedController', submittedController);
})();