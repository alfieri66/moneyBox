var infoAgentiPerAnalisi = new Array();
var infoAgentiPerAnalisiCopia = new Array();

function btnLeggiAgentiPerAnalisi() {
    var info = new Object;
    info.dataIncasso = $("#dataAnalisi").val();
    datiJson = JSON.stringify(info);

    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "leggiAgentiPerAnalisiTmp",
        type: "POST",
        data: datiJson,
        contentType: "application/json",
        dataType: "json",
        success: successoLeggiAgentiPerAnalisi,
        error: erroreLeggiAgentiPerAnalisi
    });
};

function successoLeggiAgentiPerAnalisi(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.length > 0) {
        copiaArray(dati, infoAgentiPerAnalisi);
        copiaArray(dati, infoAgentiPerAnalisiCopia);
        popolaTabellaAgentiPerAnalisi(infoAgentiPerAnalisi);
    }
}

function erroreLeggiAgentiPerAnalisi() {
    attesa.style.display = "none";
}



function popolaTabellaAgentiPerAnalisi(data) {
    var tdStato = '';
    var tdNome = '';
    var tdRecupero = '';
    var tdDaRiportare = '';
    var tdAcconto = '';
    var tdPdf = '';
    var totAcconto = 0;
    var totRecupero = 0;
    var totDaRiportare = 0;

    var stringaHtml = "";
    var riga = 0;
    if (data.length > 0) {
        $.each(data, function (key, val) {
            riga++;
            areaCollapse = "collapse_" + riga.toString();
            tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="Link"><i class="fas fa-external-link-alt" data-toggle="collapse" data-target="#' + areaCollapse +'")></i></td>';
            tdNome = '<td>' + val.nomeUtente + '</td>';
            tdRecupero = '<td>' + val.recupero + '</td>';
            tdDaRiportare = '<td>' + val.daRiportare + '</td>';
            tdAcconto = '<td>' + val.acconto+ '</td>';
            totAcconto += val.acconto.val();
            totRecupero += val.recupero.val();
            totDaRiportare += val.daRiportare.val();

            tdPdf = '<td></td>';
            stringaHtml += '<tr>' + tdStato + tdNome + tdAcconto + tdRecupero + tdDaRiportare + tdPdf + '</tr>';
            stringaHtml += '<tr><td colspan=100 cellaspcing=0 cellpadding=0> <div id="' + areaCollapse + '" class="collapse">';
            stringaHtml += '<table>';
            for (i = 1; i < 10; i++)
            {
                stringaHtml += '<tr>' +
                    '           <td colspan=2> &nbsp </td>' +
                    '           <td> 2.345,00 </td>' +
                    '           <td> 345,00 </td>' +
                    '           <td> 342.345,00 </td>' +
                    '           <td> &nbsp </td>';
            }
            stringaHtml +=  '</table>' +
                            '</div>' +
                            '</td></tr>';
                
        });
    }
    stringaHtml += '<th> </th>' +
        '<th> </th>' +
        '<th style="text-align: right"> € ' + totAcconto + '</div> </th>' +
        '<th style="text-align: right"> € ' + totRecupero + '</div> </th>' +
        '<th style="text-align: right"> € ' + totdaRiportare + '</div> </th>'

    $('#datiTabellaAnalisi').html(stringaHtml);
    $('#lblNumAgentiPerAnalisi').html("(record: " + data.length + ")");
};
