
HBMAIN.controller("PropertiesController", ["$scope", "$rootScope", "Communicator", "GlobalService",
    function ($scope, $rootScope, Communicator, GlobalService) {
        $scope.global = GlobalService;

        $scope.$on("fileSelectionChanged", function (event) {
        });

        $scope.addNewProperty = function () {
            var file = $scope.global.selectedFile;
            if (file.properties && file.properties[file.properties.length - 1]) {
                if (file.properties[file.properties.length - 1].propName == file.properties[file.properties.length - 1].propValue && file.properties[file.properties.length - 1].propValue === '') {
                    return;
                }
            }
            file.properties.push(new Property());
            setTimeout(function () {
                $(".edit").each(function (i, e) {
                    if ($(e).css("display") === 'none')
                        $(e).slideDown(200);
                });
            }, 300);
        };

        $scope.save = function () {
            GlobalService.ACTIONS.SAVE.execute();
        };

        $scope.discard = function () {
            $rootScope.$broadcast("fileSelectionChanged");
        };
    }]);
