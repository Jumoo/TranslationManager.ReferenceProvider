(function () {
    'use strict';

    function pendingController($scope) {

        var pvm = this;

        // pending is show when a user is creating a new Translation job.
        // here you set things like descriptions, or any other job options

        // if you need to fetch settings etc you do that here.

        // example - deadline date picker

        pvm.dateEditor = {
            editor: "Umbraco.DateTime",
            label: "Deadline",
            description: "Suggested deadline for translation",
            hideLabel: false,
            view: "datepicker",
            alias: "deadline",
            value: getDefaultDeadline(14),
            config: {
                format: "YYYY-MM-DD HH:mm:ss",
                pickDate: true,
                pickTime: true,
                useSeconds: true
            }
        };

        $scope.$watch("pvm.dateEditor.value", function (newValue, oldValue) {
            if (newValue !== undefined && newValue.length !== 0
                && $scope.vm.job !== undefined) {
                $scope.vm.job.providerOptions.deadline = newValue;
            }
        });

        function getDefaultDeadline(days) {
            var deadline = new Date();
            deadline.setDate(deadline.getDate() + days * 1);
            var stringDate = deadline.toISOString().slice(0, 10)
                + " " + deadline.getHours() + ":" + deadline.getMinutes() + ":" + deadline.getSeconds();

            $scope.vm.job.providerOptions.deadline = stringDate;

            return stringDate;
        }

    }

    angular.module('umbraco')
        .controller('referencePendingController', pendingController);
})();