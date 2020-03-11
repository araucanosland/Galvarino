$(document).ready(function () {
    LoadTable();
});

function LoadTable() {
    debugger;
    var oTable = $('#example').dataTable({
        dom: 'Bfrtip',
        buttons: [
            'copyHtml5',
            'excelHtml5',
            'csvHtml5',
            'pdfHtml5'
        ],
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": "/api/wfsc/v1/reporte-setcomplementario/PREPARAR_NÓMINA_SET_COMPLEMENTARIO",
            "type": "GET"
        },
        "columnDefs": [{ //this definition is set so the column with the action buttons is not sortable
            "targets": -1, //this references the last column of the date
            "orderable": false
        }],
        "columns": [
            {
                "data": "FolioCrrdito",
                "width": "10%"
            }
        ],
    });
}


