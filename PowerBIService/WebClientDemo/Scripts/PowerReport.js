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

            var parentWorkSpaceName=$("#GroupFromList option:selected").text();  
            var childWorkSpace=$("#GroupToList option:selected").text();
    
            var reports=[];
            reports.push({ ParentReportName:$("#fromReports option:selected").text(),CloneReportName:$("#cloneReportName").val(),WebApiEndPoint:$("#webApiUrl").val() })
                  
            var CloneReportRequestVM={
    
                ParentWorkSpace:parentWorkSpaceName,
                ClientWorkSpace:childWorkSpace,
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
    
    
}(window.PowerReport = window.PowerReport || {}, jQuery));