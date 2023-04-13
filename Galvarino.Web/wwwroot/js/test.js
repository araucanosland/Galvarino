$(function () {

    $("#btn").on("click", function () {
        debugger;
        $.ajax({
            type: "GET",
            url: `/api/test/trae-oficinas`
        }).done(function (data) {
            debugger;

        }).fail(function (errMsg) {
            debugger;
            console.log(errMsg)
            $.niftyNoty({
                type: "danger",
                container: "floating",
                title: "Error",
                message: errMsg.responseText,
                closeBtn: true,
                timer: 5000
            });


        })

    });


});