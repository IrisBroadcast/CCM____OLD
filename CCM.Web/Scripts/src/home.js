'use strict';

/* *******************************************************
 * JS Application */
var ccmApp = angular.module('ccm', ['ccmControllers', 'ui.bootstrap', 'ngStorage']);

ccmApp.filter('timeAgo', function () {
    return function (started, currentTime) {
        var secNum = parseInt((currentTime - started) / 1000, 10);
        if (secNum < 0) {
            return '---';
        }

        var days = Math.floor(secNum / (24 * 60 * 60));
        var hours = Math.floor(secNum / (60 * 60)) % 24;
        var minutes = Math.floor(secNum / 60) % 60;
        var seconds = secNum % 60;

        if (hours < 10) { hours = "0" + hours; }
        if (minutes < 10) { minutes = "0" + minutes; }
        if (seconds < 10) { seconds = "0" + seconds; }
        if (days > 1) {
            return days + " dagar " + hours + ':' + minutes + ':' + seconds; // TODO: Internationalisera detta
        } else if (days === 1) {
            return days + " dag " + hours + ':' + minutes + ':' + seconds;
        } else {
            return hours + ':' + minutes + ':' + seconds;
        }
    }
});

ccmApp.filter('max', function () {
    return function (value, max) {
        if (max < 0) { max = 0; }
        return value > max ? max : value;
    }
});

ccmApp.filter('callTimeWarning', function () {
    // {{'text'|callTimeWarning:warningLevel(10000):call.start:call.end}}
    return function (type, warningLevel, callStart, callEnd) { 
        var diff = new Date(callEnd) - new Date(callStart);
        if (parseInt(diff) <= warningLevel ) { 
            if(type=='class') {
                return 'list-ccm--warning-calltime';
            } else {
                return true;
            }
        } else {
            return false;
        }
    }
});

ccmApp.directive('shortcut', function() {
  return {
    restrict: 'E',
    replace: true,
    scope: true,
    link:    function postLink(scope, iElement, iAttrs){
      jQuery(document).on('keypress', function(e){
         scope.$apply(scope.keyPressed(e));
       });
    }
  };
});

ccmApp.factory('backendHubProxy', ['$rootScope', function ($rootScope) {

    function backendFactory(serverUrl, hubName) {

        var connection = $.hubConnection(serverUrl);
        connection.logging = true;
        
        connection.error(function (error) {
            console.error('* SignalR error: ', error);
        });

        connection.connectionSlow(function () {
            console.log("* SignalR connectionSlow");
        });
        
        connection.disconnected(function () {
            console.log("* SignalR disconnected");
            setTimeout(function () {
                connection.start();
            }, 2000);
        });

        var proxy = connection.createHubProxy(hubName);

        return {
            on: function (eventName, callback) {
                proxy.on(eventName,
                    function (result) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback(result);
                            }
                        });
                    });
            },
            start: function () {
                connection.start()
                    .done(function () { 
                        console.log("* Connected to SignalR hub. Connection id=" + connection.id + ", Transport=" + connection.transport.name); 
                    })
                    .fail(function () { 
                        console.log('* Could not connect to hubName: ', hubName, ', serverUrl: ', serverUrl); 
                    });
            },
            stop: function () {
                connection.stop(false, true);
            },
            status: function (callback) {
                // Get connection status
                connection.starting(
                    function () {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback({ action: "starting", current: 5 });
                            }
                        });
                    });
                connection.stateChanged(
                    function (state) {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback({ action: "statechanged", current: state.newState });
                            }
                        });
                    });
                connection.disconnected(
                    function () {
                        $rootScope.$apply(function () {
                            if (callback) {
                                callback({ action: "disconnected", current: 4 });
                            }
                        });
                    });
            }
        };
    };

    return backendFactory;
}]);

ccmApp.directive('vuMeter', function () {
    return {
        restrict: 'E',
        scope: { level: '=level' },
        template: "<div class='progress vertical'>" +
            "<div class='progress-bar green' style='bottom: 0%;' ng-style='{ height: (level | max:70) + \"%\"}'></div>" +
            "<div class='progress-bar yellow' style='bottom: 70%;' ng-style='{height: (level-70 | max:15) + \"%\"}'></div>" +
            "<div class='progress-bar red' style='bottom: 85%;' ng-style='{height: (level-85 | max:15) + \"%\"}'></div>" +
            "</div>"
    };
});

var ccmControllers = angular.module('ccmControllers', []);

ccmControllers.controller('overviewController', function($scope, $http, $interval, $uibModal, backendHubProxy, $sessionStorage) {

        var timerHandle_updateCallDuration;

        $scope.currentTime = new Date();

        $scope.startCallDurationTimer = function() {
            if (angular.isDefined(timerHandle_updateCallDuration)) return;

            timerHandle_updateCallDuration = $interval(function() {
                    $scope.currentTime = new Date();
                },
                1000);
        };

        $scope.stopCallDurationTimer = function() {
            if (angular.isDefined(timerHandle_updateCallDuration)) {
                $interval.cancel(timerHandle_updateCallDuration);
                timerHandle_updateCallDuration = undefined;
            }
        };

        var setOngoingCalls = function(ongoingCalls) {
            // Updates start-time based on duration and local time/clock
            // call-length is presented in a correct way even if there is different
            // times on server and client

            ongoingCalls.forEach(function(item) {
                var startTime = new Date() - (item.DurationSeconds * 1000);
                item.StartedLocal = new Date(startTime);
            });

            if (ongoingCalls.length > 0) {
                $scope.startCallDurationTimer();
            } else {
                $scope.stopCallDurationTimer();
            }

            $scope.onGoingCalls = ongoingCalls;
        }

        var viewIsFiltered = function() {
            if ($scope.codecType || $scope.region || $scope.searchString) {
                return true;
            } else {
                return false;
            }
        }

        var onOldCalls = function(oldCalls) {
            if (viewIsFiltered()) {
                // If filter is applied get filtered data from server and show
                $scope.refreshOldFiltered();
            } else {
                $scope.oldCalls = oldCalls;
            }
        }

        /* *******************************************************
         * Hub Sockets */
        var ccmDataHub = backendHubProxy("/signalr", 'WebGuiHub');

        ccmDataHub.on('codecsOnline',
            function(data) {
                console.log("Received codecs online update: ", data.length);
                $scope.registeredSips = data;
            });

        ccmDataHub.on('ongoingCalls',
            function(data) {
                console.log("Received ongoing calls update: ", data.length);
                setOngoingCalls(data);
            });

        ccmDataHub.on('oldCalls',
            function(data) {
                console.log("Received old calls update: ", data.length);
                onOldCalls(data);
            });

        ccmDataHub.start();

        $scope.socketStatusHubGui = { reconnectAttempts: 0, socketStatus: false};
        ccmDataHub.status(
            function (state) {
                console.log("* Status changed: ", state);
                if (state.current == 1) {
                    // Connecting
                    $scope.socketStatusHubGui.reconnectAttempts = 0;
                    $scope.socketStatusHubGui.socketStatus = true;
                } else if (state.current == 2) {
                    // Connected ish
                    $scope.socketStatusHubGui.reconnectAttempts = 0;
                    $scope.socketStatusHubGui.socketStatus = false;
                } else if (state.current == 3) {
                    // Reconnecting
                    $scope.socketStatusHubGui.reconnectAttempts += 1;
                    $scope.socketStatusHubGui.socketStatus = false;
                } else if (state.current == 4) {
                    // Disconnected
                    $scope.socketStatusHubGui.socketStatus = false;
                } else if (state.current == 5) {
                    // Starting
                    $scope.socketStatusHubGui.reconnectAttempts += 1;
                    $scope.socketStatusHubGui.socketStatus = false;
                } else {
                    // Unknown
                    $scope.socketStatusHubGui.socketStatus = false;
                }
            });

        /* *******************************************************
         * Lists and data, filtering */
        $scope.registeredSipsFilter = function(item) {
            if ($scope.region && item.RegionName !== $scope.region) {
                return false;
            }

            if ($scope.codecType && item.CodecTypeName !== $scope.codecType) {
                return false;
            }

            if ($scope.searchString) {
                var search = $scope.searchString.toLowerCase();
                if ($scope.containsString(item.DisplayName, search)) {
                    return true;
                }
                if ($scope.containsString(item.Sip, search)) {
                    return true;
                }
                if ($scope.containsString(item.UserName, search)) {
                    return true;
                }
                if ($scope.containsString(item.UserComment, search)) {
                    return true;
                }
                if ($scope.containsString(item.Location, search)) {
                    return true;
                }
                if ($scope.containsString(item.LocationShortName, search)) {
                    return true;
                }
                if ($scope.containsString(item.UserDisplayName, search)) {
                    return true;
                }
                return false;
            }

            return true;
        };

        $scope.callFilter = function(item) {
            if ($scope.region && item.FromRegionName !== $scope.region && item.ToRegionName !== $scope.region) {
                return false;
            }

            if ($scope.codecType &&
                item.FromCodecTypeName !== $scope.codecType &&
                item.ToCodecTypeName !== $scope.codecType) {
                return false;
            }

            if ($scope.searchString) {
                var search = $scope.searchString.toLowerCase();
                if ($scope.containsString(item.FromDisplayName, search)) {
                    return true;
                }
                if ($scope.containsString(item.ToDisplayName, search)) {
                    return true;
                }
                if ($scope.containsString(item.FromSip, search)) {
                    return true;
                }
                if ($scope.containsString(item.ToSip, search)) {
                    return true;
                }
                if ($scope.containsString(item.FromLocationName, search)) {
                    return true;
                }
                if ($scope.containsString(item.ToLocationName, search)) {
                    return true;
                }
                if ($scope.containsString(item.FromLocationShortName, search)) {
                    return true;
                }
                if ($scope.containsString(item.ToLocationShortName, search)) {
                    return true;
                }
                if ($scope.containsString(item.FromUserDisplayName, search)) {
                    return true;
                }
                if ($scope.containsString(item.ToUserDisplayName, search)) {
                    return true;
                }
                return false;
            }

            return true;
        };

        $scope.containsString = function(item, searchString) {
            return !!(item && item.toLowerCase().indexOf(searchString) > -1);
        };

        $scope.refreshOngoing = function() {
            $http.post('/api/OngoingCall')
                .then(function(response) {
                    setOngoingCalls(response.data);
                }, function() {
                    console.error("No answer from /api/OngoingCall");
                });
        }

        $scope.refreshOld = function() {
            $http.post('/api/OldCall')
                .then(function(response) {
                    $scope.oldCalls = response.data;
                }, function(response) {
                    console.error("No answer from /api/OldCall");
                });
        }

        $scope.refreshOldFiltered = function() {
            var region = $scope.region ? $scope.region : "";
            var codecType = $scope.codecType ? $scope.codecType : "";
            console.log("Getting filtered cancelled/hungup calls, region: ", region,", type: ", codecType,", search: ", $scope.searchString);
            $http.get('/api/OldCallFiltered?region=' +
                    encodeURI(region) +
                    '&codecType=' +
                    encodeURI(codecType) +
                    '&search=' +
                    encodeURI($scope.searchString))
                .then(function(response) {
                    var oldCalls = response.data;
                    console.log("Recieved cancelled/hungup calls from server: ", oldCalls.length);
                    $scope.oldCalls = oldCalls;
                }, function(response) {
                    console.error("No answer from /api/OldCallFiltered");
                });
        }

        $scope.refreshRegistered = function() {
            $http.post('/api/RegisteredSipsOverview')
                .then(function(response) {
                    $scope.registeredSips = response.data;
                }, function(response) {
                    console.error("No answer from /api/RegisteredSipsOverview");
                });
        }

        $scope.changeOngoingCallSorting = function(sortProperty) {
            $scope.setSorting($scope.ongoingCallSort, sortProperty);
        }

        $scope.changeRegisteredSorting = function(sortProperty) {
            $scope.setSorting($scope.registeredSort, sortProperty);
        }

        $scope.setSorting = function(sort, sortProperty) {
            if (sort.column === sortProperty) {
                sort.descending = !sort.descending;
            } else {
                sort.column = sortProperty;
                sort.descending = false;
            }
            console.log("Sort: ", sort.column, ", descending: ", sort.descending);
        }

        $scope.editComment = function(id) {
            var url = '/home/EditRegisteredSipComment/' + id;
            $('#registeredSipModal').modal({ remote: url });
        }

        $scope.setFilterRegion = function(region) {
            $scope.region = region;
            $scope.regionName = region || 'Alla';
            $('#regions li').removeClass('active');
            $('#regions li:contains(' + $scope.regionName + ')').addClass('active');
            $sessionStorage.region = $scope.region;
        }

        $scope.setFilterCodecType = function(codecType) {
            $scope.codecType = codecType;
            $scope.codecTypeName = codecType || 'Alla';
            $('#codecTypes li').removeClass('active');
            $('#codecTypes li:contains(' + $scope.codecTypeName + ')').addClass('active');
            $sessionStorage.codecType = $scope.codecType;
        }

        $scope.showSipInfo = function(id, $event) {
            $event.preventDefault();
            $uibModal.open({
                templateUrl: 'sipInfo.html',
                controller: 'sipInfoController',
                resolve: { sipid: function() { return id; } }
            }).result.catch(function(res) {
                console.log('Closed modal', res);
                if (!(res === 'cancel' || res === 'escape key press')) {
                    throw res;
                }
            });
        }

        $scope.$on("$destroy",
            function() {
                $scope.stopCallDurationTimer();
            });

        // Sort columns init
        $scope.registeredSort = { column: 'DisplayName', descending: false };
        $scope.ongoingCallSort = { column: 'DurationSeconds', descending: false };

        // Set filter inits from session
        $scope.setFilterRegion($sessionStorage.region);
        $scope.setFilterCodecType($sessionStorage.codecType);
        $scope.searchString = '';

        $scope.refreshOngoing();
        $scope.refreshRegistered();

        if (viewIsFiltered()) {
            $scope.refreshOldFiltered();
        } else {
            $scope.refreshOld();
        }

        $scope.$watch('codecType',
            function(newValue, oldValue) {
                if (newValue === oldValue) {
                    return;
                }
                console.log('Codec type changed', $scope.codecType);
                $scope.refreshOldFiltered();
            });

        $scope.$watch('region',
            function(newValue, oldValue) {
                if (newValue === oldValue) {
                    return;
                }
                console.log('Region changed', $scope.region);
                $scope.refreshOldFiltered();
            });

        $scope.$watch('searchString',
            function(newValue, oldValue) {
                if (newValue === oldValue) {
                    return;
                }
                console.log('Search string changed', $scope.searchString);
                $scope.refreshOldFiltered();
            });

        $scope.searchKeyUp = function(keyCode) {
            if (keyCode === 27) {
                // when escape = 27 is pressed clear field
                $scope.searchString = "";
            }
        }

        // Night-mode, full-overview , shift + i = 73, ctrl + i = 9
        $scope.uiStateNightmode = false;
        $scope.keyPressed = function(e) {
            if(e.which==9) {
                console.log("Triggered nightmode");
                $scope.uiStateNightmode = !$scope.uiStateNightmode;
            }
        };
});

function convertVuToPercentage(value) {
    // Konverterar db till utslag på mätare mellan 0% och 100%
    // Skalan är "linjär db" där varje db ger lika mycket utslag på mätaren.
    var maxScaleValue = 0; // dB-värde då mätaren ska visa 100%
    var minScaleValue = -60; // dB-värde då mätaren ska visa 0%
    var scaleLength = maxScaleValue - minScaleValue;

    value = value + scaleLength;
    var p = value / scaleLength * 100;
    return p > 100 ? 100 : (p < 0 ? 0 : p);
}

var fallback = function (lastValue, newValue) {
    var updateInterval = 500; // i ms
    var decayRate = 12.0; // i dB/s
    var maxDecay = decayRate * updateInterval / 1000;
    return newValue >= lastValue ? newValue : Math.max(newValue, lastValue - maxDecay);
}

function makeUrlAbsolute(url) {
    if (typeof url == 'undefined' || url === null) {
        return "";
    }
    return url.match(/^[a-zA-Z]+:\/\//) ? url : 'http://' + url;
}
