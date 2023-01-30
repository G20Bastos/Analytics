///Função responsável por exibir um popup centralizado e com boas dimensões
function popupwindow(url, title, w, h) {
    var left = (screen.width / 2) - (w / 2);
    var top = (screen.height / 2) - (h / 2);
    return window.open(url, title, "width=800,height=600,left=" + (screen.width - 800) / 2 + ",top=" + (screen.height - 700) / 2);

}

//Variavel global utilizada para identificar se houve submit em alguma página 
var ocorreuSubmit = false;

//Função que submete os dados e informa para variável "ocorreuSubmit" que houve submit e recarrega página anterior
$(document).ready(function () {
    $("a.submitForm").click(function () {
        $("form").submit();
        //opener.location.reload();
        ocorreuSubmit = true;
         
    });

});


//Função voltada para elementos da classe "submitFormWithoutReload"
//Realiza o submit sem recarregar a página que a chamou

$(document).ready(function () {
    $("a.submitFormWithoutReload").click(function () {
        $("form").submit();

    });

});




///Função JS que é chamada no unload da página. Fecha a janela atual e dá um refresh na página que a chamou.*@
function fecha() {

    // fechando a janela atual ( popup )
    window.close();
    //Refresh na página pai
    opener.location.reload();
    
}



///Função JS que é chamada no unload da página. Fecha a janela atual caso haja SUBMIT.*@

function fechaJanelaSeSubmit() {

    if (ocorreuSubmit == true) {
        opener.location.reload();
        window.close();
    }
   
}


///Função JS que permite "Selecionar Todos" em um checkboxlist.*@
$(document).ready(function () {

    $("#Selected").click(function () {  // Referenciando meu checkbox que marcará os demais

        if ($("#Selected").prop("checked"))   // se ela estiver marcada

            $(".chk").prop("checked", true);  // as que estiverem nessa classe ".chk" tambem serão marcadas
        else $(".chk").prop("checked", false);   // se não, elas tambem serão desmarcadas

    });
});



///Função JS que habilita seleção única a uma lista de checkbox

$(document).ready(function () {
    $('.checkSelecaoUnica').click(function () {
        $('.checkSelecaoUnica').not(this).prop('checked', false);
    });
});

///Função responsável por imprimir o conteudo da div em que o iframe do dashboard está inserido.
function printReport() {
    //var divContents = $("#reportContainer").html();
    var element = document.getElementById("reportContainer");
    var report = powerbi.get(element);

    report.print()
      .catch(error => { });
}
