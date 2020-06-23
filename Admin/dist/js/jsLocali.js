var infoLocali = new Array();
var infoLocaliCopia = new Array();

function btnLeggiLocali() {
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "leggiLocali",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: successoLeggiLocali,
        error: erroreLeggiLocali
    });
};

function successoLeggiLocali(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.length > 0) {
        copiaArray(dati, infoLocali);
        copiaArray(dati, infoLocaliCopia);
        popolaTabellaLocali(infoLocali);
    }
}

function erroreLeggiLocali() {
    attesa.style.display = "none";
}

function copiaRigaArray(riga, sorgente, destinatario) {
    var voce = new Object();
    voce = Object.assign({}, sorgente[riga]);
    destinatario[riga]=voce;
}

function popolaTabellaLocali(data) {
    var tdStato = '';
    var tdNome = '';
    var tdindirizzo = '';
    var tdCitta = '';
    var tdTel = '';
    var tdConferma = '';

    var stringaHtml = "";
    var totRighe = data.length;
    var riga = 0;
    if (totRighe > 0) {
        $.each(data, function (key, val) {
            riga++;
            switch (val.stato) {
                case 'D':
                    // Record eliminato
                    tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="Eliminato"><i class="far fa-trash-alt"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=annullaLocale("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                        '</td>';
                    tdNome = '<td style="color: #dc3545;"><del>' + val.nome + '</del></td>';
                    tdindirizzo = '<td style="color: #dc3545;"><del>'  + val.indirizzo + '</td>';
                    tdCitta = '<td style="color: #dc3545;"><del>'  + val.citta + '</td>';
                    tdTel = '<td style="color: #dc3545;"><del>'  + val.tel + '</td>';
                    break
                case 'X':
                    // Record originale
                    tdStato = '<<td data-toggle="tooltip" data-placement="bottom" title="Originale"><i class="far fa-check-square"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=editLocale("' + (riga - 1) + '") class="btn btn-light">Edit</button>&nbsp;' + 
                        '<button type="button" onClick=eliminaLocale("' + (riga - 1) + '") class="btn btn-light">Cancella</button>&nbsp;' + 
                        '<button type="button" onClick=generaDocLocale("' + (riga - 1) + '") class="btn btn-info"><i class="fas fa-download"></i>&nbsp; Doc </button>&nbsp;' + 
                        '</td>';
                    tdNome = '<td>' + val.nome +
                             '<br/>(' + val.codiceLocale + ")" + '</td>';
                    tdindirizzo = '<td>' + val.indirizzo + '</td>';
                    tdCitta = '<td>' + val.citta + '</td>';
                    tdTel = '<td>&nbsp;' + val.tel + '</td>';
                    break
                case 'M':
                    // record modificato
                    tdStato = '<<td data-toggle="tooltip" data-placement="bottom" title="Modificato"><i class="far fa-keyboard"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=editLocale("' + (riga - 1) + '") class="btn btn-light">Edit</button>&nbsp;' +
                        '<button type="button" onClick=annullaLocale("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                        '</td>';
                    tdNome = '<td>' + val.nome + '</td>';
                    tdindirizzo = '<td>' + val.indirizzo + '</td>';
                    tdCitta = '<td>' + val.citta + '</td>';
                    tdTel = '<td>&nbsp;' + val.tel + '</td>';
                    break
                case 'E':
                    // record in stato Editing
                    tdStato = '<<td data-toggle="tooltip" data-placement="bottom" title="In corso di modifica"><i class="fas fa-pencil-alt"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=salvaLocale("' + (riga - 1) + '") class="btn btn-light">Salva</button>&nbsp;' +
                        '<button type="button" onClick=annullaLocale("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                        '</td>';
                    tdNome = '<td><input type="text" id="inNomeLoc" name="inNomeLoc" placeholder="Nome" value="' + val.nome + '"</td>';
                    tdindirizzo = '<td><input type="text" id="inIndirizzoLoc" name="inIndirizzoLoc" placeholder="Indirizzo" value="' + val.indirizzo + '"</td>';
                    tdCitta = '<td><input type="text" id="inCittaLoc" name="inCittaLoc" placeholder="Citt&agrave" value="' + val.citta + '"</td>';
                    tdTel = '<td>&nbsp;<input type="tel" id="inTelLoc" name="inTelLoc" placeholder="Telefono" value="' + val.tel + '"</td>';
                    break
            }

            stringaHtml += '<tr>' + tdStato + tdNome + tdindirizzo + tdCitta + tdTel + tdConferma + '</tr>';
        });
    }
    $('#datiTabellaLocali').html(stringaHtml);
    $('#lblNumLocali').html("(record: " + data.length + ")");
};

function salvaLocale(riga) {
    if (riga < infoLocali.length) {

        if ($('#inNomeLoc').val().trim() != "") {
            infoLocali[riga].stato = "M";
            infoLocali[riga].nome = $('#inNomeLoc').val().trim();
            infoLocali[riga].indirizzo = $('#inIndirizzoLoc').val().trim();
            infoLocali[riga].citta = $('#inCittaLoc').val().trim();
            infoLocali[riga].tel = $('#inTelLoc').val().trim();
            ripulisciListaLocali(); 
            popolaTabellaLocali(infoLocali);
        }
    }
}

function editLocale(riga) {
    for (i = 0; i < infoLocali.length; i++) {
        if (infoLocali[i].stato == "E")
            copiaRigaArray(i, infoLocaliCopia, infoLocali);
    }
    infoLocali[riga].stato = "E";
    ripulisciListaLocali();
    popolaTabellaLocali(infoLocali);
}

function generaDocLocale(r) {
    var infoDoc = new Object();
    attesa.style.display = "block";

    infoDoc.codiceLocale = infoLocali[r].codiceLocale;
    infoDoc.nome = infoLocali[r].nome;
    infoDoc.indirizzo = infoLocali[r].indirizzo
    infoDoc.citta = infoLocali[r].citta;
    infoDoc.tel = infoLocali[r].tel; 

    datiJson = JSON.stringify(infoDoc);

    $.ajax({
            url: costanti.pathWebServices + "generaDocLocale",
            type: "POST",
            data: datiJson,
            contentType: "application/json",
            dataType: "json",
            success: successoGeneraDocLocale,
            error: erroreGeneraDocLocale
        });

}

function successoGeneraDocLocale(msg) {
    var stringaJSON = msg.d;
    var link = document.createElement('a');
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);

    document.body.appendChild(link);
    link.href = dati.messaggio;
    link.click();
}

function erroreGeneraDocLocale() {
    attesa.style.display = "none";
    alert("errore");
}


function eliminaLocale(riga) {
    infoLocali[riga].stato = "D";
    ripulisciListaLocali();
    popolaTabellaLocali(infoLocali);
}

function annullaLocale(riga) {
    if (riga < infoLocali.length) {
        copiaRigaArray(riga, infoLocaliCopia, infoLocali);
    }
    ripulisciListaLocali();
    popolaTabellaLocali(infoLocali);
}

function ripulisciListaLocali() {
    i = 0;
    while (i < infoLocali.length) {
        if (infoLocali[i].nome.trim() == "") {
            infoLocali.splice(i, 1);
            infoLocaliCopia.splice(i, 1);
        }
        else
            i++;
    }
};

function aggiungiRigaLocale()
{
    var locale = Object();

    locale.stato = "E";
    locale.idLocale = 0;
    locale.nome = "";
    locale.indirizzo = "";
    locale.tel = "";
    locale.citta = "";
    locale.codiceLocale = 0;

    ripulisciListaLocali();

    infoLocali.unshift(Object.assign({}, locale))
    infoLocaliCopia.unshift(Object.assign({}, locale));
    popolaTabellaLocali(infoLocali);
}

function salvaModificheLocali() {
    var infoLocaliTmp = [];
    var info = new Object;
    attesa.style.display = "block";
    
    infoLocali.forEach(function (el) {
        if (el.stato != "X" && el.nome.trim() !== "")
            infoLocaliTmp.push(Object.assign({}, el));
    });

    //datiJson = JSON.stringify(infoLocaliTmp);
    info.locali = infoLocaliTmp;
    datiJson = JSON.stringify(info);

    $.ajax({
        url: costanti.pathWebServices + "salvaLocali",
        type: "POST",
        data: datiJson,
        contentType: "application/json",
        dataType: "json",
        success: successoSalvaModificheLocali,
        error: erroreSalvaModificheLocali
    });

}

function successoSalvaModificheLocali(msg){
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.length > 0) {
        copiaArray(dati, infoLocali);
        copiaArray(dati, infoLocaliCopia);
        popolaTabellaLocali(infoLocali);
    }
}

function erroreSalvaModificheLocali() {
    attesa.style.display = "none";
    alert("errore");
}


