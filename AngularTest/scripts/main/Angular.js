/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/signalr/signalr.d.ts" />
/// <reference path="../typings/angularjs/angular.d.ts" />
var Game //Not currently used in code but left here to show what the object contains.
 = (function () {
    function Game //Not currently used in code but left here to show what the object contains.
        () {
        this.game = {};
    }
    return Game //Not currently used in code but left here to show what the object contains.
    ;
}());
(function () {
    'use strict';
    angular
        .module('app', [])
        .controller('menuController', menuController)
        .controller('gameController', gameController)
        .factory('comService', comService)
        .filter('filterNotOwnId', function () {
        return function (items, scope) {
            var filtered = [];
            angular.forEach(items, function (item) {
                if (item.id != scope.id) {
                    filtered.push(item);
                }
            });
            return filtered;
        };
    });
    menuController.$inject = ['$scope', 'comService'];
    gameController.$inject = ['$scope', 'comService'];
    function menuController($scope, comService) {
        var hub = $.connection.menuHub;
        hub.client.refreshList = function (userList) {
            $scope.$apply(function () {
                $scope.users = userList;
            });
        };
        hub.client.recInvite = function (gameId, message) {
            $scope.invGameId = gameId;
            $("#invite-message").text(message);
            $("#invite-dialog").dialog("open");
        };
        hub.client.declinedInvite = function (message) {
            alert(message);
        };
        hub.client.latestMatch = function (message) {
            $scope.$apply(function () {
                $scope.historyObj = angular.fromJson(message);
            });
        };
        $.connection.hub.start();
        $scope.$watch(function () { return comService.gameId; }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.gameId = comService.gameId;
            }
        });
        $scope.$watch(function () { return comService.loggedIn; }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.loggedIn = comService.loggedIn;
            }
        });
        $scope.login = function () {
            if ($scope.strUsername != null && $scope.strUsername != "") {
                hub.server.loginUser($scope.strUsername).done(function (result) {
                    comService.userId = result;
                    $scope.id = result;
                });
                comService.loggedIn = true;
                //Remove user if leaving page. Also runs on "reload" so postback has to be avoided.
                window.addEventListener('beforeunload', function () {
                    $scope.logOut();
                });
                if (!$scope.$$phase) {
                    $scope.$digest(); //Fixes a wierd bug where comService.userId still has no value;
                }
            }
        };
        $scope.logOut = function () {
            hub.server.logoutUser($scope.id, $scope.gameId);
            comService.loggedIn = false;
        };
        $scope.invite = function (userId) {
            hub.server.inviteUser(userId, $scope.gameId);
        };
        function acceptInvite() {
            if ($scope.invGameId != -1) {
                var gameHub = $.connection.gameHub;
                $scope.$apply(function () {
                    $scope.gameId = $scope.invGameId;
                });
                comService.gameId = $scope.invGameId;
                gameHub.server.acceptInvite($scope.id, $scope.invGameId, $scope.gameId);
            }
        }
        function declineInvite() {
            if ($scope.invGameId != -1) {
                hub.server.declineInvite($scope.id, $scope.invGameId);
            }
        }
        $("#invite-dialog").dialog({
            modal: false,
            draggable: false,
            resizable: false,
            autoOpen: false,
            height: 200,
            width: 400,
            title: "Invite received",
            buttons: {
                "Accept": function () {
                    acceptInvite();
                    $('#invite-dialog').dialog('close');
                    $('#invite-message').text('');
                },
                "Decline": function () {
                    declineInvite();
                    $('#invite-dialog').dialog('close');
                    $('#invite-message').text('');
                }
            }
        });
    }
    function gameController($scope, comService) {
        var hub = $.connection.gameHub;
        $.connection.hub.start();
        hub.client.updateBoard = function (game) {
            if (!$scope.$$phase) {
                $scope.$apply(function () {
                    $scope.game = angular.fromJson(game); //Update the game info.
                    $scope.gameId = $scope.game.gameId;
                });
            }
            else {
                $scope.game = angular.fromJson(game); //Update the game info.
                $scope.gameId = $scope.game.gameId;
            }
        };
        hub.client.matchDone = function (message) {
            if (confirm(message)) {
                clearBoard($scope, comService);
            }
            else {
                clearBoard($scope, comService);
            }
            var menuHub = $.connection.menuHub;
            menuHub.server.viewLatestMatch();
        };
        $scope.gameId = -1;
        $scope.$watch(function () { return comService.userId; }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.id = comService.userId;
            }
        });
        $scope.$watch(function () { return comService.loggedIn; }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.loggedIn = comService.loggedIn;
            }
        });
        $scope.startGame = function () {
            if ($scope.id == -1) {
                if (!$scope.$$phase) {
                    $scope.$digest();
                }
                else {
                    alert("Something went wrong, please relog");
                    return;
                }
            }
            hub.server.startGame($scope.id).done(function (result) {
                comService.gameId = result;
                $scope.$apply(function () {
                    $scope.gameId = result;
                });
            });
        };
        $scope.playerClick = function (tileNumber) {
            if (verifyTurn($scope)) {
                hub.server.makeMove($scope.id, $scope.gameId, tileNumber);
            }
        };
    }
    function clearBoard($scope, comService) {
        $scope.$apply(function () {
            $scope.game = null;
            $scope.gameId = -1;
            comService.gameId = -1;
        });
    }
    function verifyTurn($scope) {
        if ($scope.game != undefined) {
            if ($scope.game.currentPlayer == 1 && $scope.id == $scope.game.playerOne.id) {
                return true;
            }
            else if ($scope.game.currentPlayer == 2 && $scope.id == $scope.game.playerTwo.id) {
                return true;
            }
        }
        return false;
    }
    function comService() {
        var sharedInfo = {};
        sharedInfo.userId = -1;
        sharedInfo.loggedIn = false;
        sharedInfo.gameId = -1;
        return sharedInfo;
    }
})();
