$(function (PowerReport, $) {
    var Url;
    PowerReport.init = function (initObject) {
      Url=initObject.Urls;
    };
    PowerReport.LoadReportForWorkSpace=function(item){
      var url=PowerReport.stringFormat(Url.GetGroupReportsUrl,item);
      
            $.ajax({
                url: url,
                type: "GET",
                async: true,
                dataType: "json",
                contentType: "application/json"
               
            })
            .done(function (data) {
                $("#fromReports").empty();
                $.each(data, function(key, value) {
                    $("#fromReports").append("<option value="+value.Id+">" + value.Name + "</option>");
                });
            });
        
    };
    PowerReport.CloneReport=function(){

            var parentWorkSpaceName=$("#GroupFromList option:selected").val();  
            var childWorkSpace=$("#GroupToList option:selected").val();
                         
            if($("#cloneReportName").val()==''){
                
                alert('Client report is empty');
                return;
            }
            
            var reports=[];
            reports.push({ ParentReportId:$("#fromReports option:selected").val(),CloneReportName:$("#cloneReportName").val(),WebApiEndPoint:$("#webApiUrl").val() })
                  
            var CloneReportRequestVM={

                ParentWorkSpaceId:parentWorkSpaceName,
                ClientWorkSpaceId:childWorkSpace,
                CloneReports:reports
                
            };         
            $.ajax({
                url: PowerReport.stringFormat(Url.CloneReportsUrl),
                data: JSON.stringify(CloneReportRequestVM),
                type: 'POST',
                contentType: "application/json"
            })
            .done(function (data) {

                $.each(data, function(key, value) {
                    $("#successReport").val('ParentReport:'+ value.ParentReportName + '  Client Report:'+ value.CloneReportName + '  Success:'+ value.Success);
                });
            })
            .fail(function (error) {
                   
            });
    };
        
    PowerReport.stringFormat = function (format) {
        if (!format)
            return "";

        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
                ? args[number]
                : match
                ;
        });
    };

    PowerReport.LoadEmbedReportForWorkSpace=function(item){
        var url=PowerReport.stringFormat(Url.GetGroupReportsUrl,item);

        $.ajax({
            url: url,
            type: "GET",
            async: true,
            dataType: "json",
            contentType: "application/json"

        })
            .done(function (data) {
                $("#fromReportsEmbed").empty();
                $.each(data, function(key, value) {
                    $("#fromReportsEmbed").append("<option value="+value.Id+">" + value.Name + "</option>");
                });
            });
    };

    PowerReport.EmbedReport=function(){

        var workSpaceId=$("#GroupEmebdList option:selected").val();
        var reportId=$("#fromReportsEmbed option:selected").val();
        var embedReportUrl=$("#webApiUrl").val();
        var parameters=[];

        var EmbedReportRequestVM={

            ReportId:reportId,
            WorkSpaceId:workSpaceId,
            EmbedReportUrl:embedReportUrl,
            ParaMeters:parameters,
            EmbedUserName:'',
            EmbedRoles:''
        };
        $.ajax({
            url: PowerReport.stringFormat(Url.EmbedReportsUrl),
            data: JSON.stringify(EmbedReportRequestVM),
            type: 'POST',
            contentType: "application/json"
        })
            .done(function (data) {

                var accessToken = data.EmbedToken.token;
                var embedUrl = data.EmbedUrl;
                var embedReportId = data.Id;
                var models = window['powerbi-client'].models;

                var config = {
                    type: 'report',
                    tokenType: models.TokenType.Embed,
                    accessToken: accessToken,
                    embedUrl: embedUrl,
                    id: embedReportId,
                    pageView: "fitToWidth",
                    permissions: models.Permissions.All,
                    settings: {
                        filterPaneEnabled: true,
                        navContentPaneEnabled: true
                    }
                };

                var reportContainer = $('#reportContainer')[0];
                var report = powerbi.embed(reportContainer, config);
                
                
            })
            .fail(function (error) {

            });
    }
    
}(window.PowerReport = window.PowerReport || {}, jQuery));