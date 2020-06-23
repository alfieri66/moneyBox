var infoCassa = new Array();
var infoLocali = new Array();
var infoLocaliCopia = new Array();
var costanti = new Array();

$(document).ready(function () {
    var dataOdierna = new Date();
    var tmpDataOdierna = dataOdierna.getFullYear() + '-' + ('0' + (dataOdierna.getMonth() + 1)).slice(-2) + '-' + ('0' + (dataOdierna.getDate())).slice(-2);
    var tmpDataIniziale = dataOdierna.getFullYear() + '-' + ('0' + (dataOdierna.getMonth() + 1)).slice(-2) + '-01';

    //costanti.pathWebServices = "https://localhost:44348/interrogaDB.asmx/";
    //costanti.pathWebServices = "https://www.dolcemare.eu/interrogaDB.asmx/";

    var livelloEsecuzione = 'L';
    switch (livelloEsecuzione) {
        case 'L':
            costanti.pathWebServices = "https://localhost:44348/interrogaDB.asmx/";;
            break
        case 'T':
            costanti.pathWebServices = "https://www.dolcemare.eu/interrogaDB.asmx/";
            break
        case 'W':
            costanti.pathWebServices = "https://www.moneysmart.cloud/interrogaDB.asmx/";
            break
    }

    $("#pagLogin").modal('hide');
    $("#pagCassa").hide();
    verificaLogin();
    $("#pagCassa").show();

    setTimeout(function () { $('#txtUnome').val(''); } , 10);

    $("#startDate").val(tmpDataIniziale);
    $("#endDate").val(tmpDataOdierna);
    $("#pagCassa").show(500);
    $("#pagUtenti").hide();
    $("#pagLocali").hide();
    $("#cambiaPassword").modal('hide');


});

function attivaArea(item)
{
    if (item == 'cassa')
    {
        $("#pagCassa").show(500);
        $("#pagUtenti").hide();
        $("#pagLocali").hide();
    }

    if (item == 'utenti') {
        $("#pagCassa").hide();
        $("#pagUtenti").show(500);
        $("#pagLocali").hide();
    }

    if (item == 'locali') {
        $("#pagCassa").hide();
        $("#pagUtenti").hide();
        $("#pagLocali").show(500);
    }

}
function logout() {
    $.ajax({
        url: costanti.pathWebServices + "logout",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: successoLogout,
        error: erroreLogout
    });
}

function successoLogout(msg) {
    attesa.style.display = "none";
    $("#pagLogin").modal('show');
}

function erroreLogout() {
    attesa.style.display = "none";
    $("#pagLogin").modal('show');
}


function verificaLogin() {
    var attesa = document.getElementById("attesa");
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "verificaLogin",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: successoLogin,
        error: erroreLogin
    });
};

function successoLogin(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();

    dati = JSON.parse(stringaJSON);
    if (dati.esito == false) {
        $("#pagLogin").modal('show');
    }
    else {
        attesa.style.display = "none";
    }
}

function erroreLogin() {
    $("#pagLogin").modal('show');
}


function copiaArray(sorgente, destinatario) {
    var voce = new Object();
    while (destinatario.length > 0) {
        destinatario.pop();
    }
    for (i = 0; i < sorgente.length; i++) {
        voce = Object.assign({},sorgente[i]);
        destinatario.push(voce);
    }
}

function copiaRigaArray(riga, sorgente, destinatario) {
    var voce = new Object();
    voce = Object.assign({}, sorgente[riga]);
    destinatario[riga]=voce;
}


function esisteUtente() {
    var dati = new Object();
    var stringaJSON;
    var msg;

    attesa.style.display = "block";
    dati.ruolo = "admin";
    dati.email = $("#inputEmailAddress").val();
    dati.password = $("#inputPassword").val();
    stringaJSON = JSON.stringify(dati)
    $.ajax({
        url: costanti.pathWebServices + "esisteUtente",
        type: "POST",
        data: stringaJSON,
        contentType: "application/json",
        dataType: "json",
        success: successoEsisteUtente,
        error: erroreEsisteUtente
    });
};

function successoEsisteUtente(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    dati = JSON.parse(stringaJSON);

    if (dati.esito == true) {
        $("#pagLogin").modal('hide');
        attesa.style.display = "none";
    }
    else {
        $("#lblRisultato").html("Login errato!");
    }
}

function erroreEsisteUtente() {
    attesa.style.display = "none";
    $("#lblRisultato").html("ops...connessione non disponibile!");
    //$("#lblRisultato").html(request.responseText);

}

function aggiornaUtente() {
    var dati = new Object();
    var stringaJSON;
    var msg;
    attesa.style.display = "block";
    dati.ruolo = "admin";
    dati.email = $("#chgEmailAddress").val();
    dati.password = $("#chgPassword").val();
    dati.nuovaEmail = $("#chgNuovaEmail").val();
    dati.nuovaPassword = $("#chgNuovaPassword").val();
    dati.nuovaRipetiPassword = $("#chgRipetiPassword").val();

    stringaJSON = JSON.stringify(dati)
    $.ajax({
        url: costanti.pathWebServices + "aggiornaUtente",
        type: "POST",
        data: stringaJSON,
        contentType: "application/json",
        dataType: "json",
        success: successoAggiornaUtente,
        error: erroreAggiornaUtente
    });
};

function successoAggiornaUtente(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    dati = JSON.parse(stringaJSON);
    attesa.style.display = "none";
    if (dati.esito == true) {
        pulisciForm("cambiaPassword");
        $('#panelCambiaPassword').modal('hide');
        $("#messaggio").html("Modifica effettuata con successo!");
        $('#panelMessaggio').modal('show');
    }
    else {
        $('#panelCambiaPassword').modal('hide');
        $("#messaggio").html("Errore! Qualcosa è andato storto");
        $('#panelMessaggio').modal('show');
    }
}

function erroreAggiornaUtente() {
    attesa.style.display = "none";
    $("#titolo").html("Accesso " + "<span style='color:red'> Errore!</span>");
}

function apriCambiaPassword() {
    $("#cambiaPassword").modal('show');
}


function copiaRigaArray(riga, sorgente, destinatario) {
    var voce = new Object();
    voce = Object.assign({}, sorgente[riga]);
    destinatario[riga] = voce;
}

function pulisciForm(nomeForm) {
    if (nomeForm == "cambiaPassword") {
        $("#chgNuovaEmail").val("");
        $("#chgPassword").val("");
        $("#chgNuovaPassword").val("");
        $("#chgRipetiPassword").val("");
    }

}


function btnDownloadApk() {
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "downloadApk",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: successoDownloadApk,
        error: erroreDownloadApk
    });
};

function successoDownloadApk(msg) {
    attesa.style.display = "none";
}

function erroreDownloadApk() {
    attesa.style.display = "none";
}

