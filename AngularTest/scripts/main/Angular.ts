/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/signalr/signalr.d.ts" />
/// <reference path="../typings/angularjs/angular.d.ts" />
declare var angular: ng.IAngularStatic;

interface SignalR {
    menuHub: any;
    gameHub: any;
} 
interface JQuery {
    dialog:any;
}
class Game //Not currently used in code but left here to show what the object contains.
{
    game = {};
    gameId: Number;
    tiles: Number[];
    currentPlayer: Number;
    playerOne: any; //"PlayerOne":{"id":0,"username":"Hiplobbe","game":0}
    playerTwo: any;
}

(function () {
    'use strict';

    angular
        .module('app', [])
        .controller('menuController', menuController)
        .controller('gameController', gameController)
        .factory('comService', comService)
        .filter('filterNotOwnId', function () {
            return function (items, scope) { //To not view oneself in player list.
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

    function menuController($scope,comService) {
        var hub = $.connection.menuHub;

        hub.client.refreshList = function (userList) {
            $scope.$apply(function () { //To update page.
                $scope.users = userList;
            });
        }

        hub.client.recInvite = function (gameId : number,message : string) {
            $scope.invGameId = gameId;

            $("#invite-message").text(message);
            $("#invite-dialog").dialog("open");
        }

        hub.client.declinedInvite = function (message: string) {
            alert(message);
        }

        hub.client.latestMatch = function (message: string) {
            $scope.$apply(function() {
                $scope.historyObj = angular.fromJson(message);
            });
        }

        $.connection.hub.start();

        $scope.$watch(function () { return comService.gameId }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.gameId = comService.gameId;
            }
        });

        $scope.$watch(function () { return comService.loggedIn }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.loggedIn = comService.loggedIn;
            }
        });

        $scope.login = function () {
            if ($scope.strUsername != null && $scope.strUsername != "") {
                hub.server.loginUser($scope.strUsername).done(function(result) {
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
        $scope.logOut = function() {
            hub.server.logoutUser($scope.id,$scope.gameId);
            comService.loggedIn = false;
        };
        $scope.invite = function (userId : Number) {
            hub.server.inviteUser(userId,$scope.gameId);
        }

        function acceptInvite() {
            if ($scope.invGameId != -1) {
                var gameHub = $.connection.gameHub;

                $scope.$apply(function() {
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

    function gameController($scope,comService) {
        var hub = $.connection.gameHub;
        $.connection.hub.start();

        hub.client.updateBoard = function (game : string) {
            if (!$scope.$$phase) {
                $scope.$apply(function() {
                    $scope.game = angular.fromJson(game); //Update the game info.
                    $scope.gameId = $scope.game.gameId;
                });
            }
            else {
                $scope.game = angular.fromJson(game); //Update the game info.
                $scope.gameId = $scope.game.gameId;
            }
        }
        hub.client.matchDone = function(message: string) {
            if (confirm(message)) {
                clearBoard($scope,comService);
            }
            else {
                clearBoard($scope,comService);
            }

            var menuHub = $.connection.menuHub;
            menuHub.server.viewLatestMatch();
        }

        $scope.gameId = -1;
        $scope.$watch(function () { return comService.userId }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.id = comService.userId;
            }
        });
        $scope.$watch(function () { return comService.loggedIn }, function (newVal, oldVal) {
            if (typeof newVal !== 'undefined') {
                $scope.loggedIn = comService.loggedIn;
            }
        });

        $scope.startGame = function () {
            if ($scope.id == -1) { //The comservice communication isn't right 100% of the times. So have to double check.
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

                $scope.$apply(function () { //To update page.
                    $scope.gameId = result;
                });
            });
        };
        $scope.playerClick = function (tileNumber: number) {
            if (verifyTurn($scope)) {
                hub.server.makeMove($scope.id, $scope.gameId, tileNumber);
            }
        }
    }

    function clearBoard($scope,comService) {
        $scope.$apply(function() {
            $scope.game = null;
            $scope.gameId = -1;
            comService.gameId = -1;
        });
    }

    function verifyTurn($scope) { //Returns true if it's the players turn.
        if ($scope.game != undefined) {
            if ($scope.game.currentPlayer == 1 && $scope.id == $scope.game.playerOne.id) {
                return true;
            } else if ($scope.game.currentPlayer == 2 && $scope.id == $scope.game.playerTwo.id) {
                return true;
            }
        }

        return false;
    }

    function comService() {
        var sharedInfo: any = {};
        sharedInfo.userId = -1;
        sharedInfo.loggedIn = false;
        sharedInfo.gameId = -1;

        return sharedInfo;
    }
})();
