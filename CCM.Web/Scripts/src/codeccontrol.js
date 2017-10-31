/* *******************************************************
 * Codec control frontpage */
ccmControllers.controller('sipInfoController', function ($scope, $http, $interval, $uibModalInstance, sipid, $timeout) {

    var vutimer;
    //var inputTimeoutHandle;

    $scope.codecOnline = false;  // 'true' if codec is reachable
    $scope.sipid = sipid;
    $scope.txL = -96;
    $scope.txR = -96;
    $scope.rxL = -96;
    $scope.rxR = -12;
    $scope.rebootconfirm1 = false;
    $scope.rebootconfirm2 = false;
    $scope.canAx = "ActiveXObject" in window;
    $scope.loadedPreset = '';
    $scope.presetToLoad = '';
    $scope.selectedLine = '';
    $scope.gpos = [];
    $scope.inputs = [];
    $scope.lines = [];
    $scope.lineStatusArea = '';
    $scope.audioModeArea = '';
    $scope.lastUpdate = new Date("1970-01-01"); // Last update initially long time ago

    $scope.setCodecIsOnline = function (codecIsOnline) {
        // Codec is reachable
        console.info('Codec is online:', codecIsOnline);
        $scope.codecOnline = codecIsOnline;

        if (codecIsOnline) {
            $scope.getAvailableGpos();
            $scope.getLoadedPreset();
            $scope.getInputData();
            $scope.startVuTimer();
        }
    };

    $scope.setInputValue = function (i, data) {
        console.info("Codec " + sipid + " level data, input " + i, data);
        var input = $scope.inputs[i];

        if (data.Error) {
            input.error = data.Error;
            input.disabled = 'disabled';
            input.stateClass = '';
            //input.upValue = $scope.info.InputGainStep;
            //input.downValue = -$scope.info.InputGainStep;
        } else {
            input.value = data.GainLevel;
            input.enabled = data.Enabled;
            input.disabled = '';
            input.stateClass = input.enabled ? 'btn-primary' : 'btn-default';
        }
    };

    $scope.getAvailableGpos = function () {
        console.info('Getting GPOs');
        $http.post('/api/codeccontrol/GetAvailableGpos', { id: sipid })
            .then(function (response) { // on success
                console.info('Available GPOs', response.data);
                $scope.gpos = response.data.Gpos;
            },
                function () { // on error
                    $scope.gpos = [];
                });
    };

    $scope.getLoadedPreset = function () {
        console.info('Getting presets');
        $http.post('/api/codeccontrol/GetLoadedPreset', { id: sipid }).then(function (response) {
            if (response.data.LoadedPreset !== null) {
                $scope.loadedPreset = response.data.LoadedPreset;
            } else {
                $scope.loadedPreset = '';
            }
        });
    };

    $scope.loadPreset = function (preset) {
        if (typeof (preset) === 'object') {
            $http.post('/api/codeccontrol/LoadPreset', { id: sipid, name: preset.Name }).then(function (response) {
                $scope.getLoadedPreset();
            });
        }
    };

    $scope.getLineStatus = function (line) {
        if (typeof (line) === 'object') {
            $http.get('/api/codeccontrol/GetLineStatus?id=' + sipid + '&line=' + line.number).then(function (response) {
                var data = response.data;
                console.log(data);
                var msg = "LineStatus: " + data.LineStatusDto.Name + ". DisconnectReason: " + data.DisconnectReasonDto.Name + " (" + data.DisconnectReasonDto.Value + ").\r\n";
                $scope.lineStatusArea = msg + $scope.lineStatusArea;
            });
        }
    };

    $scope.getAudioMode = function () {
        $http.get('/api/codeccontrol/GetAudioMode?id=' + sipid).then(function (response) {
            var data = response.data;
            console.log(data);
            var msg;
            if (data.Error) {
                msg = data.Error;
            } else {
                msg = "AudioMode: Encoder=" + data.EncoderAudioModeString + " (" + data.EncoderAudioMode + ")" + ", Decoder=" + data.DecoderAudioModeString + " (" + data.DecoderAudioMode + ")";
            }
            $scope.audioModeArea = msg + "\r\n" + $scope.audioModeArea;
        });
    };

    $scope.setGpo = function (gpo) {
        var active = gpo.Active ? false : true;
        $http({ url: '/api/codeccontrol/SetGpo', method: "GET", params: { id: sipid, number: gpo.Number, active: active } })
            .then(function (response) {
                console.log("Codec GPO data: ", response.data);
                gpo.Active = response.data.Active;
            });
    };

    $scope.startVuTimer = function () {
        if (angular.isDefined(vutimer)) return;

        vutimer = $interval(function () {
            if ($scope.shouldUpdateGui()) {
                console.log("Should update now");
                $scope.lastUpdate = new Date(Date.now() + 10000); // If not changed, next update will be in 10 seconds
                $scope.getInputData();
            }
        }, 100);
    };

    $scope.shouldUpdateGui = function () {
        // Returns true when it's time to update
        let now = Date.now();
        let updateTime = new Date($scope.lastUpdate + 300);
        //console.log("Should update?", now, updateTime);
        return !!(
            $('#codecControlTab').hasClass('active') &&
                $('#inputsTab').hasClass('active') &&
                now > updateTime
        );
    }

    $scope.getInputData = function () {
        console.info('Getting codec audio status');

        var nrOfInputs = $scope.inputs.length;

        $http.get('/api/codeccontrol/GetAudioStatus?id=' + sipid + '&nrOfInputs=' + nrOfInputs).then(function (response) {
            var data = response.data;
            console.log("Audio status", data);

            for (var i = 0; i < nrOfInputs; i++) {
                var inputStatus = data.InputStatuses[i];
                $scope.setInputValue(i, inputStatus);
            }

            for (var j = 0; j < $scope.gpos.length; j++) {
                var gpo = $scope.gpos[j];
                var gpoData = !!data.Gpos[j];
                console.log("GPO", gpo, gpoData);
                gpo.Active = gpoData;
            }

            console.info("* Codec VU-data", data.VuValues);
            // Values is presented in db where 0 = Fullscale +18db, -18 = Test 0dB, -96 = min-level
            $scope.txL = fallback($scope.txL, convertVuToPercentage(data.VuValues.TxLeft));
            $scope.txR = fallback($scope.txR, convertVuToPercentage(data.VuValues.TxRight));
            $scope.rxL = fallback($scope.rxL, convertVuToPercentage(data.VuValues.RxLeft));
            $scope.rxR = fallback($scope.rxR, convertVuToPercentage(data.VuValues.RxRight));
            console.info("VU values", $scope.txL, $scope.txR, $scope.rxL,$scope.rxR);
            $scope.lastUpdate = Date.now();

        });

    };
    $scope.stopVuTimer = function () {
        if (angular.isDefined(vutimer)) {
            $interval.cancel(vutimer);
            vutimer = undefined;
        }

        $scope.txL = 0;
        $scope.txR = 0;
        $scope.rxL = 0;
        $scope.rxR = 0;
    };

    //$scope.checkInputValues = function () {
    //    // TODO: Använd callback då användaren väljer / väljer bort tabben
    //    console.info('Check input values');
    //    if ($('#codecControlTab').hasClass('active') && $('#inputsTab').hasClass('active')) {
    //        $scope.getInputData();
    //    }

    //    console.info('Set input timeout');
    //    inputTimeoutHandle = $timeout(function () {
    //        $scope.checkInputValues();
    //    }, 1000);

    //};

    //$scope.disableInputChecking = function () {
    //    if (angular.isDefined(inputTimeoutHandle)) {
    //        $timeout.cancel(inputTimeoutHandle);
    //        inputTimeoutHandle = undefined;
    //    }
    //};

    //$scope.getVuValues = function () {
    //    $http.get('/api/codeccontrol/GetVuValues?id=' + sipid).then(
    //        function (response) {
    //            var data = response.data;
    //            if (data.Error) {
    //                // Do nothing
    //                console.error('* Codec error when getting vu values');
    //            } else {
    //                console.info("* Codec VU-data", data);
    //                // Values is presented in db where 0 = Fullscale +18db, -18 = Test 0dB, -96 = min-level
    //                $scope.txL = fallback($scope.txL, convertVuToPercentage(data.TxLeft));
    //                $scope.txR = fallback($scope.txR, convertVuToPercentage(data.TxRight));
    //                $scope.rxL = fallback($scope.rxL, convertVuToPercentage(data.RxLeft));
    //                $scope.rxR = fallback($scope.rxR, convertVuToPercentage(data.RxRight));
    //            }
    //        });
    //};

    $scope.toggleInput = function (input) {
        var enabled = input.enabled ? false : true;
        $http.post('/api/codeccontrol/SetInputEnabled', { id: $scope.sipid, input: input.id, enabled: enabled })
            .then(function (response) {
            }
            , function (response) {
            });

        input.enabled = input.enabled ? false : true;
        input.stateClass = input.enabled ? 'btn-primary' : 'btn-default';
    };

    $scope.setGainLevel = function (input, value) {
        $http.post('/api/codeccontrol/SetInputGainLevel', { id: $scope.sipid, input: input.id, level: input.value + value })
            .then(function (response) {
                var data = response.data;
                if (data.error) {
                    // Do nothing
                } else {
                    input.value = data.GainLevel;
                }
            });
    };

    $scope.cancel = function () {
        // TODO: in use?
        $uibModalInstance.dismiss('cancel');
    };

    $scope.openAdmin = function (link, width, height, scrollbars) {
        if (width <= 0) {
            width = 1000;
        }

        if (height <= 0) {
            height = 620;
        }

        window.open(link, '_blank', 'width=' + width + ',height=' + height + ',status=no,menubar=no,resizable=no,scrollbars=' + (scrollbars ? 'yes' : 'no') + ',toolbar=no,location=no,directories=no');
        return false;
    };

    $scope.reboot = function (info) {
        this.rebootconfirm1 = false;
        this.rebootconfirm2 = false;
        $http.post('/api/codeccontrol/RebootCodec', { id: info.Id });
    };

    $scope.$on("$destroy", function () {
        console.log("Destroy called, stop input checking, stop VU");
        $scope.stopVuTimer();
    });

    console.info('Getting Codec information for ' + $scope.sipid);

    $http.get('/api/registeredsipdetails/GetRegisteredSipInfo?id=' + $scope.sipid)
        .then(function (response) { // on success
            console.info('Codec information', response.data);
            $scope.info = response.data;

            if ($scope.info.IsAuthenticated && $scope.info.CodecControl) {
                console.info('* Codec-control is Authenticated');

                if ($scope.info.Inputs) {
                    var inputs = [];
                    for (var i = 0; i < $scope.info.Inputs; i++) {
                        var input = { id: i, number: i + 1, value: 0, enabled: false, disabled: 'disabled', stateClass: '' };
                        inputs.push(input);
                    }
                    $scope.inputs = inputs;
                }

                if ($scope.info.Lines) {
                    var lines = [];
                    for (var j = 0; j < $scope.info.Lines; j++) {
                        var line = { id: j, number: j + 1 };
                        lines.push(line);
                    }
                    $scope.lines = lines;
                    if (lines.length > 0) {
                        $scope.selectedLine = lines[0];
                    }
                }

                console.info('Checking if codec is online');
                $http.get('/api/codeccontrol/CheckCodecAvailable?id=' + sipid).then(function (result) {
                    $scope.setCodecIsOnline(result.data);
                });
            } else {
                console.info('* Codec-control is not Authorized');
            }
        },
        function () { // on error
            console.info("Codec information is missing");
        });

});
