var infoAgentiPerAnalisi = new Array();
var infoAgentiPerAnalisiCopia = new Array();

function btnLeggiAgentiPerAnalisi() {
    var info = new Object;
    info.dataIncasso = $("#dataAnalisi").val();
    datiJson = JSON.stringify(info);

    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "leggiAgentiPerAnalisi",
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
    totali = JSON.parse(dati.totali);
    dettaglio = JSON.parse(dati.dettaglio)
    if (totali.length > 0)
    {
       popolaTabellaAgentiPerAnalisi(dati.totAcconto, dati.totDaRiportare, dati.totRecupero, totali, dettaglio);
    }
}

function erroreLeggiAgentiPerAnalisi() {
    attesa.style.display = "none";
}



function popolaTabellaAgentiPerAnalisi(acconto, daRiportare, recupero, totali, dettaglio) {
    var tdStato = '';
    var tdNome = '';
    var tdRecupero = '';
    var tdDaRiportare = '';
    var tdAcconto = '';
    var tdPdf = '';
    var dataIncasso = $("#dataAnalisi").val();

    var stringaHtml = "";
    var riga = 0;

    if (totali.length > 0) {
        $.each(totali, function (key, val) {
            riga++;
            areaCollapse = "collapse_" + riga.toString();
            tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="Link"><i class="fas fa-external-link-alt" data-toggle="collapse" data-target="#' + areaCollapse +'")></i></td>';
            tdNome = '<td>' + val.nomeUtente +
                '<br>' +
                '<span class="text-success">Tot. cassa: ' + val.totCassa + "</span>" +
                '<br>' +
                '<span class="text-success">Flusso di cassa: ' + val.flussoCassa + "</span>" +
                '<br>' +
                '<span class="text-danger">Cassa Generale: ' + val.cassaGenerale + "</span>" +
                '</td>';
            tdRecupero = '<td style="text-align: right;">' + val.recupero + '</td>';
            tdDaRiportare = '<td style="text-align: right;">' + val.daRiportare + '</td>';
            tdAcconto = '<td style="text-align: right;">' + val.acconto+ '</td>';
            tdPdf = '<td><button type="button" class="btn btn-light"  onclick="btnDownloadPdfAgente(\'' + dataIncasso.toString() + '\', \'' + val.idAgente.toString() +'\');">PDF</button></td>';
            stringaHtml += '<tr>' + tdStato + tdNome + tdAcconto + tdRecupero + tdDaRiportare + tdPdf + '</tr>';
            stringaHtml += '<tr><td colspan=100 cellaspcing=0 cellpadding=0> <div id="' + areaCollapse + '" class="collapse">';
            if (dettaglio.length > 0) {
                stringaHtml += '<table>';
                $.each(dettaglio, function (key, valDet) {
                    if (val.idAgente == valDet.idAgente) {
                        stringaHtml += '<tr>' +
                            '           <td class="text-info">' + valDet.oraIncasso + '</td>' +
                            '           <td class="text-info">' + valDet.nomeLocale + '</td>' +
                            '           <td class="text-info">' + valDet.cittaLocale + '</td>'
                        if (valDet.acconto == "00,00" && valDet.recupero == "00,00" && valDet.daRiportare == "00,00") {
                            stringaHtml += '<td colspan=3 class="text-info" style = "text-align: left;" >** Operazione generica **</td>';
                        }
                        else
                        {
                            stringaHtml += '<td class="text-info" style = "text-align: right;" >' + valDet.acconto + '</td>' +
                                '           <td class="text-info" style = "text-align: right;" >' + valDet.recupero + '</td>' +
                                '           <td class="text-info" style = "text-align: right;" >' + valDet.daRiportare + '</td>';
                        }
                        stringaHtml += "</tr>";
                    }
                });
            stringaHtml += '</table>';
            }
            stringaHtml +=  '</div> </td></tr>';
        });
    }
    stringaHtml += '<th> </th>' +
        '<th> </th>' +
        '<th style="text-align: right"> € ' + acconto + '</div> </th>' +
        '<th style="text-align: right"> € ' + recupero + '</div> </th>' +
        '<th style="text-align: right"> € ' + daRiportare + '</div> </th>'

    $('#datiTabellaAnalisi').html(stringaHtml);
    $('#lblNumAgentiPerAnalisi').html("(record: " + totali.length + ")");
};

function btnDownloadPdfAgente(dataIncasso, idAgente) {
    var info = new Object;
    info.dataIncasso = dataIncasso;
    info.idAgente= idAgente;
    datiJson = JSON.stringify(info);
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "downloadPdfAgente",
        type: "POST",
        data: datiJson,
        contentType: "application/json",
        dataType: "json",
        success: successoDownloadPdfAgente,
        error: erroreDownloadPdfAgente
    });
};

function successoDownloadPdfAgente(msg) {
    var stringaJSON = msg.d;
    var link = document.createElement('a');
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    document.body.appendChild(link);
    link.href = dati.messaggio;
    link.target = "_blank";
    link.click();
}

function erroreDownloadPdfAgente() {
    attesa.style.display = "none";
}

