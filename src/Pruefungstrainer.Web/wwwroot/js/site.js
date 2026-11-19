// Kleine Frontend-Interaktionen des Prüfungstrainers.
// Alles hängt an data-Attributen im HTML, damit die Views selbst kein JavaScript enthalten.

document.addEventListener("DOMContentLoaded", function () {

    // 1. Sicherheitsabfrage vor dem Absenden, z.B. beim Löschen:
    //    <form ... data-bestaetigen="Wirklich löschen?">
    document.querySelectorAll("form[data-bestaetigen]").forEach(function (formular) {
        formular.addEventListener("submit", function (ereignis) {
            if (!confirm(formular.dataset.bestaetigen)) {
                ereignis.preventDefault();
            }
        });
    });

    // 2. Frage-Formular: je nach Fragetyp nur die passenden Felder zeigen.
    //    <select data-fragetyp-auswahl> und Bereiche mit data-nur-bei-typ="MultipleChoice" / "Freitext"
    var typAuswahl = document.querySelector("[data-fragetyp-auswahl]");
    if (typAuswahl) {
        var bereicheAktualisieren = function () {
            document.querySelectorAll("[data-nur-bei-typ]").forEach(function (bereich) {
                bereich.hidden = bereich.dataset.nurBeiTyp !== typAuswahl.value;
            });
        };
        typAuswahl.addEventListener("change", bereicheAktualisieren);
        bereicheAktualisieren();
    }

    // 3. Prüfungssimulation: Countdown anzeigen und bei Ablauf automatisch abgeben.
    //    <span data-endet-um="2026-11-19T10:30:00Z" data-formular="quizFormular">
    var timer = document.querySelector("[data-endet-um]");
    if (timer) {
        var endetUm = new Date(timer.dataset.endetUm).getTime();
        var formular = document.getElementById(timer.dataset.formular);
        var abgegeben = false;

        var aktualisieren = function () {
            var restSekunden = Math.max(0, Math.floor((endetUm - Date.now()) / 1000));
            var minuten = Math.floor(restSekunden / 60);
            var sekunden = restSekunden % 60;
            timer.textContent = minuten + ":" + String(sekunden).padStart(2, "0");

            if (restSekunden <= 60) {
                timer.classList.add("text-danger");
            }

            if (restSekunden === 0 && !abgegeben && formular) {
                abgegeben = true;
                formular.submit();
            }
        };

        aktualisieren();
        setInterval(aktualisieren, 1000);
    }
});
