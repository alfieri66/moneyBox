var dimPagina = 10;
var numPagina = 0;
var totPagine = 0;
var mostraTutto = false;
var infoCassa = new Array();
var infoCassaTmp = new Array(); 

function btnInviaMail() {
    var dati = new Object();
    dati.dataIni = $("#startDate").val();
    dati.dataFin = $("#endDate").val();
    dati.utente = $("#txtUnome").val() + ' ' + $("#txtUcognome").val()  ;
    dati.locale = $("#txtLocale").val();
    dati.cassa = infoCassa;

    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "inviaMail",
        type: "POST",
        data: JSON.stringify(dati),
        contentType: "application/json",
        dataType: "json",
        success: successoInviaMail,
        error: erroreInviaMail
    })
}

function successoInviaMail(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    $("#messaggio").html(dati.messaggio);
    $('#panelMessaggio').modal('show');

}

function erroreInviaMail(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    $("#lblMessaggio").html(dati.messaggio);
}

function leggiDettagliCassa() {
    var cassa = new Object();
    cassa.dataIni = $("#startDate").val();
    cassa.dataFin = $("#endDate").val();
    cassa.utenteNome = $("#txtUnome").val();
    cassa.utenteCognome = $("#txtUcognome").val();
    cassa.locale = $("#txtLocale").val();
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "leggiCassa",
        type: "POST",
        data: JSON.stringify(cassa),
        contentType: "application/json",
        dataType: "json",
        success: successoLeggiDettagliCassa,
        error: erroreLeggiDettagliCassa
    });
};

function successoLeggiDettagliCassa(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    copiaArray(dati.dettaglio, infoCassa);
    popolaTabellaCassa(infoCassa, dati.totAcconto, dati.totRecupero, dati.totDaRiportare);
}

function erroreLeggiDettagliCassa() {
    attesa.style.display = "none";
}

function popolaTabellaCassa(data, totAcconto, totRecupero, totDaRiportare) {
    var tdStato = '';
    var tdData = '';
    var tdLocale = '';
    var tdUtente = '';
    var tdAcconto = '';
    var tdRecupero = '';
    var tddaRiportare = '';
    var tdConferma = '';

    var stringaHtml = "";
    var totRighe = data.length;
    var riga = 0;
    if (totRighe > 0) {
        if (numPagina == 0)
            numPagina = 1;

        $.each(data, function (key, val) {
            riga++;
            if ((riga >= ((numPagina - 1) * dimPagina + 1) && riga <= (numPagina * dimPagina) && mostraTutto == false) || (mostraTutto == true)) {
                switch (val.stato) {
                    case 'X':
                        {
                            tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="Originale"><i class="far fa-check-square"></i></td>';

                            tdData = '<td>' + val.data + '</td>';
                            tdLocale = '<td>' + val.nomeLocale + '</td>';
                            tdUtente = '<td>' + val.nomeUtente + '</td>';
                            tdAcconto = '<td style="text-align: right">' + val.acconto + '</td>';
                            tdRecupero = '<td style="text-align: right">' + val.recupero + '</td>';
                            tddaRiportare = '<td style="text-align: right">' + val.daRiportare + '</td>';

                            tdConferma = '<td>' +
                                '<button type="button" onClick=eliminaCassa("' + (riga - 1) + '") class="btn btn-light">Cancella</button>&nbsp;' +
                                '</td>';
                            break
                        }
                    case 'D':
                        {
                            tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="Originale"><i class="far fa-trash-alt"></i></td>';

                            tdData = '<td style="color: #dc3545;"><del>' + val.data + '</del></td>';
                            tdLocale = '<td style="color: #dc3545;"><del>' + val.nomeLocale + '</del></td>';
                            tdUtente = '<td style="color: #dc3545;"><del>' + val.nomeUtente + '</del></td>';
                            tdAcconto = '<td style="color: #dc3545; text-align: right"><del>' + val.acconto + '</del></td>';
                            tdRecupero = '<td style="color: #dc3545; text-align: right"><del>' + val.recupero + '</del></td>';
                            tddaRiportare = '<td style="color: #dc3545; text-align: right"><del>' + val.daRiportare + '</del></td>';

                            tdConferma = '<td>' +
                                '<button type="button" onClick=annullaCassa("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                                '</td>';
                            break
                        }

                }

                stringaHtml += '<tr>' + tdStato + tdData + tdLocale + tdUtente + tdAcconto + tdRecupero + tddaRiportare + tdConferma + '</tr>';
            }
        });

    }
    $("#txtValAcconto").html(totAcconto);
    $("#txtValRecupero").html(totRecupero);
    $("#txtValDaRiportare").html(totDaRiportare);

    totPagine = parseInt(totRighe / dimPagina);
    if (totPagine * dimPagina != totRighe)
        totPagine++;
    $('#datiTabella').html(stringaHtml);
    if (mostraTutto == true)
    {
        $('#txtNumPagine').html("(record: " + data.length + ")");
    }
    else
    {
        $('#txtNumPagine').html("(pag. " + numPagina + " di " + totPagine + ")");
    }
};

function eliminaCassa(riga) {
    infoCassa[riga].stato = "D";
    popolaTabellaCassa(infoCassa);
}

function annullaCassa(riga) {
    infoCassa[riga].stato = "X";
    popolaTabellaCassa(infoCassa);
}


function cambiaPagina(avantiDietro) {
    if (avantiDietro == "+") {
        mostraTutto = false;
        if (numPagina < totPagine && infoCassa.length > 2) {
            numPagina++;
            popolaTabellaCassa(infoCassa);
        }
    }
    if (avantiDietro == "-") {
        mostraTutto = false;
        if (numPagina > 1 && infoCassa.length > 2) {
            numPagina--;
            popolaTabellaCassa(infoCassa);
        }
    }
    if (avantiDietro == "*") {
        mostraTutto = true;
        if (infoCassa.length > 0) {
            popolaTabellaCassa(infoCassa)
        };
    }
};

function btnRicercaCassa() {
    numPagina = 0;
    leggiDettagliCassa();
}


function salvaModificheCassa() {
    var infoCassaTmp = [];
    var info = new Object;
    attesa.style.display = "block";

    infoCassa.forEach(function (el) {
        if (el.stato == "D")
            infoCassaTmp.push(Object.assign({}, el));
    });

    info.cassa = infoCassaTmp;
    datiJson = JSON.stringify(info);

    $.ajax({
        url: costanti.pathWebServices + "rimuoviOperazioni",
        type: "POST",
        data: datiJson,
        contentType: "application/json",
        dataType: "json",
        success: successoSalvaModificheCassa,
        error: erroreSalvaModificheCassa
    });

}

function successoSalvaModificheCassa(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    $("#messaggio").html(dati.messaggio);
    $('#panelMessaggio').modal('show');
    leggiDettagliCassa();
}

function erroreSalvaModificheCassa() {
    attesa.style.display = "none";
    alert("errore");
}


