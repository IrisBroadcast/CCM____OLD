function locationSearch() {
    $('#locationSearchBtn').prop('disabled', true);

    var queryParams = {
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        regionId: $('#Regions').val(),
        ownerId: $('#Owners').val(),
        codecTypeId: $('#CodecTypes').val()
    };
    var sim24HourParams = {
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        regionId: $('#Regions').val(),
        locationId: '00000000-0000-0000-0000-000000000000'
    };

    $('#errorInfo').html('');
    $('#locationNumberOfCallsChartDiv').load('/Statistics/GetLocationNumberOfCallsTable', queryParams, function(response, status, xhr) {
        if (status === 'error') {
            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
            $('#locationSearchBtn').prop('disabled', false);
        } else {
            $('#locationTotalTimeForCallsChartDiv').load('/Statistics/GetLocationTotaltTimeForCallsTable', queryParams, function (response, status, xhr) {
                if (status === 'error') {
                    $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                    $('#locationSearchBtn').prop('disabled', false);
                } else {
                    $('#locationMaxSimultaneousCallsChartDiv').load('/Statistics/GetLocationMaxSimultaneousCallsTable', queryParams, function (response, status, xhr) {
                        if (status === 'error') {
                            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                            $('#locationSearchBtn').prop('disabled', false);
                        } else {
                            $('#locationSim24HourChartDiv').load('/Statistics/GetLocationSim24HourChart', sim24HourParams, function (response, status, xhr) {
                                if (status === 'error') {
                                    $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                                    $('#locationSearchBtn').prop('disabled', false);
                                } else {
                                    $('#locationSim24HourSelect').on("change", sim24HourSearch);
                                    $('#locationSearchBtn').prop('disabled', false);
                                }
                            });
                        }
                    });
                }
            });
        }
    });
}

function sim24HourSearch() {
    $('#locationSim24HourSelect').prop('disabled', true);

    var sim24HourParams = {
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        regionId: $('#Regions').val(),
        locationId: $('#locationSim24HourSelect').val()
    };
    $('#locationSim24HourChartDiv').load('/Statistics/GetLocationSim24HourChart', sim24HourParams, function (response, status, xhr) {
        if (status === 'error') {
            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
        } else {
            $('#locationSim24HourSelect').on("change", sim24HourSearch);
        }
    });
}

function regionSearch() {
    $('#regionSearchBtn').prop('disabled', true);

    var queryParamsCalls = {
        filterType: 'Regions',
        chartType: 'NumberOfCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#regionRegions').val()
    };

    var queryParamsTime = {
        filterType: 'Regions',
        chartType: 'TotalTimeForCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#regionRegions').val()
    };

    $('#errorInfo').html('');
    $('#regionNumberOfCallsChartDiv').load('/Statistics/GetDateBasedChart', queryParamsCalls, function (response, status, xhr) {
        if (status === 'error') {
            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
            $('#regionSearchBtn').prop('disabled', false);
        } else {
            $('#regionTimeChartDiv').load('/Statistics/GetDateBasedChart', queryParamsTime, function (response, status, xhr) {
                if (status === 'error') {
                    $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                }
                $('#regionSearchBtn').prop('disabled', false);
            });
        }
    });
}

function sipAccountsSearch() {
    $('#sipAccountsSearchBtn').prop('disabled', true);

    var queryParamsCalls = {
        filterType: 'SipAccounts',
        chartType: 'NumberOfCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#sipAccountsAccounts').val()
    };

    var queryParamsTime = {
        filterType: 'SipAccounts',
        chartType: 'TotalTimeForCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#sipAccountsAccounts').val()
    };

    $('#errorInfo').html('');
    $('#sipAccountsNumberOfCallsChartDiv').load('/Statistics/GetDateBasedChart', queryParamsCalls, function (response, status, xhr) {
        if (status === 'error') {
            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
            $('#sipAccountsSearchBtn').prop('disabled', false);
        } else {
            $('#sipAccountsTimeChartDiv').load('/Statistics/GetDateBasedChart', queryParamsTime, function (response, status, xhr) {
                if (status === 'error') {
                    $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                }
                $('#sipAccountsSearchBtn').prop('disabled', false);
            });
        }
    });
}

function codecTypesSearch() {
    $('#codecTypesSearchBtn').prop('disabled', true);

    var queryParamsCalls = {
        filterType: 'CodecTypes',
        chartType: 'NumberOfCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#codecTypesCodecTypes').val()
    };

    var queryParamsTime = {
        filterType: 'CodecTypes',
        chartType: 'TotalTimeForCalls',
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        filterId: $('#codecTypesCodecTypes').val()
    };

    $('#errorInfo').html('');
    $('#codecTypeNumberOfCallsChartDiv').load('/Statistics/GetDateBasedChart', queryParamsCalls, function (response, status, xhr) {
        if (status === 'error') {
            $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
            $('#codecTypesSearchBtn').prop('disabled', false);
        } else {
            $('#codecTypeTimeChartDiv').load('/Statistics/GetDateBasedChart', queryParamsTime, function (response, status, xhr) {
                if (status === 'error') {
                    $('#errorInfo').html('Fel vid laddning av statistik: ' + xhr.status + " " + xhr.statusText);
                }
                $('#codecTypesSearchBtn').prop('disabled', false);
            });
        }
    });
}

$(function () {
    var endDate = new Date();
    endDate.setHours(0);
    endDate.setMinutes(0);
    endDate.setSeconds(0);
    endDate.setMilliseconds(0);

    var startDate = new Date(endDate.toISOString());
    startDate.setDate(startDate.getDate() - 30);

    $('#startdatetimepicker').datetimepicker({ format: 'YYYY-MM-DD', date: startDate.toISOString() });
    $('#enddatetimepicker').datetimepicker({ format: 'YYYY-MM-DD', date: endDate.toISOString() });
    
    $("#startdatetimepicker").on("dp.change", function (e) {
        $('#enddatetimepicker').data("DateTimePicker").minDate(e.date);
    });
    $("#enddatetimepicker").on("dp.change", function (e) {
        $('#startdatetimepicker').data("DateTimePicker").maxDate(e.date);
    });

    $('#locationSearchBtn').on("click", locationSearch);
    $('#regionSearchBtn').on("click", regionSearch);
    $('#sipAccountsSearchBtn').on('click', sipAccountsSearch);
    $('#codecTypesSearchBtn').on('click', codecTypesSearch);

});

