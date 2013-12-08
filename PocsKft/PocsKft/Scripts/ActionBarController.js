/**
* This controller is responsible for providing backing data for the action bar
* The main case is when the file selection changes, triggering 'fileSelectionChanged' on $rootScope.
* Different actions for different selections
**/
HBMAIN.controller("ActionBarController",
    ["$scope", "Communicator", "GlobalService",
    function ($scope, Communicator, GlobalService) {
        $scope.global = GlobalService;

        ACTIONS = GlobalService.ACTIONS;

        $scope.$on("fileSelectionChanged", function () {
            if (GlobalService.selectedFile) {
                if (GlobalService.selectedFile.isRealFile) {
                    setFileActions(GlobalService.selectedFile);
                } else if (GlobalService.selectedFile.hasOwnProperty("isRealFile")) {
                    setFolderActions(GlobalService.selectedFile);
                } else {
                    setProjectActions(GlobalService.selectedFile);
                }
            } else {
                if (GlobalService.currentPath === '/') {
                    setNoTargetActionsProject();
                } else {
                    setNoTargetActions();
                }
            }
        });

        setFolderActions = function (file) {
            $scope.actions = [];
            if (file.rights === 'WRITE') $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForFolder());
            $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForFolder());
        };

        setProjectActions = function (file) {
            $scope.actions = [];
            if (file.rights === 'WRITE') $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForProject());
            $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForProject());
        };

        setFileActions = function (file) {
            $scope.actions = [];
            if (file.rights === 'WRITE') $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsForFile(file));
            $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsForFile(file));
        };

        setNoTargetActions = function () {
            $scope.actions = [];
            if (true) $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsInFolder());
            $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsInFolder());
        };

        setNoTargetActionsProject = function () {
            $scope.actions = [];
            if (true) $scope.actions = $scope.actions.concat(ACTIONS.getWriteActionsInLobby());
            $scope.actions = $scope.actions.concat(ACTIONS.getReadActionsInLobby());
        };

        $scope.execute = function (action) {
            action.execute(GlobalService.selectedFile);
        };
    }]);