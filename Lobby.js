/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/angularjs/angular.d.ts" />
(function () {
    'use strict';
    angular
        .module('app')
        .controller('menuController', menuController);
    menuController.$inject = ['$scope'];
    function menuController($scope) {
        $scope.title = 'controller';
        $scope.loggedIn = false;
        $scope.login = login($scope);
        activate();
        function activate() { }
    }
    function login($scope) {
        if ($scope.strUsername != null && $scope.strUsername != "") {
            $scope.loggedIn = !$scope.loggedIn;
        }
    }
})();
