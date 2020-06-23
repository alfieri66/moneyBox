var dimPagina = 10;
var numPagina = 0;
var totPagine = 0;
var infoCassa = "[]";

$(document).ready(function () {
    var dataOdierna = new Date();
    var tmp = dataOdierna.getFullYear() + '-' + ('0' + (dataOdierna.getMonth() + 1)).slice(-2) + '-' + ('0' + (dataOdierna.getDate())).slice(-2);
    $("#pagCassa").hide();
    verificaLogin();
    $("#pagCassa").show();

    $("#startDate").val("2020-01-01");
    $("#endDate").val(tmp);
});

function logout() {
    $.ajax({
        url: "https://localhost:44348/interrogaDB.asmx/logout",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: fSuccessoOut,
        error: fErroreOut
    });
}

function fSuccessoOut(msg) {
    attesa.style.display = "none";
    window.location = "login.html";
}

function fErroreOut() {
    attesa.style.display = "none";
    window.location = "login.html";
}


function verificaLogin() {
    var attesa = document.getElementById("attesa");
    attesa.style.display = "block";
    $.ajax({
        url: "https://localhost:44348/interrogaDB.asmx/verificaLogin",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: fSuccesso,
        error: fErrore
    });
};

function fSuccesso(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.esito == false) {
        window.location = "login.html";
    }
}

function fErrore() {
    window.location = "login.html";
}

function leggiDettagliCassa() {
    var cassa = new Object;
    cassa.dataIni = $("#startDate").val();
    cassa.dataFin = $("#endDate").val();
    cassa.idUtente = $("#txtUtente").val();
    cassa.idLocale = $("#txtLocale").val();
    attesa.style.display = "block";
    $.ajax({
        url: "https://localhost:44348/interrogaDB.asmx/leggiCassa",
        type: "POST",
        data: JSON.stringify(cassa),
        contentType: "application/json",
        dataType: "json",
        success: fSuccessoDC,
        error: fErroreDC
    });
};

function fSuccessoDC(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.dettaglio.length > 0) {
        infoCassa = dati.dettaglio;
        fpopolaTabella(dati.dettaglio);
    }
}

function fErroreDC() {
    attesa.style.display = "none";
}


function btnRicercaPerData() {
    numPagina = 0;
    leggiDettagliCassa();
}

function fpopolaTabella(data) {
    var tdCheck = '';
    var tdLocale = '';
    var tdUtente = '';
    var tdAcconto = '';
    var tdRecupero = '';
    var tddaRiportare = '';

    var stringaHtml = "";
    var totAcconto = 0;
    var totRecupero = 0;
    var totDaRiportare = 0;
    var totRighe = data.length;
    var riga = 0;
    if (totRighe > 0) {
        if (numPagina == 0)
            numPagina = 1;

        $.each(data, function (key, val) {

            totAcconto += parseFloat(val.acconto);
            totRecupero += parseFloat(val.recupero);
            totDaRiportare += parseFloat(val.daRiportare);
            riga++;
            if (riga >= ((numPagina - 1) * dimPagina + 1) && riga <= (numPagina * dimPagina)) {
                tdCheck = '<td>' + val.data + '</td>';
                tdLocale = '<td>' + val.nomeLocale + '</td>';
                tdUtente = '<td>' + val.nomeUtente + '</td>';
                tdAcconto = '<td style="text-align: right">' + val.acconto + '</td>';
                tdRecupero = '<td style="text-align: right">' + val.recupero + '</td>';
                tddaRiportare = '<td style="text-align: right">' + val.daRiportare + '</td>';
                stringaHtml += '<tr>' + tdCheck + tdLocale + tdUtente + tdAcconto + tdRecupero + tddaRiportare + '</tr>';
            }
        });
        $("#txtValAcconto").html(totAcconto.toFixed(2));
        $("#txtValRecupero").html(totRecupero.toFixed(2));
        $("#txtValDaRiportare").html(totDaRiportare.toFixed(2));
    }
    totPagine = parseInt(totRighe / dimPagina);
    if (totPagine * dimPagina != totRighe)
        totPagine++;
    $('#datiTabella').html(stringaHtml);
    $('#txtNumPagine').html("<i class='fas fa-table mr-1'> </i> Lista movimenti (pag. " + numPagina + " di " + totPagine + ")");

};

function cambiaPagina(avantiDietro) {
    if (avantiDietro == "+") {
        if (numPagina < totPagine && infoCassa.length > 2) {
            numPagina++;
            fpopolaTabella(infoCassa);
        }
    }
    if (avantiDietro == "-") {
        if (numPagina > 1 && infoCassa.length > 2) {
            numPagina--;
            fpopolaTabella(infoCassa);
        }
    }



};

