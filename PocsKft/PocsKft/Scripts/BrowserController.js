
NGMAIN.controller("BrowserController", ["$scope", "$rootScope", "Communicator", "GlobalService",
    function ($scope, $rootScope, Communicator, GlobalService) {
        $scope.reload = function () {
            $scope.currentPath = window.location.pathname || "/";
            GlobalService.currentPath = $scope.currentPath;
            $rootScope.$broadcast("pathChanged", $scope.currentPath);
            window.document.title = decodeURI($scope.currentPath);
            Communicator.listFolder($scope.currentPath).then(function (data) {
                $scope.files = data.map(function (e) {
                    if (e.isProject) return new Project(e); else return new File(e);
                });
                $scope.clearSelections();
                NGMAIN.setLoading(false);
            }, function (errorData) {
                alert("Error during listing the folder:\n" + JSON.stringify(errorData));
            });
        };

        $scope.$on("searchDataArrived", function (event,data) {
            $scope.files = data.map(function (e) {
                if (e.isProject) return new Project(e); else return new File(e);
                $scope.clearSelections();
            });
            NGMAIN.setLoading(false);
        });

        $scope.$on("fileStructureChanged", function () {
            $scope.reload();
        });

        $scope.select = function (selected, event) {
            if (selected.selected && !selected.isRealFile) {
                delete GlobalService.selectedFile;
                $scope.currentPath = selected.filePath + (selected.fileName || selected.projectName) + "/";
                window.history.pushState($scope.currentPath, null, $scope.currentPath);
                selected.selected = false;
                $scope.reload();
            } else {
                $scope.clearSelections();
                selected.selected = true;
                GlobalService.selectedFile = selected;
                $rootScope.$broadcast("fileSelectionChanged", selected);
                setTimeout(window.resize, 500);
            }
            return true;
        };

        $scope.clearSelections = function (event) {
            if (event) { event.bubbles = true; event.handled = false; }
            for (var i in $scope.files) { $scope.files[i].selected = false; }
            delete GlobalService.selectedFile;
            $rootScope.$broadcast("fileSelectionChanged", null);
        };

        $(window).on('popstate', function (e) {
            $scope.currentPath = e.originalEvent.state;
            $scope.$apply(function () { $scope.reload(); });
            window.resize();
        });

        $scope.reload();
    }]);