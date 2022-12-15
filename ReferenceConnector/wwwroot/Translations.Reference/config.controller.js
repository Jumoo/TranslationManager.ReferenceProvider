(function () {
    'use strict';

    function configController($scope) {

        var pvm = this;

        // if you need to go and get values from your service 
        // to show the user during config - do it in this controller.
    }

    angular.module('umbraco')
        .controller('referenceConfigController', configController);
})();