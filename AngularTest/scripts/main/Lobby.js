(function () {
    'use strict';
    angular
        .module('app', [])
        .controller('menuController', menuController)
        .controller('gameController', gameController);
    menuController.$inject = ['$scope', '$http'];
    function menuController($scope) {
        var hub = $.connection.menuHub;
        hub.client.refreshList = function (userList) {
            $scope.$apply(function () {
                $scope.users = userList;
            });
        };
        hub.client.recInvite = function (gameId) {
            alert(gameId);
        };
        $.connection.hub.start();
        $scope.loggedIn = false;
        $scope.login = function () {
            if ($scope.strUsername != null && $scope.strUsername != "") {
                hub.server.loginUser($scope.strUsername).done(function (result) {
                    $scope.id = result;
                });
                $scope.loggedIn = true;
                window.addEventListener('unload', function () {
                    hub.server.logoutUser($scope.id);
                });
            }
        };
        $scope.logOut = function () {
            hub.server.logoutUser($scope.id);
            $scope.loggedIn = false;
        };
        $scope.invite = function (id) {
            hub.server.inviteUser(id, 0);
        };
    }
    function gameController($scope) {
    }
})();
//# sourceMappingURL=Lobby.js.map