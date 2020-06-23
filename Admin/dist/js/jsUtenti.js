var infoUtenti = new Array();
var infoUtentiCopia = new Array();
var ck_password = /(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}/

function btnLeggiUtenti() {
    attesa.style.display = "block";
    $.ajax({
        url: costanti.pathWebServices + "leggiUtenti",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        success: successoLeggiUtenti,
        error: erroreLeggiUtenti
    });
};

function successoLeggiUtenti(msg) {
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.length > 0) {
        copiaArray(dati, infoUtenti);
        copiaArray(dati, infoUtentiCopia);
        popolaTabellaUtenti(infoUtenti);
    }
}

function erroreLeggiUtenti() {
    attesa.style.display = "none";
}



function popolaTabellaUtenti(data) {
    var tdStato = '';
    var tdNome = '';
    var tdCognome = '';
    var tdEmail = '';
    var tdAltro = '';
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
                        '<button type="button" onClick=annullaUtente("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                        '</td>';
                    tdNome = '<td style="color: #dc3545;"><del>' + val.nome + '</del></td>';
                    tdCognome = '<td style="color: #dc3545;"><del>'  + val.cognome + '</td>';
                    tdEmail = '<td style="color: #dc3545;"><del>'  + val.email + '</td>';
                    tdAltro = '<td style="color: #dc3545;"><del><i class="far fa-eye-slash"></i>'  + '</td>';
                    break
                case 'X':
                    // Record originale
                    tdStato = '<<td data-toggle="tooltip" data-placement="bottom" title="Originale"><i class="far fa-check-square"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=editUtente("' + (riga - 1) + '") class="btn btn-light">Edit</button>&nbsp;' + 
                        '<button type="button" onClick=eliminaUtente("' + (riga - 1) + '") class="btn btn-light">Cancella</button>&nbsp;' + 
                        '</td>';
                    tdNome = '<td>' + val.nome + '</td>';
                    tdCognome = '<td>' + val.cognome + '</td>';
                    tdEmail = '<td>' + val.email + '</td>';
                    tdAltro = '<td>&nbsp;<i class="far fa-eye-slash">'  + '</td>';
                    break
                case 'M':
                    // record modificato
                    tdStato = '<<td data-toggle="tooltip" data-placement="bottom" title="Modificato"><i class="far fa-keyboard"></i>' + '</td>';
                    tdConferma = '<td>' +
                        '<button type="button" onClick=editUtente("' + (riga - 1) + '") class="btn btn-light">Edit</button>&nbsp;' +
                        '<button type="button" onClick=annullaUtente("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                        '</td>';
                    tdNome = '<td>' + val.nome + '</td>';
                    tdCognome = '<td>' + val.cognome + '</td>';
                    tdEmail = '<td>' + val.email + '</td>';
                    tdAltro = '<td>&nbsp;<i class="far fa-eye-slash"></i>'  + '</td>';
                    break
                case 'E':
                    // record in stato Editing
                    tdStato = '<td data-toggle="tooltip" data-placement="bottom" title="In corso di modifica"><i class="fas fa-pencil-alt"></i>' + '</td>';
                    tdConferma =    '<td>' +
                                        '<button type="button" onClick=salvaUtente("' + (riga - 1) + '") class="btn btn-light">Salva</button>&nbsp;' +
                                        '<button type="button" onClick=annullaUtente("' + (riga - 1) + '") class="btn btn-light">Annulla</button>&nbsp;' +
                                    '</td>';
                    tdNome = '<td><input type="text" id="inNomeLoc" name="inNomeLoc" placeholder="Nome" value="' + val.nome + '"</td>';
                    tdCognome = '<td><input type="text" id="inCognomeLoc" name="inCognomeLoc" placeholder="Cognome" value="' + val.cognome + '"</td>';
                    tdEmail = '<td><input type="text" id="inEmailUtente" name="inEmailUtente" placeholder="email" value="' + val.email + '"</td>';
                    tdAltro = '<td> <table style="border: 0px;"> ';
                    tdAltro += '        <tr>';
                    tdAltro += '            <td style="padding: 0px;">&nbsp;<input type="password" id="inPasswordUtente" name="inPasswordUtente" title="almeno 8 caratteri, un numero, un carattere maiuscolo e uno minuscolo"  placeholder="password" value=""</td>';
                    tdAltro += '        </tr>';
                    tdAltro += '        <tr>';
                    tdAltro += '            <td style="padding: 5px 0px 0px 0px;">&nbsp;<input type="password" id="inPasswordUtenteRip" name="inPasswordUtenteRip" placeholder="ripeti password" value=""</td>';
                    tdAltro += '        </tr>';
                    tdAltro += '    </table>';
                    tdAltro += '</td>';
                    break
            }

            stringaHtml += '<tr>' + tdStato + tdNome + tdCognome + tdEmail + tdAltro + tdConferma + '</tr>';
        });
    }
    $('#datiTabellaUtenti').html(stringaHtml);
    $('#lblNumUtenti').html("(record: " + data.length + ")");
};

function salvaUtente(riga) {
    correggere = false;
    if (riga < infoUtenti.length) {

        if ($('#inNomeLoc').val().trim() != "") {
            infoUtenti[riga].stato = "M";
            infoUtenti[riga].nome = $('#inNomeLoc').val().trim();
            infoUtenti[riga].cognome = $('#inCognomeLoc').val().trim();
            infoUtenti[riga].email = $('#inEmailUtente').val().trim();
            infoUtenti[riga].password = $('#inPasswordUtente').val().trim();
            infoUtenti[riga].ripetiPassword = $('#inPasswordUtenteRip').val().trim();

            if (infoUtenti[riga].nome.trim() != "" && infoUtenti[riga].email.trim() != "") {
                if (infoUtenti[riga].idUtente == 0) {
                    if (infoUtenti[riga].password == infoUtenti[riga].ripetiPassword && ck_password.test(infoUtenti[riga].password)) {
                        ripulisciListaUtenti();
                        popolaTabellaUtenti(infoUtenti);
                    }
                    else {
                        correggere = true;
                    }

                }
                else {
                    if ((infoUtenti[riga].password == infoUtenti[riga].ripetiPassword && ck_password.test(infoUtenti[riga].password)) || (infoUtenti[riga].password.trim() == "" && infoUtenti[riga].ripetiPassword.trim() == "")) {
                        ripulisciListaUtenti();
                        popolaTabellaUtenti(infoUtenti);
                    }
                    else {
                        correggere = true;
                    }
                }
            }
            else {
                correggere = true;
            }
        }
        else {
            correggere = true;
        }
        if (correggere)
        {
            txtMessaggio = "Verifica e correggi:" +
                "<ul>" +
                "   <li> il nome dell'utente " +
                "   <li> la casella di posta elettronica " +
                "   <li> la password (almeno 8 caratteri, 1 numero, 1 carattere maiuscolo, 1 carattere minuscolo) " +
                "</ul>";
            $("#intestazioneMessaggio").html("Attenzione!");
            $("#messaggio").html(txtMessaggio);
            $('#panelMessaggio').modal('show');
        }

    }
}

function editUtente(riga) {
    for (i = 0; i < infoUtenti.length; i++) {
        if (infoUtenti[i].stato == "E")
            copiaRigaArray(i, infoUtentiCopia, infoUtenti);
    }
    infoUtenti[riga].stato = "E";
    ripulisciListaUtenti();
    popolaTabellaUtenti(infoUtenti);
}


function eliminaUtente(riga) {
    infoUtenti[riga].stato = "D";
    ripulisciListaUtenti();
    popolaTabellaUtenti(infoUtenti);
}

function annullaUtente(riga) {
    if (riga < infoUtenti.length) {
        copiaRigaArray(riga, infoUtentiCopia, infoUtenti);
    }
    ripulisciListaUtenti();
    popolaTabellaUtenti(infoUtenti);
}

function ripulisciListaUtenti() {
    i = 0;
    while (i < infoUtenti.length) {
        if (infoUtenti[i].email.trim() == "" || infoUtenti[i].nome.trim() == "") {
            infoUtenti.splice(i, 1);
            infoUtentiCopia.splice(i, 1);
        }
        else
            i++;
    }
};

function aggiungiRigaUtente()
{
    var locale = Object();

    locale.stato = "E";
    locale.idUtente = 0;
    locale.nome = "";
    locale.cognome = "";
    locale.email = "";
    locale.password = "";

    ripulisciListaUtenti();

    infoUtenti.unshift(Object.assign({}, locale))
    infoUtentiCopia.unshift(Object.assign({}, locale));
    popolaTabellaUtenti(infoUtenti);
}

function salvaModificheUtenti() {
    var infoUtentiTmp = [];
    var info = new Object;
    attesa.style.display = "block";
    
    infoUtenti.forEach(function (el) {
        if (el.stato != "X" && el.nome.trim() !== "")
            infoUtentiTmp.push(Object.assign({}, el));
    });

    //datiJson = JSON.stringify(infoUtentiTmp);
    info.utenti = infoUtentiTmp;
    datiJson = JSON.stringify(info);

    $.ajax({
        url: costanti.pathWebServices + "salvaUtenti",
        type: "POST",
        data: datiJson,
        contentType: "application/json",
        dataType: "json",
        success: successoSalvaModificheUtenti,
        error: erroreSalvaModificheUtenti
    });

}

function successoSalvaModificheUtenti(msg){
    var stringaJSON = msg.d;
    var dati = new Object();
    attesa.style.display = "none";
    dati = JSON.parse(stringaJSON);
    if (dati.length > 0) {
        copiaArray(dati, infoUtenti);
        copiaArray(dati, infoUtentiCopia);
        popolaTabellaUtenti(infoUtenti);
    }
}

function erroreSalvaModificheUtenti() {
    attesa.style.display = "none";
    alert("errore");
}

function btnRicercaCassa() {
    numPagina = 0;
    leggiDettagliCassa();
}
